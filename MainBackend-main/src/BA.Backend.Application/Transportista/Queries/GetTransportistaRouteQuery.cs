using MediatR;
using BA.Backend.Application.Transportista.DTOs;
using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Transportista.Queries;

public record GetRouteQuery(
    Guid TransportistaId,
    Guid TenantId) : IRequest<List<TransportistaRouteDto>>;

