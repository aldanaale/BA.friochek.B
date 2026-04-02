using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Cliente.Queries;
using BA.Backend.Application.Cliente.Commands;
using BA.Backend.Application.Users.DTOs;
using BA.Backend.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/v1/cliente")]
[Authorize(Roles = "Cliente")]
public class ClienteController(IMediator mediator, ILogger<ClienteController> logger) : ControllerBase
{
    /// <summary>
    /// Obtiene la información para el Dashboard del Home del Cliente
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(ClientHomeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientHomeDto>> GetHome(CancellationToken ct)
    {
        try 
        {
            var userId = GetUserIdFromClaims();
            var tenantId = GetTenantIdFromClaims();

            logger.LogInformation("Cargando Dashboard para el Cliente {UserId} en Tenant {TenantId}", userId, tenantId);

            var query = new GetClientHomeQuery(userId, tenantId);
            var result = await mediator.Send(query, ct);
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Catálogo de productos disponibles para el Tenant
    /// </summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(CancellationToken ct)
    {
        var tenantId = GetTenantIdFromClaims();
        var query = new GetProductsQuery(tenantId);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo pedido
    /// </summary>
    [HttpPost("orders")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        var tenantId = GetTenantIdFromClaims();
        
        var command = new CreateOrderCommand(request.NfcAccessToken, userId, tenantId);
        var orderId = await mediator.Send(command, ct);
        
        return Ok(orderId);
    }

    /// <summary>
    /// Agregar item al pedido
    /// </summary>
    [HttpPost("orders/{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddItemRequest request, CancellationToken ct)
    {
        try
        {
            var tenantId = GetTenantIdFromClaims();
            var command = new AddOrderItemCommand(id, request.ProductId, request.ProductName, request.Quantity, request.UnitPrice, tenantId);
            await mediator.Send(command, ct);
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { code = ex.Code, message = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar cantidad de un item
    /// </summary>
    [HttpPut("orders/{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        try
        {
            var tenantId = GetTenantIdFromClaims();
            var command = new UpdateOrderItemCommand(id, itemId, request.Quantity, tenantId);
            await mediator.Send(command, ct);
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { code = ex.Code, message = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar un item del pedido
    /// </summary>
    [HttpDelete("orders/{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
    {
        var tenantId = GetTenantIdFromClaims();
        var command = new RemoveOrderItemCommand(id, itemId, tenantId);
        await mediator.Send(command, ct);
        return Ok();
    }

    /// <summary>
    /// Confirmar pedido
    /// </summary>
    [HttpPost("orders/{id:guid}/confirm")]
    [ProducesResponseType(typeof(ClientOrderSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClientOrderSummaryDto>> ConfirmOrder(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantIdFromClaims();
        var command = new ConfirmOrderCommand(id, tenantId);
        var summary = await mediator.Send(command, ct);
        return Ok(summary);
    }

    /// <summary>
    /// Listar mis pedidos (paginado)
    /// </summary>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(PagedResultDto<ClientOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ClientOrderSummaryDto>>> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var userId = GetUserIdFromClaims();
        var tenantId = GetTenantIdFromClaims();
        
        var query = new GetMyOrdersQuery(userId, tenantId, pageNumber, pageSize);
        var result = await mediator.Send(query, ct);
        
        return Ok(result);
    }

    /// <summary>
    /// Detalle de un pedido con sus items
    /// </summary>
    [HttpGet("orders/{id:guid}")]
    [ProducesResponseType(typeof(ClientOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientOrderDto>> GetOrderById(Guid id, CancellationToken ct)
    {
        var tenantId = GetTenantIdFromClaims();
        var query = new GetOrderByIdQuery(id, tenantId);
        var result = await mediator.Send(query, ct);
        
        if (result is null) return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva solicitud de soporte técnico
    /// </summary>
    [HttpPost("tech-support")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Guid>> CreateTechSupport([FromForm] CreateTechSupportRequest request, CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        var tenantId = GetTenantIdFromClaims();

        var command = new CreateTechSupportCommand(
            request.NfcAccessToken,
            request.FaultType,
            request.Description,
            request.ScheduledDate,
            request.Photos,
            userId,
            tenantId
        );

        var id = await mediator.Send(command, ct);
        return Ok(id);
    }

    /// <summary>
    /// Lista las solicitudes de soporte técnico del cliente
    /// </summary>
    [HttpGet("tech-support")]
    public async Task<ActionResult<PagedResultDto<TechSupportDto>>> GetMyTechRequests([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var userId = GetUserIdFromClaims();
        var tenantId = GetTenantIdFromClaims();

        var query = new GetMyTechRequestsQuery(userId, tenantId, pageNumber, pageSize);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Reporta un tag NFC dañado que no se puede escanear
    /// </summary>
    [HttpPost("nfc/report-damaged")]
    public async Task<ActionResult<Guid>> ReportDamagedTag([FromBody] ReportDamagedTagRequest request, CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        var tenantId = GetTenantIdFromClaims();

        var command = new ReportDamagedTagCommand(request.CoolerId, request.Description, userId, tenantId);
        var id = await mediator.Send(command, ct);
        return Ok(id);
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("UserId no encontrado o inválido en el JWT");
        
        return userId;
    }

    private Guid GetTenantIdFromClaims()
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            throw new UnauthorizedAccessException("TenantId no encontrado o inválido en el JWT");
        
        return tenantId;
    }
}

public record CreateOrderRequest(string NfcAccessToken);
public record AddItemRequest(Guid ProductId, string ProductName, int Quantity, int UnitPrice);
public record UpdateItemRequest(int Quantity);
public record CreateTechSupportRequest(string NfcAccessToken, string FaultType, string Description, DateTime ScheduledDate, IFormFileCollection Photos);
public record ReportDamagedTagRequest(Guid CoolerId, string Description);
