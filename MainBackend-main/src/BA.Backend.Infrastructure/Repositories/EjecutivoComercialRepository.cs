using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class EjecutivoComercialRepository : IEjecutivoComercialRepository
{
    private readonly ApplicationDbContext _db;

    public EjecutivoComercialRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<EjecutivoComercial?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.EjecutivosComerciales
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

    public async Task<List<EjecutivoComercial>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.EjecutivosComerciales
            .Include(e => e.User)
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task AddAsync(EjecutivoComercial ejecutivo, CancellationToken ct = default)
    {
        ejecutivo.CreatedAt = DateTime.UtcNow;
        await _db.EjecutivosComerciales.AddAsync(ejecutivo, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(EjecutivoComercial ejecutivo, CancellationToken ct = default)
    {
        _db.EjecutivosComerciales.Update(ejecutivo);
        await _db.SaveChangesAsync(ct);
    }
}
