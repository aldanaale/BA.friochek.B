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

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
