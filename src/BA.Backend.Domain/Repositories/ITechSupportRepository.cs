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
    
    Task SaveChangesAsync(CancellationToken ct = default);
}
