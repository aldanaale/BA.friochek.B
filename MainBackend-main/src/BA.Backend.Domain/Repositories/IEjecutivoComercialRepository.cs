using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

/// <summary>
/// Contrato para operaciones de lectura/escritura sobre perfiles EjecutivoComercial.
/// </summary>
public interface IEjecutivoComercialRepository
{
    Task<EjecutivoComercial?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<EjecutivoComercial>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(EjecutivoComercial ejecutivo, CancellationToken ct = default);
    Task UpdateAsync(EjecutivoComercial ejecutivo, CancellationToken ct = default);
}
