using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

/// <summary>
/// Contrato para operaciones de lectura/escritura sobre perfiles Supervisor.
/// </summary>
public interface ISupervisorRepository
{
    Task<Supervisor?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<Supervisor>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Supervisor supervisor, CancellationToken ct = default);
    Task UpdateAsync(Supervisor supervisor, CancellationToken ct = default);
}
