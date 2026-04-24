using BA.Backend.Application.Common.Models;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("transportistas-admin")]
[Authorize(Roles = "Admin,PlatformAdmin")]
[Tags("Transporte")]
public class TransportistasAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransportistasAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista todos los transportistas registrados en el tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransportistaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await _mediator.Send(new GetAllTransportistasQuery(tenantId), ct);
        return Ok(ApiResponse<IEnumerable<TransportistaDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene el detalle de un transportista por su ID de usuario.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransportistaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await _mediator.Send(new GetTransportistaByIdQuery(id, tenantId), ct);
        if (result == null) return NotFound(ApiResponse<object>.FailureResponse("Transportista no encontrado"));
        return Ok(ApiResponse<TransportistaDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Registra un nuevo transportista.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransportistaDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new CreateTransportistaCommand(tenantId, dto.UserId, dto.VehiclePlate);

        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<object>.SuccessResponse(new { id }, "Transportista creado exitosamente"));
    }

    /// <summary>
    /// Actualiza el perfil de un transportista.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransportistaDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new UpdateTransportistaCommand(id, tenantId, dto.IsAvailable, dto.VehiclePlate);

        var result = await _mediator.Send(command, ct);
        if (!result) return NotFound(ApiResponse<object>.FailureResponse("Transportista no encontrado"));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Transportista actualizado exitosamente"));
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("tenant_id")?.Value;
        return Guid.Parse(claim ?? throw new UnauthorizedAccessException("Tenant ID no encontrado"));
    }
}