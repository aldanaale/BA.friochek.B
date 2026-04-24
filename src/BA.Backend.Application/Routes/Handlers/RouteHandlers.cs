using BA.Backend.Application.Routes.Commands;
using BA.Backend.Application.Routes.DTOs;
using BA.Backend.Application.Routes.Queries;
using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Routes.Handlers;

public class RouteHandlers :
    IRequestHandler<GetRoutesQuery, IEnumerable<RouteDto>>,
    IRequestHandler<GetRouteByIdQuery, RouteDto?>,
    IRequestHandler<CreateRouteCommand, Guid>,
    IRequestHandler<UpdateRouteStatusCommand, bool>,
    IRequestHandler<DeleteRouteCommand, bool>
{
    private readonly IRouteRepository _routeRepository;
    private readonly IUserRepository _userRepository;

    public RouteHandlers(
        IRouteRepository routeRepository,
        IUserRepository userRepository)
    {
        _routeRepository = routeRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<RouteDto>> Handle(GetRoutesQuery request, CancellationToken ct)
    {
        var routes = request.TransportistaId.HasValue
            ? await _routeRepository.GetByTransportistaAsync(request.TransportistaId.Value, request.TenantId, ct)
            : await _routeRepository.GetAllByTenantAsync(request.TenantId, ct);

        return routes.Select(MapToDto);
    }

    public async Task<RouteDto?> Handle(GetRouteByIdQuery request, CancellationToken ct)
    {
        var route = await _routeRepository.GetByIdWithStopsAsync(request.Id, ct);
        if (route == null || route.TenantId != request.TenantId) return null;
        return MapToDto(route);
    }

    public async Task<Guid> Handle(CreateRouteCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.TransportistaId, request.TenantId, ct);
        if (user == null || user.TenantId != request.TenantId)
            throw new KeyNotFoundException("Transportista no encontrado o no pertenece al tenant");

        var route = Route.Create(request.TenantId, request.TransportistaId, request.Date);

        foreach (var stopDto in request.Stops)
        {
            route.AddStop(stopDto.StoreId, stopDto.StopOrder);
        }

        await _routeRepository.AddAsync(route, ct);
        return route.Id;
    }

    public async Task<bool> Handle(UpdateRouteStatusCommand request, CancellationToken ct)
    {
        var route = await _routeRepository.GetByIdAsync(request.Id, ct);
        if (route == null || route.TenantId != request.TenantId) return false;

        var validStatuses = new[] { "Pendiente", "EnProgreso", "Completada", "Cancelada" };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException("Status inválido");

        route.UpdateStatus(request.Status);
        await _routeRepository.UpdateAsync(route, ct);
        return true;
    }

    public async Task<bool> Handle(DeleteRouteCommand request, CancellationToken ct)
    {
        var route = await _routeRepository.GetByIdAsync(request.Id, ct);
        if (route == null || route.TenantId != request.TenantId) return false;

        await _routeRepository.DeleteAsync(request.Id, ct);
        return true;
    }

    private static RouteDto MapToDto(Route route)
    {
        return new RouteDto(
            route.Id,
            route.TenantId,
            route.TransportistaId,
            route.Transportista?.FullName ?? "Unknown",
            route.Date,
            route.Status,
            route.CreatedAt,
            route.Stops?.Select(s => new RouteStopDto(
                s.Id, s.StoreId, s.Store?.Name ?? "Unknown",
                s.StopOrder, s.Status, s.ArrivalAt, s.Notes
            )).ToList() ?? new List<RouteStopDto>()
        );
    }
}