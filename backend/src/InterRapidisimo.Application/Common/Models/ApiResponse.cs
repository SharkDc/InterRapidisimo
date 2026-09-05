namespace InterRapidisimo.Application.Common.Models;

/// <summary>
/// Envoltorio estandarizado de respuesta para la Web API.
/// </summary>
/// <typeparam name="T">Tipo del contenido de datos retornado.</typeparam>
public class ApiResponse<T>
{
    public bool Estado { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public T? Data { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(bool estado, string descripcion, T? data = default)
    {
        Estado = estado;
        Descripcion = descripcion;
        Data = data;
    }

    public static ApiResponse<T> Success(T data, string descripcion = "Operación completada exitosamente.")
    {
        return new ApiResponse<T>(true, descripcion, data);
    }

    public static ApiResponse<T> Failure(string descripcion)
    {
        return new ApiResponse<T>(false, descripcion, default);
    }
}

/// <summary>
/// Clase auxiliar para crear respuestas estandarizadas no genéricas o sin payload.
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string descripcion = "Operación completada exitosamente.")
    {
        return ApiResponse<T>.Success(data, descripcion);
    }

    public static ApiResponse<object?> Success(string descripcion = "Operación completada exitosamente.")
    {
        return new ApiResponse<object?>(true, descripcion, null);
    }

    public static ApiResponse<object?> Failure(string descripcion)
    {
        return ApiResponse<object?>.Failure(descripcion);
    }
}
