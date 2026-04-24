using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Store>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct);
    Task AddAsync(Store store, CancellationToken ct);
    Task UpdateAsync(Store store, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
