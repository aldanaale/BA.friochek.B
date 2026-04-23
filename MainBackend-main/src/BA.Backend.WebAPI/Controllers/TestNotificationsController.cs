using BA.Backend.Application.Common.Models;
using BA.Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("test-notifications")]
[Authorize]
[Tags("Test")]
public class TestNotificationsController : ControllerBase
{
    private readonly INotificacionService _notificacionService;

    public TestNotificationsController(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    /// <summary>
    /// Envía una notificación de prueba directamente al usuario que hace la petición.
    /// </summary>
    [HttpPost("notify-me")]
    public async Task<ActionResult<ApiResponse<object>>> NotifyMe([FromBody] string mensaje)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized(ApiResponse<object>.FailureResponse("Usuario no válido."));

        await _notificacionService.NotificarUsuarioAsync(userId, "Prueba Personal", mensaje ?? "¡Hola! Esta es una notificación para ti.");

        return Ok(ApiResponse<object>.SuccessResponse(null, "Notificación personal enviada."));
    }

    /// <summary>
    /// Envía una notificación de prueba a TODOS los usuarios del mismo Tenant.
    /// </summary>
    [HttpPost("notify-tenant")]
    public async Task<ActionResult<ApiResponse<object>>> NotifyTenant([FromBody] string mensaje)
    {
        var tenantIdStr = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdStr, out var tenantId))
            return BadRequest(ApiResponse<object>.FailureResponse("Tenant no identificado."));

        await _notificacionService.NotificarTenantAsync(tenantId, "Alerta de Equipo (Broadcast)", mensaje ?? "Atención: Notificación masiva de prueba.");

        return Ok(ApiResponse<object>.SuccessResponse(null, $"Notificación enviada al grupo del Tenant {tenantId}."));
    }
}
