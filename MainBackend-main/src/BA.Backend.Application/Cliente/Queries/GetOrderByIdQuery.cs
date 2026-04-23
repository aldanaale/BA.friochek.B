using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Queries;

public record GetOrderByIdQuery(Guid OrderId, Guid TenantId) : IRequest<ClientOrderDto?>;

/// <summary>
/// Maneja la consulta del detalle completo de un pedido, incluyendo sus ítems.
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ClientOrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Retorna el detalle del pedido o <c>null</c> si no existe en el tenant indicado.
    /// </summary>
    /// <param name="request">Query con el ID del pedido y el ID de tenant.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>DTO con el detalle del pedido e ítems, o <c>null</c> si no se encontró.</returns>
    /// <exception cref="ArgumentException">Si algún ID requerido es vacío.</exception>
    public async Task<ClientOrderDto?> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        if (request.OrderId == Guid.Empty)
            throw new ArgumentException("El ID del pedido no puede ser vacío.", nameof(request.OrderId));

        if (request.TenantId == Guid.Empty)
            throw new ArgumentException("El ID de tenant no puede ser vacío.", nameof(request.TenantId));

        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, request.TenantId, ct);
        if (order is null) return null;

        return new ClientOrderDto
        {
            Id = order.Id,
            Status = order.Status,
            Total = order.Items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = order.CreatedAt,
            DispatchDate = order.DispatchDate,
            Items = order.Items.Select(i => new ClientOrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        };
    }
}
