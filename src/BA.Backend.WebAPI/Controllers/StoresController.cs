
using BA.Backend.Application.Stores.Commands;
using BA.Backend.Application.Stores.DTOs;
using BA.Backend.Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public StoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista todas las tiendas pertenecientes al Tenant del usuario actual.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        Console.WriteLine("Cargando tiendas para el tenant: " + tenantId);
        
        var result = await _mediator.Send(new GetAllStoresQuery(tenantId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de una tienda por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await _mediator.Send(new GetStoreByIdQuery(id, tenantId), ct);
        
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Registra una nueva tienda en el sistema.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateStoreDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new CreateStoreCommand(
            dto.Name,
            dto.Address,
            dto.ContactName,
            dto.ContactPhone,
            dto.Latitude,
            dto.Longitude,
            tenantId
        );
        
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza los datos de una tienda existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        Console.WriteLine("Actualizando datos de la tienda: " + id);

        var command = new UpdateStoreCommand(
            id,
            dto.Name,
            dto.Address,
            dto.ContactName,
            dto.ContactPhone,
            dto.Latitude,
            dto.Longitude,
            dto.IsActive,
            tenantId
        );

        try
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("No se encontro la tienda para actualizar");
            return NotFound();
        }
    }

    /// <summary>
    /// Elimina una tienda del sistema.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        Console.WriteLine("Intentando borrar la tienda: " + id);

        var result = await _mediator.Send(new DeleteStoreCommand(id, tenantId), ct);
        
        if (!result) return NotFound();
        
        Console.WriteLine("Tienda borrada con exito");
        return NoContent();
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("tenant_id")?.Value;
        
        if (string.IsNullOrEmpty(claim))
        {
            throw new UnauthorizedAccessException("Tenant ID no encontrado en los claims");
        }
        
        return Guid.Parse(claim);
    }
}
