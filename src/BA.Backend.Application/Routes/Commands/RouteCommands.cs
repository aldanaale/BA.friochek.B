using BA.Backend.Application.Routes.DTOs;
using MediatR;

namespace BA.Backend.Application.Routes.Commands;

public record CreateRouteCommand(
    Guid TenantId,
    Guid TransportistaId,
    DateTime Date,
    List<CreateRouteStopDto> Stops
) : IRequest<Guid>;

public record UpdateRouteStatusCommand(Guid Id, Guid TenantId, string Status) : IRequest<bool>;

public record DeleteRouteCommand(Guid Id, Guid TenantId) : IRequest<bool>;