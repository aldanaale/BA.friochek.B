
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Queries;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tecnico")]
public class TecnicoController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TecnicoController> _logger;

    public TecnicoController(IMediator mediator, ILogger<TecnicoController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene los tickets asignados al técnico indicado.
    /// </summary>
    [HttpGet("tickets")]
    public async Task<IActionResult> GetTickets([FromQuery] Guid tecnicoId)
    {
        _logger.LogInformation("Endpoint GetTickets llamado para el técnico {TecnicoId}", tecnicoId);
        var tickets = await _mediator.Send(new GetTicketsAsignadosQuery(tecnicoId));
        return Ok(tickets);
    }

    /// <summary>
    /// Reporta una nueva falla detectada en un cooler.
    /// </summary>
    [HttpPost("falla")]
    public async Task<IActionResult> ReportarFalla([FromBody] ReportarFallaCommand command)
    {
        _logger.LogInformation("Endpoint ReportarFalla llamado por el técnico {TecnicoId}", command.TecnicoId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Certifica el cierre de una reparación en un ticket.
    /// </summary>
    [HttpPost("cierre")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CertificarReparacion([FromForm] CertificarReparacionRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value!);
        var tecnicoId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        _logger.LogInformation("Endpoint CertificarReparacion llamado por tecnico {TecnicoId} para ticket {TicketId}", tecnicoId, request.TicketId);

        var command = new CertificarReparacionCommand(
            tenantId,
            tecnicoId,
            request.TicketId,
            request.Comentarios,
            request.Photo,
            request.NfcAccessToken
        );

        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Reparación certificada con éxito." });
    }

    /// <summary>
    /// Re-enrola un Tag NFC en caso de avería, dando de baja el anterior.
    /// </summary>
    [HttpPost("re-enroll")]
    public async Task<IActionResult> ReEnrollNfc([FromBody] ReEnrollNfcRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value!);
        var tecnicoId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        _logger.LogInformation("Endpoint ReEnrollNfc llamado por tecnico {TecnicoId} para Cooler {CoolerId}", tecnicoId, request.CoolerId);

        var command = new ReEnrollNfcCommand(
            tenantId,
            tecnicoId,
            request.CoolerId,
            request.OldNfcUid,
            request.NewNfcUid
        );

        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "NFC Re-enrolado con éxito." });
    }
} 

public record ReEnrollNfcRequest(
    Guid CoolerId,
    string OldNfcUid,
    string NewNfcUid
);

public record CertificarReparacionRequest(
    Guid TicketId, 
    string Comentarios, 
    IFormFile Photo,
    string NfcAccessToken
);
