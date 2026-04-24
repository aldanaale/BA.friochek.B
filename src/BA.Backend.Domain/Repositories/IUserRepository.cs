using BA.Backend.Domain.Entities;
namespace BA.Backend.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct);
    Task<User?> GetGlobalByEmailAsync(string email, CancellationToken ct);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken ct);
}
