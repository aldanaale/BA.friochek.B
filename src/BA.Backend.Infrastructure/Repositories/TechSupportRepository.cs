using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Infrastructure.Repositories;

public class TechSupportRepository : ITechSupportRepository
{
    private readonly ApplicationDbContext _context;

    public TechSupportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TechSupportRequest?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        return await _context.Set<TechSupportRequest>()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    }

    public async Task AddAsync(TechSupportRequest request, CancellationToken ct = default)
    {
        await _context.Set<TechSupportRequest>().AddAsync(request, ct);
    }

    public async Task<(IEnumerable<TechSupportRequest> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = _context.Set<TechSupportRequest>().Where(x => x.TenantId == tenantId);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, totalCount);
    }

    public async Task<(IEnumerable<TechSupportRequest> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, Guid tenantId, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = _context.TechSupportRequests
            .Where(t => t.UserId == userId && t.TenantId == tenantId);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
