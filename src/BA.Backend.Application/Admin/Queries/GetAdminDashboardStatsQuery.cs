using BA.Backend.Application.Admin.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Admin.Queries;

public record GetAdminDashboardStatsQuery(Guid TenantId) : IRequest<AdminDashboardStatsDto>;

public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetAdminDashboardStatsQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken ct)
    {
        var stats = await _dashboardRepository.GetAdminDashboardStatsAsync(request.TenantId, ct);

        return new AdminDashboardStatsDto(
            stats.ActiveCoolers,
            stats.MermasToday,
            stats.PendingTickets,
            stats.TotalStores
        );
    }
}
