using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record AddOrderItemCommand(
    Guid OrderId, 
    Guid ProductId, 
    string ProductName, 
    int Quantity, 
    int UnitPrice, 
    Guid TenantId) : IRequest<Unit>;

public class AddOrderItemCommandHandler(IOrderRepository orderRepository, ICoolerRepository coolerRepository) 
    : IRequestHandler<AddOrderItemCommand, Unit>
{
    public async Task<Unit> Handle(AddOrderItemCommand request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, request.TenantId, ct);
        if (order == null) throw new KeyNotFoundException("ORDER_NOT_FOUND");

        var cooler = await coolerRepository.GetByIdAsync(order.CoolerId, ct);
        if (cooler == null) throw new KeyNotFoundException("COOLER_NOT_FOUND");

        order.AddItem(request.ProductId, request.ProductName, request.Quantity, request.UnitPrice, cooler.Capacity);

        await orderRepository.UpdateAsync(order, ct);
        await orderRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
