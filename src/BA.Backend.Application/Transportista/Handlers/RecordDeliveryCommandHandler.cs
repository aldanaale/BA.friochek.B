using MediatR;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Exceptions;
using BA.Backend.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BA.Backend.Application.Transportista.Handlers;

public class DeliveryCommandHandler : IRequestHandler<DeliveryCommand, bool>
{
    private readonly IDeliveryRepository _repository;
    private readonly IJwtTokenService _jwtService;

    public DeliveryCommandHandler(IDeliveryRepository repository, IJwtTokenService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    public async Task<bool> Handle(DeliveryCommand request, CancellationToken ct)
    {
        var order = await _repository.GetOrderByIdAsync(request.OrderId, ct);
        var stop = await _repository.GetRouteStopByIdAsync(request.RouteStopId, ct);

        if (order == null || stop == null)
            throw new DomainException("NOT_FOUND", "No se encontró el pedido o la parada de ruta especificada.");

        var nfcValidation = _jwtService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null)
            throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        if (order.CoolerId != nfcValidation.CoolerId)
            throw new DomainException("NFC_MISMATCH", "El tag escaneado no corresponde al cooler de este pedido.");

        var deliveredQuantities = request.DeliveredItems?
            .ToDictionary(i => i.OrderItemId, i => i.Quantity);

        order.MarkAsDelivered(nfcValidation.TagId, deliveredQuantities);
        stop.MarkAsCompleted();

        await _repository.SaveChangesAsync(ct);
        
        return true;
    }
}

