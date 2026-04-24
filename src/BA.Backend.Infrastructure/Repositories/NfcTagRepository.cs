using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class NfcTagRepository : INfcTagRepository
{
    private readonly ApplicationDbContext _context;

    public NfcTagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NfcTag?> GetByTagIdAsync(string tagId, Guid tenantId, CancellationToken ct)
    {
        return await _context.NfcTags
            .Include(t => t.Cooler)
                .ThenInclude(c => c!.Store)
            .FirstOrDefaultAsync(t => t.TagId == tagId && t.Cooler!.Store!.TenantId == tenantId, ct);
    }

    public async Task<NfcTag?> GetByTagIdAsync(string tagId, CancellationToken ct)
    {
        return await _context.NfcTags
            .Include(t => t.Cooler)
                .ThenInclude(c => c!.Store)
            .FirstOrDefaultAsync(t => t.TagId == tagId, ct);
    }

    public async Task<NfcTag?> GetByCoolerIdAsync(Guid coolerId, Guid tenantId, CancellationToken ct)
    {
        return await _context.NfcTags
            .Include(t => t.Cooler)
                .ThenInclude(c => c!.Store)
            .FirstOrDefaultAsync(t => t.CoolerId == coolerId && t.Cooler!.Store!.TenantId == tenantId, ct);
    }

    public async Task AddAsync(NfcTag nfcTag, CancellationToken ct)
    {
        await _context.NfcTags.AddAsync(nfcTag, ct);
    }

    public Task UpdateAsync(NfcTag nfcTag, CancellationToken ct)
    {
        _context.NfcTags.Update(nfcTag);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string tagId, CancellationToken ct)
    {
        var nfcTag = await _context.NfcTags.FindAsync(new object[] { tagId }, cancellationToken: ct);
        if (nfcTag != null)
        {
            _context.NfcTags.Remove(nfcTag);
        }
    }
}
