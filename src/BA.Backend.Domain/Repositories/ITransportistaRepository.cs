using BA.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Domain.Repositories;

public interface ITransportistaRepository
{
    Task<IEnumerable<Transportista>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct);
    Task<Transportista?> GetByIdAsync(Guid userId, CancellationToken ct);
    Task AddAsync(Transportista transportista, CancellationToken ct);
    Task UpdateAsync(Transportista transportista, CancellationToken ct);
    Task DeleteAsync(Guid userId, CancellationToken ct);
}