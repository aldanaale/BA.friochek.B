using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Commands;

public record CreateRetailerPedidoCommand(
    Guid UserId,
    Guid TenantId,
    List<RetailerCoolerOrderEntry> Coolers) : IRequest<string>;

public record RetailerCoolerOrderEntry(Guid CoolerId, List<RetailerOrderItemEntry> Items);
public record RetailerOrderItemEntry(Guid ProductId, int Quantity);

/// <summary>
/// Maneja la creación masiva de pedidos para múltiples coolers en el flujo retailer.
/// Cada entrada de cooler genera un pedido independiente confirmado en la misma transacción.
/// </summary>
public class CreateRetailerPedidoHandler : IRequestHandler<CreateRetailerPedidoCommand, string>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public CreateRetailerPedidoHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Crea y confirma un pedido por cada cooler incluido en el comando.
    /// </summary>
    public async Task<string> Handle(CreateRetailerPedidoCommand request, CancellationToken ct)
    {
        // Pre-load all unique product IDs to avoid N+1 queries per cooler entry.
        var uniqueProductIds = request.Coolers
            .SelectMany(c => c.Items.Select(i => i.ProductId))
            .Distinct()
            .ToList();

        var productCache = new Dictionary<Guid, Product?>();
        foreach (var productId in uniqueProductIds)
        {
            productCache[productId] = await _productRepository.GetByIdAsync(productId, ct);
        }

        foreach (var coolerEntry in request.Coolers)
        {
            var order = Order.Create(request.UserId, coolerEntry.CoolerId, null, request.TenantId);
            AddItemsToOrder(order, coolerEntry, productCache);
            order.Confirm();
            await _orderRepository.AddAsync(order, ct);
        }

        await _orderRepository.SaveChangesAsync(ct);
        return "Pedido(s) creado(s) correctamente";
    }

    /// <summary>
    /// Agrega los ítems de un cooler al pedido usando el caché de productos pre-cargado.
    /// Si el producto no existe en el caché, se usa un nombre genérico y precio cero como fallback.
    /// </summary>
    private static void AddItemsToOrder(
        Order order,
        RetailerCoolerOrderEntry coolerEntry,
        Dictionary<Guid, Product?> productCache)
    {
        foreach (var item in coolerEntry.Items)
        {
            productCache.TryGetValue(item.ProductId, out var product);

            var name = product?.Name ?? $"Producto {item.ProductId}";
            var price = product != null ? (decimal)product.Price : 0m;

            order.AddItem(item.ProductId, name, item.Quantity, price);
        }
    }
}
