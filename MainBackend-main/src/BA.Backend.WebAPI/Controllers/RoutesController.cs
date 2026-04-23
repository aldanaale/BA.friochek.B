using BA.Backend.Application.Routes.Commands;
using BA.Backend.Application.Routes.DTOs;
using BA.Backend.Application.Routes.Queries;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("routes")]
[Authorize]
[Tags("Transporte")]
public class RoutesController(
    IMediator mediator,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Lista todas las rutas de entrega programadas en el tenant.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Transportista,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RouteDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RouteDto>>>> GetAll([FromQuery] Guid? transportistaId = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var result = await Mediator.Send(new GetRoutesQuery(tenantId, transportistaId), ct);
        return Ok(ApiResponse<IEnumerable<RouteDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene el detalle de una ruta específica e incluye sus paradas.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Transportista,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<RouteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RouteDto>>> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await Mediator.Send(new GetRouteByIdQuery(id, tenantId), ct);
        if (result == null) return NotFound(ApiResponse<object>.FailureResponse("Ruta no encontrada"));
        return Ok(ApiResponse<RouteDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Planifica y crea una nueva ruta de entrega.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateRouteDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new CreateRouteCommand(tenantId, dto.TransportistaId, dto.Date, dto.Stops);

        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<object>.SuccessResponse(new { id }, "Ruta creada exitosamente"));
    }

    /// <summary>
    /// Actualiza el estado global de la ruta.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromBody] UpdateRouteStatusDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new UpdateRouteStatusCommand(id, tenantId, dto.Status);

        var result = await Mediator.Send(command, ct);
        if (!result) return NotFound(ApiResponse<object>.FailureResponse("Ruta no encontrada"));
        return Ok(ApiResponse<object>.SuccessResponse(null, "Estado actualizado correctamente"));
    }

    /// <summary>
    /// Elimina una ruta (soft-delete).
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await Mediator.Send(new DeleteRouteCommand(id, tenantId), ct);
        if (!result) return NotFound(ApiResponse<object>.FailureResponse("Ruta no encontrada"));
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Ruta eliminada exitosamente"));
    }

}