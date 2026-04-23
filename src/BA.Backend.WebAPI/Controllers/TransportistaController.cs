using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BA.Backend.Application.Transportista.Queries;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Cliente.Commands;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Transportista")]
public class TransportistaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransportistaController> _logger;

    public TransportistaController(IMediator mediator, ILogger<TransportistaController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la ruta de entrega asignada al transportista conectado.
    /// </summary>
    [HttpGet("route")]
    [ProducesResponseType(typeof(List<TransportistaRouteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyRoute()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var tenantIdClaim = User.FindFirst("tenant_id");

        if (userIdClaim == null || tenantIdClaim == null)
        {
            _logger.LogWarning("Intento de acceso sin claims de identificación válidos.");
            return Unauthorized();
        }

        var transportistId = Guid.Parse(userIdClaim.Value);
        var tenantId = Guid.Parse(tenantIdClaim.Value);

        _logger.LogInformation("Obteniendo hoja de ruta para Transportista {TransportistId}", transportistId);

        var query = new GetRouteQuery(transportistId, tenantId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Registra una entrega como completada usando el NFC escaneado.
    /// </summary>
    [HttpPost("delivery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordDelivery([FromBody] RecordDeliveryRequest req)
    {
        _logger.LogInformation("Registrando entrega para Pedido {OrderId}", req.OrderId);
        
        var command = new DeliveryCommand(req.OrderId, req.RouteStopId, req.NfcAccessToken, req.DeliveredItems);
        await _mediator.Send(command);
        
        return Ok(new { message = "Entrega registrada exitosamente." });
    }

    /// <summary>
    /// Reporta una merma de producto o cooler durante la entrega.
    /// </summary>
    [HttpPost("merma")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordMerma([FromForm] RecordMermaRequest req)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value!);
        var transportistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        _logger.LogInformation("Reportando merma en Cooler {CoolerId} por Transportista {TransportistId}", req.CoolerId, transportistId);

        var command = new MermaCommand(
            tenantId,
            transportistId,
            req.CoolerId,
            req.ProductId,
            req.ProductName,
            req.Quantity,
            req.Reason,
            req.Description ?? string.Empty,
            req.Photo,
            req.NfcAccessToken
        );

        var id = await _mediator.Send(command);
        return Ok(new { id, message = "Merma reportada con éxito." });
    }

    /// <summary>
    /// Crea un ticket de soporte técnico (ej. cooler descompuesto) durante la ruta.
    /// </summary>
    [HttpPost("tech-support")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTechSupport([FromForm] CreateTechSupportRequest req)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value!);
        var transportistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        _logger.LogInformation("Creando Ticket de Soporte por Transportista {TransportistId}", transportistId);

        var command = new CreateTechSupportCommand(
            req.NfcAccessToken,
            req.FaultType,
            req.Description,
            req.ScheduledDate,
            req.Photos,
            transportistId,
            tenantId
        );

        var id = await _mediator.Send(command);
        return Ok(new { id, message = "Ticket de soporte creado con éxito." });
    }
}
