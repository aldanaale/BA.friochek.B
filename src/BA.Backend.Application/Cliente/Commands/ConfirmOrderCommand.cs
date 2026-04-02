using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record ConfirmOrderCommand(Guid OrderId, Guid TenantId) : IRequest<ClientOrderSummaryDto>;

public class ConfirmOrderCommandHandler(IOrderRepository orderRepository) 
    : IRequestHandler<ConfirmOrderCommand, ClientOrderSummaryDto>
{
    public async Task<ClientOrderSummaryDto> Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, request.TenantId, ct);
        if (order == null) throw new KeyNotFoundException("ORDER_NOT_FOUND");

        order.Confirm();

        await orderRepository.UpdateAsync(order, ct);
        await orderRepository.SaveChangesAsync(ct);

        return new ClientOrderSummaryDto(
            order.Id,
            order.Status,
            order.Total,
            order.CreatedAt,
            order.DispatchDate
        );
    }
}
