using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Commands;

public record AddOrderItemCommand(Guid OrderId, Guid ProductId, int Quantity, Guid TenantId) : IRequest<Unit>;

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public AddOrderItemCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<Unit> Handle(AddOrderItemCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, request.TenantId, ct)
            ?? throw new KeyNotFoundException($"Pedido {request.OrderId} no encontrado.");

        var product = await _productRepository.GetByIdAsync(request.ProductId, ct)
            ?? throw new KeyNotFoundException($"Producto {request.ProductId} no encontrado.");

        order.AddItem(product.Id, product.Name, request.Quantity, (decimal)product.Price);
        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
