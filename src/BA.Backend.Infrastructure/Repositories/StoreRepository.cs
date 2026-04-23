using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class StoreRepository(ApplicationDbContext context) : IStoreRepository
{
    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        Console.WriteLine("Repositorio: Buscando tienda con ID: " + id);
        return await context.Stores
            .Include(s => s.Coolers)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IEnumerable<Store>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct)
    {
        Console.WriteLine("Repositorio: Cargando tiendas del tenant: " + tenantId);
        return await context.Stores
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Store store, CancellationToken ct)
    {
        Console.WriteLine("Repositorio: Agregando nueva tienda: " + store.Name);
        context.Stores.Add(store);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Store store, CancellationToken ct)
    {
        Console.WriteLine("Repositorio: Actualizando tienda ID: " + store.Id);
        context.Stores.Update(store);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        Console.WriteLine("Repositorio: Eliminando tienda ID: " + id);
        
        var store = await context.Stores.FindAsync([id], cancellationToken: ct);
        
        if (store != null)
        {
            context.Stores.Remove(store);
            await context.SaveChangesAsync(ct);
            Console.WriteLine("Repositorio: Tienda eliminada correctamente");
        }
    }
}
