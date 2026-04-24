namespace BA.Backend.Application.Common.Models;

public class ApiResponse<T>
{
    /// <example>true</example>
    public bool Success { get; set; }

    /// <example>Operación completada exitosamente</example>
    public string? Message { get; set; }

    public T? Data { get; set; }

    public List<string> Errors { get; set; } = new();

    public ApiResponseMeta? Meta { get; set; }

    public static ApiResponse<T> SuccessResponse(T? data, string? message = null, ApiResponseMeta? meta = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        Meta = meta
    };

    public static ApiResponse<T> FailureResponse(IEnumerable<string> errors) => new()
    {
        Success = false,
        Errors = errors.ToList(),
        Message = errors.FirstOrDefault()
    };

    public static ApiResponse<T> FailureResponse(string error, string? errorCode = null) => new()
    {
        Success = false,
        Message = error,
        Errors = new List<string> { error }
    };
}

public class ApiResponseMeta
{
    /// <example>1</example>
    public int Page { get; set; }

    /// <example>10</example>
    public int PageSize { get; set; }

    /// <example>100</example>
    public int TotalCount { get; set; }

    /// <example>10</example>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
