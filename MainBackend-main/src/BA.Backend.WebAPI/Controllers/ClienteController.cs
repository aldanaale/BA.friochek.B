using BA.Backend.Application.Common.DTOs;
using BA.Backend.Application.Common.Queries;
using BA.Backend.Domain.Exceptions;
using BA.Backend.WebAPI.DTOs.Cliente;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Cliente.Queries;
using BA.Backend.Application.Cliente.Commands;
using BA.Backend.Application.Users.DTOs;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("cliente")]
[Authorize(Roles = "Cliente,Admin")]
[Tags("Cliente")]
public class ClienteController(
    IMediator mediator,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Dashboard Home - Retailer
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(ApiResponse<RetailerHomeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RetailerHomeResponse>>> GetHome(CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetRetailerDashboardQuery(userId, tenantId);
        var result = await mediator.Send(query, ct);

        return Ok(ApiResponse<RetailerHomeResponse>.SuccessResponse(result));
    }

    /// <summary>
    /// Catálogo de productos disponibles para el Tenant.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Respuesta 200:
    /// {
    ///   "success": true,
    ///   "data": [
    ///     {
    ///       "id": "a1b2c3d4-0040-0000-0000-000000000000",
    ///       "name": "Helado Vainilla 1L",
    ///       "price": 2990.00,
    ///       "unit": "unidad",
    ///       "isActive": true
    ///     }
    ///   ],
    ///   "message": null
    /// }
    /// </remarks>
    [HttpGet("products")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts(CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var query = new GetProductsQuery(tenantId);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Consulta el stock real de un producto en el sistema del Tenant.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("products/{id:guid}/stock")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> GetProductStock(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var query = new GetProductStockQuery(id, tenantId);
        var stock = await mediator.Send(query, ct);
        return Ok(ApiResponse<int>.SuccessResponse(stock));
    }

    /// <summary>
    /// Crea una nueva solicitud de soporte técnico (Soporta Multipart/Form-Data para fotos).
    /// </summary>
    [HttpPost("tech-support")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTechSupportForm([FromForm] CreateTechSupportRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateTechSupportCommand(
            request.NfcAccessToken,
            request.FaultType,
            request.Description,
            request.ScheduledDate,
            request.Photos,
            GetUserId(),
            GetTenantId()
        ), ct);

        return Created("", ApiResponse<Guid>.SuccessResponse(result));
    }

    /// <summary>
    /// Crea una solicitud de soporte técnico (Soporta JSON, sin fotos).
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("tech-support-json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTechSupportJson([FromBody] CreateTechSupportRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateTechSupportCommand(
            request.NfcAccessToken,
            request.FaultType,
            request.Description,
            request.ScheduledDate,
            null,
            GetUserId(),
            GetTenantId()
        ), ct);

        return Created("", ApiResponse<Guid>.SuccessResponse(result));
    }

    /// <summary>
    /// Lista las solicitudes de soporte técnico del cliente
    /// </summary>
    [HttpGet("tech-support")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<TechSupportDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<TechSupportDto>>>> GetMyTechRequests([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetMyTechRequestsQuery(userId, tenantId, pageNumber, pageSize);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResultDto<TechSupportDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Reporta un tag NFC dañado que no se puede escanear.
    /// </summary>
    /// <remarks>
    /// Request:
    /// {
    ///   "coolerId": "a1b2c3d4-0020-0000-0000-000000000000",
    ///   "description": "El tag NFC está físicamente dañado, no responde al lector del celular"
    /// }
    /// </remarks>
    [HttpPost("nfc/report-damaged")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Guid>>> ReportDamagedTag([FromBody] ReportDamagedTagRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var command = new ReportDamagedTagCommand(request.CoolerId, request.Description, userId, tenantId);
        var id = await mediator.Send(command, ct);
        return Ok(ApiResponse<Guid>.SuccessResponse(id));
    }

}
