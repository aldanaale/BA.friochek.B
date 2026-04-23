using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Queries;
using BA.Backend.Application.Common.DTOs;
using BA.Backend.Application.Common.Queries;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("tecnico")]
[Authorize(Roles = "Tecnico")]
[Tags("Tecnico")]
public class TecnicoController(
    IMediator mediator,
    ILogger<TecnicoController> logger,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Dashboard Home - Técnico
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianHomeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianHomeResponse>>> GetHome(CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetTechnicianDashboardQuery(userId, tenantId);
        var result = await mediator.Send(query, ct);

        return Ok(ApiResponse<TechnicianHomeResponse>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene la lista de tickets asignados a un técnico.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Respuesta 200:
    /// {
    ///   "success": true,
    ///   "data": [
    ///     {
    ///       "ticketId": "a1b2c3d4-0050-0000-0000-000000000000",
    ///       "faultType": "Temperatura",
    ///       "description": "Cooler no enfría correctamente",
    ///       "status": "Asignado",
    ///       "coolerId": "a1b2c3d4-0020-0000-0000-000000000000",
    ///       "storeName": "Tienda Centro",
    ///       "scheduledDate": "2026-04-10T10:00:00Z"
    ///     }
    ///   ],
    ///   "message": null
    /// }
    /// </remarks>
    [HttpGet("tickets")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TicketAsignadoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TicketAsignadoDto>>>> GetTickets([FromQuery] Guid tecnicoId)
    {
        logger.LogInformation("Endpoint GetTickets called for technician {TecnicoId}", tecnicoId);
        var result = await mediator.Send(new GetTicketsAsignadosQuery(tecnicoId));
        return Ok(ApiResponse<IEnumerable<TicketAsignadoDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Reporta una nueva falla detectada en un cooler.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Request:
    /// {
    ///   "tecnicoId": "a1b2c3d4-0004-0000-0000-000000000000",
    ///   "coolerId": "a1b2c3d4-0020-0000-0000-000000000000",
    ///   "faultType": "Motor",
    ///   "description": "Motor del compresor hace ruido inusual al arrancar",
    ///   "nfcAccessToken": "nfc_tok_eyJhbGciOiJIUzI1NiJ9.example"
    /// }
    /// </remarks>
    [HttpPost("falla")]
    [ProducesResponseType(typeof(ApiResponse<RegistroActividadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RegistroActividadDto>>> ReportarFalla([FromBody] ReportarFallaCommand command)
    {
        logger.LogInformation("Endpoint ReportarFalla called by technician {TecnicoId}", command.TecnicoId);
        var result = await mediator.Send(command);
        return Ok(ApiResponse<RegistroActividadDto>.SuccessResponse(result));
    }

    /*
    /// <summary>
    /// Lista todas las órdenes de trabajo asignadas al técnico.
    /// </summary>
    /// <remarks>
    /// Devuelve las tareas de mantenimiento pendientes o en curso para el técnico autenticado.
    /// </remarks>
    [HttpGet("ordenes")]
    [Authorize(Roles = "Tecnico,Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WorkOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdenes(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await mediator.Send(new GetTecnicoWorkOrdersQuery(tenantId), ct);
        return Ok(ApiResponse<IEnumerable<WorkOrderDto>>.SuccessResponse(result));
    }
    */

    /*
    /// <summary>
    /// Registra el inicio o fin de una actividad de mantenimiento.
    /// </summary>
    /// <param name="dto">Datos del registro de mantenimiento.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPost("mantenimiento")]
    [Authorize(Roles = "Tecnico,Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarMantenimiento([FromBody] RegistrarMantenimientoDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new RegistrarMantenimientoCommand(tenantId, dto.CoolerId, dto.Tipo, dto.Descripcion);
        await mediator.Send(command, ct);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Mantenimiento registrado correctamente"));
    }
    */

    /// <summary>
    /// Certifica el cierre de una reparación en un ticket.
    /// </summary>
    /// <remarks>
    /// Content-Type: multipart/form-data
    /// ticketId: "a1b2c3d4-0050-0000-0000-000000000000"
    /// comentarios: "Se reemplazó el compresor. Cooler operando correctamente a -8°C."
    /// photo: [evidencia.jpg]
    /// nfcAccessToken: "nfc_tok_eyJhbGciOiJIUzI1NiJ9.example"
    /// </remarks>
    [HttpPost("cierre")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CertificarReparacion([FromForm] CertificarReparacionRequest request)
    {
        var tenantId = GetTenantId();
        var tecnicoId = GetUserId();

        logger.LogInformation("Endpoint CertificarReparacion called by technician {TecnicoId} for ticket {TicketId}", tecnicoId, request.TicketId);

        var command = new CertificarReparacionCommand(
            tenantId,
            tecnicoId,
            request.TicketId,
            request.Comentarios,
            request.Photo,
            request.NfcAccessToken
        );

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Reparación certificada con éxito."));
    }

    /// <summary>
    /// Re-enrola un Tag NFC en caso de avería, dando de baja el anterior.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Request:
    /// {
    ///   "coolerId": "a1b2c3d4-0020-0000-0000-000000000000",
    ///   "oldNfcUid": "04:AB:CD:EF:12:34:56",
    ///   "newNfcUid": "04:AB:CD:EF:12:34:99"
    /// }
    /// </remarks>
    [HttpPost("re-enroll")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ReEnrollNfc([FromBody] ReEnrollNfcRequestDto request)
    {
        var tenantId = GetTenantId();
        var tecnicoId = GetUserId();

        logger.LogInformation("Endpoint ReEnrollNfc called by technician {TecnicoId} for Cooler {CoolerId}", tecnicoId, request.CoolerId);

        var command = new ReEnrollNfcCommand(
            tenantId,
            tecnicoId,
            request.CoolerId,
            request.OldNfcUid,
            request.NewNfcUid
        );

        var result = await mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "NFC Re-enrolado con éxito."));
    }
}

public record ReEnrollNfcRequestDto(
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
