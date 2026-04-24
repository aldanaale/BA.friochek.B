using System;
using MediatR;

namespace BA.Backend.Application.Transportista.Commands;

public record CreateTransportistaCommand(Guid TenantId, Guid UserId, string? VehiclePlate) : IRequest<Guid>;
public record UpdateTransportistaCommand(Guid Id, Guid TenantId, bool IsAvailable, string? VehiclePlate) : IRequest<bool>;
