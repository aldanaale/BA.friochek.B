using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class SupervisorRepository : ISupervisorRepository
{
    private readonly ApplicationDbContext _db;

    public SupervisorRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Supervisor?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Supervisores
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<List<Supervisor>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Supervisores
            .Include(s => s.User)
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task AddAsync(Supervisor supervisor, CancellationToken ct = default)
    {
        supervisor.CreatedAt = DateTime.UtcNow;
        await _db.Supervisores.AddAsync(supervisor, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Supervisor supervisor, CancellationToken ct = default)
    {
        _db.Supervisores.Update(supervisor);
        await _db.SaveChangesAsync(ct);
    }
}
