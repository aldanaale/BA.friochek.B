using BA.Backend.Application.Cliente.DTOs;
using MediatR;
using System;

namespace BA.Backend.Application.Cliente.Queries;

public record GetClientHomeQuery(Guid UserId, Guid TenantId) : IRequest<ClientHomeDto>;
