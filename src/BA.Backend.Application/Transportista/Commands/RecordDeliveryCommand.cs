using MediatR;
using System;
using System.Collections.Generic;
using BA.Backend.Application.Transportista.DTOs;

namespace BA.Backend.Application.Transportista.Commands;

public record DeliveryCommand(
    Guid OrderId,
    Guid RouteStopId,
    string NfcAccessToken,
    List<DeliveredItemDto>? DeliveredItems = null
) : IRequest<bool>;
