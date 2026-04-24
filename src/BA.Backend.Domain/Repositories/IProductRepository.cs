using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    void Update(Product product);
    void Add(Product product);
}
