using BA.Backend.Application.PlatformAdmin.Queries;
using BA.Backend.Domain.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.PlatformAdmin.Handlers;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, List<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<List<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        // El repositorio de Tenant no suele tener filtros globales restrictivos 
        // pero aquí nos aseguramos de traer todo.
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);

        return tenants.Select(t => new TenantDto(
            t.Id,
            t.Name,
            t.Slug,
            t.ExternalOrderUrl,
            t.IntegrationType,
            t.IsActive,
            t.CreatedAt
        )).ToList();
    }
}
