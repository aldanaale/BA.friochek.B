using BA.Backend.Application.Admin.DTOs;
using BA.Backend.Application.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Devuelve las estadísiticas consolidadas para el home/dashboard del administrador.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats(CancellationToken ct)
    {
        var tenantId = GetTenantIdFromClaims();
        var query = new GetAdminDashboardStatsQuery(tenantId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    private Guid GetTenantIdFromClaims()
    {
        var claim = User.FindFirst("tenant_id");
        if (claim == null || !Guid.TryParse(claim.Value, out var tenantId))
        {
            throw new UnauthorizedAccessException("Tenant invalid");
        }
        return tenantId;
    }
}
