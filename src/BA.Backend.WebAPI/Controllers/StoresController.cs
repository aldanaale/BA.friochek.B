using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using BA.Backend.Application.Stores.Commands;
using BA.Backend.Application.Stores.DTOs;
using BA.Backend.Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("stores")]
[Authorize]
[Tags("Stores")]
public class StoresController(IMediator mediator, ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Lista todas las tiendas pertenecientes al Tenant del usuario actual.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Respuesta 200:
    /// {
    ///   "success": true,
    ///   "data": [
    ///     {
    ///       "id": "a1b2c3d4-0010-0000-0000-000000000000",
    ///       "name": "Tienda Centro",
    ///       "address": "Av. Providencia 1234, Santiago",
    ///       "contactName": "Juan Pérez",
    ///       "contactPhone": "+56912345678",
    ///       "isActive": true
    ///     }
    ///   ],
    ///   "message": null
    /// }
    /// </remarks>
    [HttpGet]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StoreDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StoreDto>>>> GetAll(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await mediator.Send(new GetAllStoresQuery(tenantId), ct);
        return Ok(ApiResponse<IEnumerable<StoreDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene el detalle de una tienda por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StoreDto>>> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await mediator.Send(new GetStoreByIdQuery(id, tenantId), ct);

        if (result == null) return NotFound(ApiResponse<object>.FailureResponse("Tienda no encontrada"));
        return Ok(ApiResponse<StoreDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Crea una nueva tienda.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StoreDto>>> Create([FromBody] CreateStoreDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new CreateStoreCommand(
            dto.Name,
            dto.Address,
            dto.ContactName,
            dto.ContactPhone,
            dto.Latitude,
            dto.Longitude,
            tenantId,
            dto.City,
            dto.District
        );

        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<StoreDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Actualiza una tienda existente.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StoreDto>>> Update(Guid id, [FromBody] UpdateStoreDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new UpdateStoreCommand(
            id,
            dto.Name,
            dto.Address,
            dto.ContactName,
            dto.ContactPhone,
            dto.Latitude,
            dto.Longitude,
            dto.IsActive,
            tenantId,
            dto.City,
            dto.District
        );

        try
        {
            var result = await mediator.Send(command, ct);
            return Ok(ApiResponse<StoreDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.FailureResponse("Tienda no encontrada para actualizar"));
        }
    }

    /// <summary>
    /// Elimina físicamente una tienda.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await mediator.Send(new DeleteStoreCommand(id, tenantId), ct);

        if (!result) return NotFound(ApiResponse<object>.FailureResponse("Tienda no encontrada"));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Tienda eliminada exitosamente" }));
    }
}
