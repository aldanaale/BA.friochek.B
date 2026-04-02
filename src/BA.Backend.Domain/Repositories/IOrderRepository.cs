using BA.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
