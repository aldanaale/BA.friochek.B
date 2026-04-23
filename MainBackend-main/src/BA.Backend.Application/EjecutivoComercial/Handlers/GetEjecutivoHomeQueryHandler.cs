using BA.Backend.Application.EjecutivoComercial.DTOs;
using BA.Backend.Application.EjecutivoComercial.Queries;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.EjecutivoComercial.Handlers;

public class GetEjecutivoHomeQueryHandler : IRequestHandler<GetEjecutivoHomeQuery, EjecutivoDashboardDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetEjecutivoHomeQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<EjecutivoDashboardDto> Handle(GetEjecutivoHomeQuery request, CancellationToken cancellationToken)
    {
        var stats = await _dashboardRepository.GetEjecutivoDashboardStatsAsync(request.UserId, request.TenantId, cancellationToken);

        return new EjecutivoDashboardDto
        {
            ActiveClients = stats.ActiveClients
        };
    }
}
