using BA.Backend.Application.Supervisor.Queries;
using BA.Backend.Domain.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Supervisor.Handlers;

public class GetSupervisorTechniciansQueryHandler : IRequestHandler<GetSupervisorTechniciansQuery, List<TechnicianWorkloadDto>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetSupervisorTechniciansQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<List<TechnicianWorkloadDto>> Handle(GetSupervisorTechniciansQuery request, CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetTechnicianWorkloadsAsync(request.TenantId, cancellationToken);
    }
}
