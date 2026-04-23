using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BA.Backend.Application.Common.Interfaces;


namespace BA.Backend.WebAPI.Middleware;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;

    public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sessionId = context.User.FindFirst("session_id")?.Value;
            _logger.LogDebug("Verificando validez de sesión: {SessionId}", sessionId);

            if (!string.IsNullOrEmpty(sessionId))
            {
                var sessionService = context.RequestServices.GetRequiredService<ISessionService>();
                var isValid = await sessionService.IsSessionValidAsync(sessionId, context.RequestAborted);

                if (!isValid)
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    _logger.LogWarning("Sesión RECHAZADA: {SessionId} para el usuario: {UserId}. Path: {Path}", sessionId, userId, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                else
                {
                    _logger.LogDebug("Sesión {SessionId} validada correctamente.", sessionId);
                }
            }
            else
            {
                _logger.LogWarning("Token sin claim 'session_id'. Path: {Path}", context.Request.Path);
            }
        }

        await _next(context);
    }
}
