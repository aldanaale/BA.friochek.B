using BA.Backend.Application.Routes.DTOs;
using MediatR;
using System.Collections.Generic;

namespace BA.Backend.Application.Routes.Queries;

public record GetRoutesQuery(Guid TenantId, Guid? TransportistaId = null) : IRequest<IEnumerable<RouteDto>>;

public record GetRouteByIdQuery(Guid Id, Guid TenantId) : IRequest<RouteDto?>;