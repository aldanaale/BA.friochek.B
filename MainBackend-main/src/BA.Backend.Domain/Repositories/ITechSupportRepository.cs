using BA.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Domain.Repositories;

public interface ITechSupportRepository
{
    Task<TechSupportRequest?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);

    Task AddAsync(TechSupportRequest request, CancellationToken ct = default);
    Task<(IEnumerable<TechSupportRequest> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct);
    Task<(IEnumerable<TechSupportRequest> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, Guid tenantId, int pageNumber, int pageSize, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct = default);
}
