using System;
using System.Collections.Generic;
using BA.Backend.Application.Transportista.DTOs;
using MediatR;

namespace BA.Backend.Application.Transportista.Queries;

public record GetAllTransportistasQuery(Guid TenantId) : IRequest<IEnumerable<TransportistaDto>>;
public record GetTransportistaByIdQuery(Guid Id, Guid TenantId) : IRequest<TransportistaDto?>;
