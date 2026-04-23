using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Infrastructure.Repositories;

/// <summary>
/// FIX: reemplazados todos los Console.WriteLine por ILogger estructurado.
/// </summary>
public class StoreRepository : IStoreRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StoreRepository> _logger;

    public StoreRepository(ApplicationDbContext context, ILogger<StoreRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        _logger.LogDebug("GetByIdAsync store {StoreId}", id);
        return await _context.Stores
            .Include(s => s.Coolers)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IEnumerable<Store>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct)
    {
        _logger.LogDebug("GetByTenantIdAsync tenant {TenantId}", tenantId);
        return await _context.Stores
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Store store, CancellationToken ct)
    {
        _logger.LogDebug("AddAsync store '{Name}'", store.Name);
        _context.Stores.Add(store);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Store store, CancellationToken ct)
    {
        _logger.LogDebug("UpdateAsync store {StoreId}", store.Id);
        _context.Stores.Update(store);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _logger.LogDebug("DeleteAsync store {StoreId}", id);
        var store = await _context.Stores.FindAsync(new object[] { id }, ct);
        if (store != null)
        {
            _context.Stores.Remove(store);
            await _context.SaveChangesAsync(ct);
        }
    }
}
