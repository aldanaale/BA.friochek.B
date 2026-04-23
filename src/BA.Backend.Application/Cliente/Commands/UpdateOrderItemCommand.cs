using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record UpdateOrderItemCommand(Guid OrderId, Guid ItemId, int Quantity, Guid TenantId) : IRequest<Unit>;

public class UpdateOrderItemCommandHandler : IRequestHandler<UpdateOrderItemCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICoolerRepository _coolerRepository;

    public UpdateOrderItemCommandHandler(IOrderRepository orderRepository, ICoolerRepository coolerRepository)
    {
        _orderRepository = orderRepository;
        _coolerRepository = coolerRepository;
    }

    public async Task<Unit> Handle(UpdateOrderItemCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, request.TenantId, ct);
        if (order == null) throw new KeyNotFoundException("ORDER_NOT_FOUND");

        var cooler = await _coolerRepository.GetByIdAsync(order.CoolerId, ct);
        if (cooler == null) throw new KeyNotFoundException("COOLER_NOT_FOUND");

        order.UpdateItem(request.ItemId, request.Quantity, cooler.Capacity);

        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
