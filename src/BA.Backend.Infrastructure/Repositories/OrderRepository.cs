using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, ct);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId && o.TenantId == tenantId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(id, tenantId, ct);
        if (order != null)
        {
            _context.Orders.Remove(order);
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
