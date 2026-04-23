using BA.Backend.Application.Common.Models;
using BA.Backend.Application.PlatformAdmin.Commands;
using BA.Backend.Application.PlatformAdmin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("platform")]
[Authorize(Roles = "PlatformAdmin")]
[Tags("Platform")]
public class PlatformAdminController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista todos los tenants registrados en la plataforma.
    /// </summary>
    [HttpGet("tenants")]
    [ProducesResponseType(typeof(ApiResponse<List<TenantDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TenantDto>>>> GetTenants(CancellationToken ct)
    {
        var query = new GetTenantsQuery();
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<List<TenantDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Crea un nuevo tenant (marca/empresa) en la plataforma.
    /// </summary>
    [HttpPost("tenants")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTenant([FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<Guid>.SuccessResponse(result));
    }
}
