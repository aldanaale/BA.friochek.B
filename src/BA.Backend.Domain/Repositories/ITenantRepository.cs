using BA.Backend.Domain.Entities;
namespace BA.Backend.Domain.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct);
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken ct);

    Task AddAsync(Tenant tenant, CancellationToken ct);
    Task UpdateAsync(Tenant tenant, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
