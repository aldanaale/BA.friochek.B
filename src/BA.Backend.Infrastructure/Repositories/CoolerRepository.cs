using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class CoolerRepository : ICoolerRepository
{
    private readonly ApplicationDbContext _context;

    public CoolerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cooler?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Coolers
            .Include(c => c.NfcTag)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Cooler?> GetByIdWithTenantAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        return await _context.Coolers
            .Include(c => c.NfcTag)
            .Include(c => c.Store)
            .FirstOrDefaultAsync(c => c.Id == id && c.Store.TenantId == tenantId, ct);
    }

    public async Task<Cooler?> GetBySerialNumberAsync(string serialNumber, CancellationToken ct)
    {
        return await _context.Coolers
            .FirstOrDefaultAsync(c => c.SerialNumber == serialNumber, ct);
    }

    public async Task<IEnumerable<Cooler>> GetByStoreIdAsync(Guid storeId, CancellationToken ct)
    {
        return await _context.Coolers
            .Where(c => c.StoreId == storeId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Cooler cooler, CancellationToken ct)
    {
        _context.Coolers.Add(cooler);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Cooler cooler, CancellationToken ct)
    {
        _context.Coolers.Update(cooler);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var cooler = await _context.Coolers.FindAsync(new object[] { id }, cancellationToken: ct);
        if (cooler != null)
        {
            _context.Coolers.Remove(cooler);
            await _context.SaveChangesAsync(ct);
        }
    }
}
