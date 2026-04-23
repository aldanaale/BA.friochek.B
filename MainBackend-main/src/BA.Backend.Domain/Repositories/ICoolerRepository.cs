using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

public interface ICoolerRepository
{
    Task<IEnumerable<Cooler>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct);
    Task<Cooler?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Cooler?> GetByIdWithTenantAsync(Guid id, Guid tenantId, CancellationToken ct);
    Task<Cooler?> GetBySerialNumberAsync(string serialNumber, CancellationToken ct);
    Task<IEnumerable<Cooler>> GetByStoreIdAsync(Guid storeId, CancellationToken ct);
    Task AddAsync(Cooler cooler, CancellationToken ct);
    Task UpdateAsync(Cooler cooler, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
