using BA.Backend.Application.Common.Models;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Supervisor.Commands;
using BA.Backend.Application.Supervisor.DTOs;
using BA.Backend.Application.Supervisor.Queries;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("supervisor")]
[Authorize(Roles = "Supervisor,Admin,PlatformAdmin")]
[Tags("Supervisor")]
public class SupervisorController(
    IMediator mediator,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Dashboard Home - Supervisor
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(ApiResponse<SupervisorDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupervisorDashboardDto>>> GetHome(CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetSupervisorHomeQuery(userId, tenantId);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<SupervisorDashboardDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Lista los técnicos del tenant con su carga de trabajo actual.
    /// </summary>
    [HttpGet("technicians")]
    [ProducesResponseType(typeof(ApiResponse<List<TechnicianWorkloadDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TechnicianWorkloadDto>>>> GetTechnicians(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var query = new GetSupervisorTechniciansQuery(tenantId);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<List<TechnicianWorkloadDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Asigna un técnico específico a un ticket de soporte.
    /// </summary>
    [HttpPost("tickets/{id:guid}/assign")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignTicket(Guid id, [FromBody] Guid technicianId, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new AssignTicketCommand(id, technicianId, tenantId);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<bool>.SuccessResponse(result));
    }
}
