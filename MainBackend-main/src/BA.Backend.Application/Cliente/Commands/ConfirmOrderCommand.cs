using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Commands;

public record ConfirmOrderCommand(Guid OrderId, Guid TenantId) : IRequest<ClientOrderSummaryDto>;

/// <summary>
/// Maneja la confirmación de un pedido, cerrando su composición para proceder al despacho.
/// </summary>
public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, ClientOrderSummaryDto>
{
    private readonly IOrderRepository _orderRepository;

    public ConfirmOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Confirma el pedido y devuelve un resumen con el total calculado.
    /// </summary>
    /// <param name="request">Comando con el ID del pedido y el ID de tenant.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resumen del pedido confirmado, incluyendo total e ítems.</returns>
    /// <exception cref="ArgumentException">Si algún ID requerido es vacío.</exception>
    /// <exception cref="KeyNotFoundException">Si el pedido no existe en el tenant indicado.</exception>
    public async Task<ClientOrderSummaryDto> Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
        if (request.OrderId == Guid.Empty)
            throw new ArgumentException("El ID del pedido no puede ser vacío.", nameof(request.OrderId));

        if (request.TenantId == Guid.Empty)
            throw new ArgumentException("El ID de tenant no puede ser vacío.", nameof(request.TenantId));

        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, request.TenantId, ct)
            ?? throw new KeyNotFoundException($"Pedido {request.OrderId} no encontrado en el tenant indicado.");

        order.Confirm();
        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        return new ClientOrderSummaryDto
        {
            OrderId = order.Id,
            Status = order.Status,
            Total = order.Items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = order.CreatedAt,
            DispatchDate = order.DispatchDate
        };
    }
}
