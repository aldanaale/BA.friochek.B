using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class RouteRepository : IRouteRepository
{
    private readonly ApplicationDbContext _context;

    public RouteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Route>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct)
    {
        return await _context.Routes
            .Include(r => r.Transportista)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Store)

            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Route>> GetByTransportistaAsync(Guid transportistaId, Guid tenantId, CancellationToken ct)
    {
        return await _context.Routes
            .Include(r => r.Transportista)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Store)
            .Where(r => r.TransportistaId == transportistaId && r.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<Route?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Routes.FindAsync(new object[] { id }, ct);
    }

    public async Task<Route?> GetByIdWithStopsAsync(Guid id, CancellationToken ct)
    {
        return await _context.Routes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AddAsync(Route route, CancellationToken ct)
    {
        _context.Routes.Add(route);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Route route, CancellationToken ct)
    {
        _context.Routes.Update(route);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var route = await _context.Routes.FindAsync(new object[] { id }, ct);
        if (route != null)
        {
            _context.Routes.Remove(route);
            await _context.SaveChangesAsync(ct);
        }
    }
}