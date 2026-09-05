using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InterRapidisimo.Api.Swagger;

/// <summary>
/// Filtro de operaciones para Swagger/OpenAPI que declara las cabeceras obligatorias de trazabilidad
/// (systemid, uuid, timestamp) en todos los endpoints de la API.
/// </summary>
public class RequiredHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "systemid",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Identificador del canal o sistema cliente emisor (ej. 'inter-rapidisimo-web').",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "uuid",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Identificador único de correlación y trazabilidad distribuida en formato UUID/GUID.",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid"
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "timestamp",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Marca de tiempo UTC en formato ISO-8601 (ej. '2026-09-05T15:30:00.000Z').",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "date-time"
            }
        });
    }
}
