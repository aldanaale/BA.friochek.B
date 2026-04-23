using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        var order = await _orderRepository.GetByIdAsync(request.OrderId, request.TenantId, ct);
        if (order == null) throw new KeyNotFoundException("ORDER_NOT_FOUND");

        order.RemoveItem(request.ItemId);

        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
