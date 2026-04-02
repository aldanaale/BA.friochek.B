using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Infrastructure.Repositories;

/// <summary>
/// Implementación basada en EF Core del repositorio de entregas.
/// </summary>
public class DeliveryRepository : IDeliveryRepository
{
    private readonly ApplicationDbContext _context;

    public DeliveryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Orders.FindAsync(new object[] { id }, ct);
    }

    public async Task<RouteStop?> GetRouteStopByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.RouteStops.FindAsync(new object[] { id }, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}
