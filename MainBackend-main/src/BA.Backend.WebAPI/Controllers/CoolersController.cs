using BA.Backend.Application.Coolers.Commands;
using BA.Backend.Application.Coolers.DTOs;
using BA.Backend.Application.Coolers.Queries;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("coolers")]
[Authorize]
[Tags("Coolers")]
public class CoolersController(
    IMediator mediator,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Lista todos los coolers registrados en el tenant del administrador.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite obtener la lista completa de equipos de refrigeración asociados a la cuenta del administrador.
    /// </remarks>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CoolerListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await Mediator.Send(new GetAllCoolersQuery(tenantId), ct);
        return Ok(ApiResponse<IEnumerable<CoolerListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene el detalle de un cooler específico por su ID.
    /// </summary>
    /// <param name="id">Identificador único del cooler (GUID).</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Tecnico")]
    [ProducesResponseType(typeof(ApiResponse<CoolerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await Mediator.Send(new GetCoolerByIdQuery(id, tenantId), ct);
        if (result == null) return NotFound(ApiResponse<object>.FailureResponse("Cooler no encontrado"));
        return Ok(ApiResponse<CoolerDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene la información del tag NFC asociado a un cooler.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("{id:guid}/tags")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<NfcTagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTags(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await Mediator.Send(new GetCoolerTagsQuery(id, tenantId), ct);
        if (result == null) return NotFound(ApiResponse<object>.FailureResponse("Tags no encontrados para este cooler"));
        return Ok(ApiResponse<NfcTagDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Registra un nuevo cooler en el sistema.
    /// </summary>
    /// <remarks>
    /// Ejemplo de cuerpo de solicitud:
    /// 
    ///     POST /api/v1/coolers
    ///     {
    ///        "storeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "name": "Cooler Principal",
    ///        "serialNumber": "SN123456",
    ///        "model": "FrioModel-2024",
    ///        "capacity": 500,
    ///        "status": "Activo"
    ///     }
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCoolerDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new CreateCoolerCommand(tenantId, dto.StoreId, dto.Name, dto.SerialNumber, dto.Model, dto.Capacity, dto.Status);

        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<object>.SuccessResponse(new { id }, "Cooler creado exitosamente"));
    }

    /// <summary>
    /// Actualiza la información básica de un cooler existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCoolerDto dto, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new UpdateCoolerCommand(id, tenantId, dto.Name, dto.SerialNumber, dto.Model, dto.Capacity, dto.Status);

        var result = await Mediator.Send(command, ct);
        if (!result) return NotFound(ApiResponse<object>.FailureResponse("Cooler no encontrado"));
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Cooler actualizado exitosamente"));
    }

    /// <summary>
    /// Actualiza únicamente el estado operativo de un cooler.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var command = new UpdateCoolerStatusCommand(id, tenantId, status);

        try
        {
            var result = await Mediator.Send(command, ct);
            if (!result) return NotFound(ApiResponse<object>.FailureResponse("Cooler no encontrado"));
            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Estado del cooler actualizado exitosamente"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Elimina de forma lógica un cooler del sistema.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var result = await Mediator.Send(new DeleteCoolerCommand(id, tenantId), ct);
        if (!result) return NotFound(ApiResponse<object>.FailureResponse("Cooler no encontrado"));
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Cooler eliminado exitosamente"));
    }

}