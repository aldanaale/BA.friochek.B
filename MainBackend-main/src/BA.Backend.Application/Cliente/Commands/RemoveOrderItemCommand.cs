using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Commands;

public record RemoveOrderItemCommand(Guid OrderId, Guid ItemId, Guid TenantId) : IRequest<Unit>;

public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;

    public RemoveOrderItemCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(RemoveOrderItemCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, request.TenantId, ct)
            ?? throw new KeyNotFoundException($"Pedido {request.OrderId} no encontrado.");

        order.RemoveItem(request.ItemId);
        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
