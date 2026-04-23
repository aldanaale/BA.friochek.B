using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Commands;

public record CreateOrderCommand(string NfcAccessToken, Guid UserId, Guid TenantId) : IRequest<Guid>;

/// <summary>
/// Maneja la creación de un nuevo pedido vinculando al cliente con un cooler via su token NFC.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly INfcTagRepository _nfcTagRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, INfcTagRepository nfcTagRepository)
    {
        _orderRepository = orderRepository;
        _nfcTagRepository = nfcTagRepository;
    }

    /// <summary>
    /// Busca el tag NFC, valida que esté enrolado y crea el pedido asociado al cooler.
    /// </summary>
    /// <param name="request">Comando con el token NFC, ID de usuario e ID de tenant.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El ID del pedido recién creado.</returns>
    /// <exception cref="ArgumentException">Si algún ID requerido es vacío o el token NFC es nulo/vacío.</exception>
    /// <exception cref="KeyNotFoundException">Si el tag NFC no existe o no está enrolado en el tenant.</exception>
    /// <exception cref="InvalidOperationException">Si el tag NFC no está en estado enrolado.</exception>
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(request.NfcAccessToken, nameof(request.NfcAccessToken));

        if (request.UserId == Guid.Empty)
            throw new ArgumentException("El ID de usuario no puede ser vacío.", nameof(request.UserId));

        if (request.TenantId == Guid.Empty)
            throw new ArgumentException("El ID de tenant no puede ser vacío.", nameof(request.TenantId));

        var tag = await _nfcTagRepository.GetByTagIdAsync(request.NfcAccessToken, request.TenantId, ct)
            ?? throw new KeyNotFoundException($"Tag NFC '{request.NfcAccessToken}' no encontrado o no está enrolado en el tenant.");

        if (!tag.IsEnrolled)
            throw new InvalidOperationException("El tag NFC no está enrolado y no puede generar pedidos.");

        var order = Order.Create(request.UserId, tag.CoolerId, tag.TagId, request.TenantId);
        await _orderRepository.AddAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);
        return order.Id;
    }
}
