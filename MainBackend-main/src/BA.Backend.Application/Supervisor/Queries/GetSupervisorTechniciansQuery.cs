using BA.Backend.Application.Supervisor.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Supervisor.Queries;

public record GetSupervisorTechniciansQuery(Guid TenantId) : IRequest<List<TechnicianWorkloadDto>>;
