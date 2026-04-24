using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Infrastructure.Repositories;

/// <summary>
/// Implementación basada en EF Core del repositorio de mermas.
/// </summary>
public class MermaRepository : IMermaRepository
{
    private readonly ApplicationDbContext _context;

    public MermaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Merma merma, CancellationToken ct)
    {
        await _context.Mermas.AddAsync(merma, ct);
    }

    public async Task<NfcTag?> GetInstalledNfcTagByCoolerAsync(Guid coolerId, CancellationToken ct)
    {
        return await _context.NfcTags
            .FirstOrDefaultAsync(n => n.CoolerId == coolerId && n.IsEnrolled == true, ct);
    }

    public async Task<(IEnumerable<Merma> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = _context.Mermas.Where(m => m.TenantId == tenantId);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, totalCount);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}
