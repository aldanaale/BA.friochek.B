using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Application.Common.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using BA.Backend.WebAPI.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IDeviceFingerprintService _fingerprint;
    private readonly ISessionService _sessionService;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger,
        IDeviceFingerprintService fingerprint,
        ISessionService sessionService)
    {
        _mediator = mediator;
        _logger = logger;
        _fingerprint = fingerprint;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Inicia sesión en el sistema y devuelve el token de acceso JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FrontendAuthResponse>), 200)]
    public async Task<ActionResult<ApiResponse<FrontendAuthResponse>>> Login(
        [FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();

        var ua = Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        var lang = Request.Headers.AcceptLanguage.FirstOrDefault() ?? "unknown";
        var fp = _fingerprint.ComputeFingerprint(ua, lang);

        var command = new LoginCommand(
            Email: request.Email?.Trim() ?? string.Empty,
            Password: request.Password ?? string.Empty,
            TenantSlug: null, // Sistema detecta automáticamente por email
            DeviceFingerprint: fp);

        var result = await _mediator.Send(command, cancellationToken);
        
        // MAPEO DE ROLES PARA FRONTEND
        string frontendRole = result.Role.ToLower() switch {
            "admin" => "admin",
            "cliente" => "retailer",
            "transportista" => "delivery",
            "tecnico" => "technician",
            _ => result.Role.ToLower()
        };

        return Ok(ApiResponse<FrontendAuthResponse>.SuccessResponse(new FrontendAuthResponse(
            result.UserId.ToString(),
            result.AccessToken,
            frontendRole,
            result.TenantId.ToString(),
            result.RedirectTo,
            result.ExpiresAt.ToString("o"),
            result.UserId.ToString(),
            result.UserFullName
        )));
    }

    /// <summary>
    /// Cierra la sesión actual invalidando el token en la lista de sesiones activas.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        var sessionId = User.FindFirst("session_id")?.Value;
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized(ApiResponse<object>.FailureResponse("Sesión no identificada."));

        await _sessionService.RevokeSessionAsync(sessionId, cancellationToken);
        _logger.LogInformation("Logout OK: session {SessionId}", sessionId);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Sesión cerrada correctamente."));
    }

    /// <summary>
    /// Solicita el restablecimiento de contraseña mediante el envío de un correo electrónico.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponseDto>), 200)]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponseDto>>> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<ForgotPasswordResponseDto>.FailureResponse("El Email es obligatorio."));

        var command = new ForgotPasswordCommand(
            Email: request.Email.Trim(),
            TenantSlug: null); // Auto-detección global
        
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Resetea la contraseña del usuario utilizando un token válido recibido por correo electrónico.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Request:
    /// {
    ///   "token": "tok_12345...",
    ///   "newPassword": "NewPassword123!",
    ///   "confirmPassword": "NewPassword123!"
    /// }
    /// </remarks>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmPassword);
        await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Contraseña actualizada correctamente."));
    }
}
