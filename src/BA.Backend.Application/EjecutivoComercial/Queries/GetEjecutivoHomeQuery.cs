using BA.Backend.Application.EjecutivoComercial.DTOs;
using MediatR;
using System;

namespace BA.Backend.Application.EjecutivoComercial.Queries;

public record GetEjecutivoHomeQuery(Guid UserId, Guid TenantId) : IRequest<EjecutivoDashboardDto>;
