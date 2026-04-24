using BA.Backend.Application.Supervisor.DTOs;
using MediatR;
using System;

namespace BA.Backend.Application.Supervisor.Queries;

public record GetSupervisorHomeQuery(Guid UserId, Guid TenantId) : IRequest<SupervisorDashboardDto>;
