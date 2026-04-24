using BA.Backend.Application.Common.Models;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.EjecutivoComercial.DTOs;
using BA.Backend.Application.EjecutivoComercial.Queries;
using BA.Backend.Application.EjecutivoComercial.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("ejecutivo")]
[Authorize(Roles = "EjecutivoComercial,Admin")]
[Tags("Comercial")]
public class EjecutivoComercialController(
    IMediator mediator,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Dashboard Home - Ejecutivo Comercial
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(ApiResponse<EjecutivoDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EjecutivoDashboardDto>>> GetHome(CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetEjecutivoHomeQuery(userId, tenantId);
        var result = await mediator.Send(query, ct);

        return Ok(ApiResponse<EjecutivoDashboardDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Agrega una nota comercial de seguimiento a un cliente específico.
    /// </summary>
    [HttpPost("clients/{id:guid}/notes")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Guid>>> AddNote(Guid id, [FromBody] string content, CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();
        var command = new AddClientNoteCommand(id, userId, content, tenantId);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<Guid>.SuccessResponse(result));
    }
}
