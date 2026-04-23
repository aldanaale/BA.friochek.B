using BA.Backend.Application.Admin.DTOs;
using BA.Backend.Application.Admin.Queries;
using BA.Backend.Application.Common.DTOs;
using BA.Backend.Application.Common.Queries;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using BA.Backend.Application.Users.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
[Tags("Admin")]
public class AdminController(
    IMediator mediator,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Dashboard Home - Admin
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(ApiResponse<AdminHomeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminHomeResponse>>> GetHome(CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetAdminDashboardQuery(userId, tenantId);
        var result = await mediator.Send(query, ct);

        return Ok(ApiResponse<AdminHomeResponse>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene las estadísticas consolidadas para el dashboard del administrador.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminDashboardStatsDto>>> GetDashboardStats(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var query = new GetAdminDashboardStatsQuery(tenantId);

        AdminDashboardStatsDto result = await mediator.Send(query, ct);

        return Ok(ApiResponse<AdminDashboardStatsDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene el listado paginado de mermas reportadas en el tenant.
    /// </summary>
    [HttpGet("mermas")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<AdminMermaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<AdminMermaDto>>>> GetMermas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var query = new GetAdminMermasQuery(tenantId, pageNumber, pageSize);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResultDto<AdminMermaDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene el listado paginado de solicitudes de soporte técnico en el tenant.
    /// </summary>
    [HttpGet("tech-support")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<BA.Backend.Application.Cliente.DTOs.TechSupportDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<BA.Backend.Application.Cliente.DTOs.TechSupportDto>>>> GetTechSupport([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var query = new GetAdminTechSupportQuery(tenantId, pageNumber, pageSize);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResultDto<BA.Backend.Application.Cliente.DTOs.TechSupportDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Dispara físicamente la sincronización del catálogo desde la fuente externa configurada.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("sync-catalog")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> SyncCatalog(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new BA.Backend.Application.Admin.Commands.SyncCatalogCommand(tenantId);
        var processedCount = await mediator.Send(command, ct);
        return Ok(ApiResponse<int>.SuccessResponse(processedCount, $"Sincronización completada. {processedCount} productos procesados."));
    }
}

