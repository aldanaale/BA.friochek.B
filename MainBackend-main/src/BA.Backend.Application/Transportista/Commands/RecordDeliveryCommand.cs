using MediatR;
using System;
using System.Collections.Generic;
using BA.Backend.Application.Transportista.DTOs;

namespace BA.Backend.Application.Transportista.Commands;

public record DeliveryCommand(
    Guid RouteStopId,
    string NfcAccessToken,
    double Latitude,
    double Longitude,
    string? SignatureBase64 = null
) : IRequest<bool>;
