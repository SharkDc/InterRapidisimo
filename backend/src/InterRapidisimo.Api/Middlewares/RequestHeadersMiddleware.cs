using System.Net;
using System.Text.Json;
using InterRapidisimo.Application.Common.Models;

namespace InterRapidisimo.Api.Middlewares;

/// <summary>
/// Middleware para la captura, validación estricta y trazabilidad de cabeceras de auditoría obligatorias:
/// systemid, uuid y timestamp.
/// </summary>
public class RequestHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestHeadersMiddleware> _logger;

    public const string SystemIdHeader = "systemid";
    public const string UuidHeader = "uuid";
    public const string TimestampHeader = "timestamp";

    public RequestHeadersMiddleware(RequestDelegate next, ILogger<RequestHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Omitir validación estricta en peticiones OPTIONS (CORS preflight)
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // 2. Si no es un endpoint de API (ej. /swagger, archivos estáticos, favicon), permitir paso
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 3. Extraer y validar cabecera obligatoria 'systemid'
        string? systemId = context.Request.Headers[SystemIdHeader].FirstOrDefault()
            ?? context.Request.Headers["X-System-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(systemId))
        {
            await WriteErrorResponseAsync(context, "La cabecera obligatoria 'systemid' no fue proporcionada o está vacía.", null, null, null);
            return;
        }

        // 4. Extraer y validar cabecera obligatoria 'uuid' (UUIDv4/GUID de correlación)
        string? uuid = context.Request.Headers[UuidHeader].FirstOrDefault()
            ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(uuid))
        {
            await WriteErrorResponseAsync(context, "La cabecera obligatoria 'uuid' no fue proporcionada o está vacía.", systemId, null, null);
            return;
        }

        if (!Guid.TryParse(uuid, out _))
        {
            await WriteErrorResponseAsync(context, "La cabecera obligatoria 'uuid' no tiene un formato UUID/GUID válido (ej. a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d).", systemId, null, null);
            return;
        }

        // 5. Extraer y validar cabecera obligatoria 'timestamp' (formato ISO-8601)
        string? timestamp = context.Request.Headers[TimestampHeader].FirstOrDefault()
            ?? context.Request.Headers["X-Timestamp"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(timestamp))
        {
            await WriteErrorResponseAsync(context, "La cabecera obligatoria 'timestamp' no fue proporcionada o está vacía.", systemId, uuid, null);
            return;
        }

        if (!DateTimeOffset.TryParse(timestamp, out _))
        {
            await WriteErrorResponseAsync(context, "La cabecera obligatoria 'timestamp' no tiene un formato de fecha/hora ISO-8601 UTC válido (ej. 2026-09-05T15:30:00.000Z).", systemId, uuid, null);
            return;
        }

        // 6. Inyectar en el contexto de la solicitud para uso en controladores y handlers
        context.Items["SystemId"] = systemId;
        context.Items["CorrelationId"] = uuid;
        context.Items["Timestamp"] = timestamp;

        // 7. Inyectar cabeceras en la respuesta HTTP para trazabilidad bidireccional
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("X-System-Id"))
                context.Response.Headers.Append("X-System-Id", systemId);

            if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
                context.Response.Headers.Append("X-Correlation-Id", uuid);

            if (!context.Response.Headers.ContainsKey("X-Timestamp"))
                context.Response.Headers.Append("X-Timestamp", timestamp);

            return Task.CompletedTask;
        });

        // 8. Logging estructurado con alcance de correlación
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["SystemId"] = systemId,
            ["CorrelationId"] = uuid,
            ["Timestamp"] = timestamp
        }))
        {
            _logger.LogInformation("Petición entrante validada HTTP {Method} {Path} | SystemId: {SystemId} | Uuid: {Uuid}",
                context.Request.Method, context.Request.Path, systemId, uuid);

            await _next(context);
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        string descripcion,
        string? systemId,
        string? uuid,
        string? timestamp)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        if (!string.IsNullOrWhiteSpace(systemId))
            context.Response.Headers["X-System-Id"] = systemId;
        if (!string.IsNullOrWhiteSpace(uuid))
            context.Response.Headers["X-Correlation-Id"] = uuid;
        if (!string.IsNullOrWhiteSpace(timestamp))
            context.Response.Headers["X-Timestamp"] = timestamp;

        var response = ApiResponse<object>.Failure(descripcion);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(json);
    }
}
