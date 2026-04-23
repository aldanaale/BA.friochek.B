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

    public async Task<IEnumerable<Cooler>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct)
    {
        return await _context.Coolers
            .Include(c => c.Store)
            .Where(c => tenantId == Guid.Empty || c.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<Cooler?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        return await _context.Coolers
            .Include(c => c.NfcTag)
            .FirstOrDefaultAsync(c => c.Id == id && (tenantId == Guid.Empty || c.TenantId == tenantId), ct);
    }

    public async Task<Cooler?> GetByIdWithTenantAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        return await _context.Coolers
            .Include(c => c.NfcTag)
            .Include(c => c.Store)
            .FirstOrDefaultAsync(c => c.Id == id && (tenantId == Guid.Empty || c.TenantId == tenantId), ct);
    }

    public async Task<Cooler?> GetBySerialNumberAsync(string serialNumber, Guid tenantId, CancellationToken ct)
    {
        return await _context.Coolers
            .FirstOrDefaultAsync(c => c.SerialNumber == serialNumber && (tenantId == Guid.Empty || c.TenantId == tenantId), ct);
    }

    public async Task<IEnumerable<Cooler>> GetByStoreIdAsync(Guid storeId, Guid tenantId, CancellationToken ct)
    {
        return await _context.Coolers
            .Where(c => c.StoreId == storeId && (tenantId == Guid.Empty || c.TenantId == tenantId))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Cooler cooler, CancellationToken ct)
    {
        _context.Coolers.Add(cooler);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Cooler cooler, CancellationToken ct)
    {
        // No llamamos a _context.Coolers.Update(cooler) para evitar actualizar el grafo completo (Store, etc)
        // EF Core ya rastrea los cambios al haber cargado la entidad previamente con tracking.
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
