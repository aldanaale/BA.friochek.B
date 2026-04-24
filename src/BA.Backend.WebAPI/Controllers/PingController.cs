using BA.Backend.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("ping")]
[AllowAnonymous]
[Tags("Ping")]
public class PingController : ControllerBase
{
    /// <summary>
    /// Verifica si el backend está en línea.
    /// </summary>
    /// <remarks>
    /// Ejemplo de respuesta exitosa (200):
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "status": "Online",
    ///     "message": "Backend FrioCheck está respondiendo correctamente",
    ///     "timestamp": "2026-04-07T15:00:00Z",
    ///     "machineName": "DESKTOP-FRIOCHEK"
    ///   },
    ///   "message": null
    /// }
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        var response = new
        {
            status = "Online",
            message = "Backend FrioCheck está respondiendo correctamente",
            timestamp = DateTime.UtcNow,
            machineName = Environment.MachineName
        };

        return Ok(ApiResponse<object>.SuccessResponse(response));
    }
}