using System.Net;
using System.Text.Json;
using BA.Backend.Application.Exceptions;
using BA.Backend.Application.Common.Models;
using BA.Backend.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BA.Backend.WebAPI.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log estructurado: Serilog capturará el contexto del LogContextMiddleware
            _logger.LogError(ex, "Excepción no controlada en {Path} [{Method}]. Mensaje: {Message}",
                context.Request.Path, context.Request.Method, ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        ApiResponse<object> response;

        switch (ex)
        {
            case DomainException domainEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse<object>.FailureResponse($"[{domainEx.Code}] {domainEx.Message}");
                break;

            case InvalidCredentialsException invalidCreds:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = ApiResponse<object>.FailureResponse(invalidCreds.Message);
                break;

            case UnauthorizedAccessException unauthorizedEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = ApiResponse<object>.FailureResponse(
                    string.IsNullOrWhiteSpace(unauthorizedEx.Message)
                        ? "No autorizado."
                        : unauthorizedEx.Message);
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse<object>.FailureResponse(validationEx.Message);
                if (validationEx.Errors != null)
                {
                    response.Errors = validationEx.Errors.SelectMany(x => x.Value).ToList();
                }
                break;

            case UserNotFoundException userNotFound:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = ApiResponse<object>.FailureResponse(userNotFound.Message);
                break;

            case KeyNotFoundException keyNotFound:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = ApiResponse<object>.FailureResponse(
                    string.IsNullOrWhiteSpace(keyNotFound.Message)
                        ? "Recurso no encontrado."
                        : keyNotFound.Message);
                break;

            case Microsoft.EntityFrameworkCore.DbUpdateException dbEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                response = ApiResponse<object>.FailureResponse($"Error de base de datos: {innerMsg}");
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var debugMsg = $"Error interno: {ex.Message} | Stack: {ex.StackTrace}";
                response = ApiResponse<object>.FailureResponse(debugMsg);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
