using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Commands;

public record UpdateOrderItemCommand(Guid OrderId, Guid ItemId, int Quantity, Guid TenantId) : IRequest<Unit>;

public class UpdateOrderItemCommandHandler : IRequestHandler<UpdateOrderItemCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderItemCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(UpdateOrderItemCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, request.TenantId, ct)
            ?? throw new KeyNotFoundException($"Pedido {request.OrderId} no encontrado.");

        order.UpdateItem(request.ItemId, request.Quantity);
        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
