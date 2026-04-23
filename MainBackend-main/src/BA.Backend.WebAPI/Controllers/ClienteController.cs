using BA.Backend.Application.Common.DTOs;
using BA.Backend.Application.Common.Queries;
using BA.Backend.Domain.Exceptions;
using BA.Backend.WebAPI.DTOs.Cliente;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Cliente.Queries;
using BA.Backend.Application.Cliente.Commands;
using BA.Backend.Application.Users.DTOs;
using Unit = MediatR.Unit;

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
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
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
    /// Crea un nuevo pedido vinculado a un cooler via NFC.
    /// </summary>
    /// <remarks>
    /// El token NFC identifica el cooler físico al que se asociará el pedido.
    /// El pedido se crea en estado "PorPagar" y queda abierto para agregar ítems.
    /// </remarks>
    [HttpPost("orders")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var command = new CreateOrderCommand(request.NfcAccessToken, GetUserId(), GetTenantId());
        return await Send(command, ct);
    }

    /// <summary>
    /// Lista los pedidos del cliente autenticado (paginado).
    /// </summary>
    /// <remarks>
    /// Los resultados se ordenan por fecha de creación descendente.
    /// </remarks>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<ClientOrderSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ClientOrderSummaryDto>>>> GetMyOrders(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var query = new GetMyOrdersQuery(GetUserId(), GetTenantId(), pageNumber, pageSize);
        return await Send(query, ct);
    }

    /// <summary>
    /// Obtiene el detalle de un pedido con sus ítems.
    /// </summary>
    /// <remarks>
    /// Solo retorna pedidos pertenecientes al tenant del usuario autenticado.
    /// </remarks>
    [HttpGet("orders/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClientOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClientOrderDto>>> GetOrderById(Guid id, CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(id, GetTenantId());
        return await Send(query, ct);
    }

    /// <summary>
    /// Agrega un producto al pedido indicado.
    /// </summary>
    /// <remarks>
    /// El pedido debe estar en estado "PorPagar". La cantidad debe ser entre 1 y 999.
    /// </remarks>
    [HttpPost("orders/{id:guid}/items")]
    [ProducesResponseType(typeof(ApiResponse<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Unit>>> AddItem(Guid id, [FromBody] AddItemRequest request, CancellationToken ct)
    {
        var command = new AddOrderItemCommand(id, request.ProductId, request.Quantity, GetTenantId());
        return await Send(command, ct);
    }

    /// <summary>
    /// Actualiza la cantidad de un ítem del pedido.
    /// </summary>
    /// <remarks>
    /// La cantidad debe ser entre 1 y 999. El pedido debe estar en estado "PorPagar".
    /// </remarks>
    [HttpPut("orders/{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Unit>>> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        var command = new UpdateOrderItemCommand(id, itemId, request.Quantity, GetTenantId());
        return await Send(command, ct);
    }

    /// <summary>
    /// Elimina un ítem del pedido.
    /// </summary>
    [HttpDelete("orders/{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Unit>>> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
    {
        var command = new RemoveOrderItemCommand(id, itemId, GetTenantId());
        return await Send(command, ct);
    }

    /// <summary>
    /// Confirma un pedido cerrando su composición.
    /// </summary>
    /// <remarks>
    /// El pedido debe tener al menos un ítem para poder confirmarse.
    /// Una vez confirmado, no se pueden agregar, modificar ni eliminar ítems.
    /// </remarks>
    [HttpPost("orders/{id:guid}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<ClientOrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClientOrderSummaryDto>>> ConfirmOrder(Guid id, CancellationToken ct)
    {
        var command = new ConfirmOrderCommand(id, GetTenantId());
        return await Send(command, ct);
    }

    /// <summary>
    /// Crea pedidos masivos para múltiples coolers (flujo retailer).
    /// </summary>
    /// <remarks>
    /// Permite crear y confirmar en una sola operación pedidos para uno o más coolers.
    /// Cada entrada en el arreglo "coolers" genera un pedido independiente.
    /// </remarks>
    [HttpPost("pedido")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> CreatePedido([FromBody] RetailerPedidoRequest request, CancellationToken ct)
    {
        var command = new CreateRetailerPedidoCommand(
            GetUserId(),
            GetTenantId(),
            request.Coolers.Select(c => new RetailerCoolerOrderEntry(
                c.CoolerId,
                c.Items.Select(i => new RetailerOrderItemEntry(i.ProductId, i.Quantity)).ToList()
            )).ToList()
        );
        return await Send(command, ct);
    }

    /// <summary>
    /// Lanza un pedido externo y devuelve la URL de redirección.
    /// </summary>
    /// <remarks>
    /// Usado para productos que se gestionan en sistemas externos de pago.
    /// El pedido se registra localmente como referencia de trazabilidad.
    /// </remarks>
    [HttpPost("orders/external-launch")]
    [ProducesResponseType(typeof(ApiResponse<ExternalOrderLaunchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExternalOrderLaunchResult>>> LaunchExternalOrder(
        [FromBody] LaunchExternalOrderRequest request, CancellationToken ct)
    {
        var command = new LaunchExternalOrderCommand(request.ProductId, GetUserId(), GetTenantId());
        return await Send(command, ct);
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
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> ReportDamagedTag([FromBody] ReportDamagedTagRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var command = new ReportDamagedTagCommand(request.CoolerId, request.Description, userId, tenantId);
        var id = await mediator.Send(command, ct);
        return Ok(ApiResponse<Guid>.SuccessResponse(id));
    }

}
