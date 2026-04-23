using BA.Backend.Application.Supervisor.DTOs;
using BA.Backend.Application.Supervisor.Queries;
using BA.Backend.Domain.Repositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Supervisor.Handlers;

public class GetSupervisorHomeQueryHandler : IRequestHandler<GetSupervisorHomeQuery, SupervisorDashboardDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetSupervisorHomeQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<SupervisorDashboardDto> Handle(GetSupervisorHomeQuery request, CancellationToken cancellationToken)
    {
        var stats = await _dashboardRepository.GetSupervisorDashboardStatsAsync(request.TenantId, null, cancellationToken);

        return new SupervisorDashboardDto
        {
            ActiveTechnicians = stats.ActiveTechnicians,
            PendingTickets = stats.PendingTickets,
            RepairsToday = stats.RepairsToday,
            RecentAlerts = stats.RecentAlerts.Select(a => new RecentAlertDto
            {
                Id = a.Id,
                Type = a.Type,
                Message = a.Message,
                CreatedAt = a.CreatedAt,
                Severity = a.Severity
            }).ToList(),
            // Para el MVP, TechnicianStatus se carga en una consulta separada o se mockea aquí
            TechnicianStatus = new() 
        };
    }
}
