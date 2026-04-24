using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace BA.Backend.Application.Transportista.Commands;

public record MermaCommand(
    Guid TenantId,
    Guid TransportistaId,
    Guid CoolerId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    string Reason,
    string Description,
    IFormFile Photo,
    string NfcAccessToken,
    double Latitude,
    double Longitude
) : IRequest<Guid>;

