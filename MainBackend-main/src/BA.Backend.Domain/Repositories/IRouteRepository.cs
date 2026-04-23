using BA.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Domain.Repositories;

public interface IRouteRepository
{
    Task<IEnumerable<Route>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct);
    Task<IEnumerable<Route>> GetByTransportistaAsync(Guid transportistaId, Guid tenantId, CancellationToken ct);
    Task<Route?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Route?> GetByIdWithStopsAsync(Guid id, CancellationToken ct);
    Task AddAsync(Route route, CancellationToken ct);
    Task UpdateAsync(Route route, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}