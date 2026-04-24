using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del pedido no puede ser vacío.", nameof(id));

        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del pedido no puede ser vacío.", nameof(id));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("El ID de tenant no puede ser vacío.", nameof(tenantId));

        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, ct);
    }

    public async Task<List<Order>> GetByUserIdAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El ID de usuario no puede ser vacío.", nameof(userId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("El ID de tenant no puede ser vacío.", nameof(tenantId));

        return await _context.Orders
            .Where(o => o.UserId == userId && o.TenantId == tenantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId, Guid tenantId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El ID de usuario no puede ser vacío.", nameof(userId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("El ID de tenant no puede ser vacío.", nameof(tenantId));

        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId && o.TenantId == tenantId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await _context.Orders.AddAsync(order, ct);

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
        => _context.Orders.Update(order);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del pedido no puede ser vacío.", nameof(id));

        var order = await _context.Orders.FindAsync(new object[] { id }, ct);
        if (order != null) _context.Orders.Remove(order);
    }

    public async Task<string> CreateExternalOrderReferenceAsync(
        Guid userId, Guid tenantId, Guid productId, string redirectUrl, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El ID de usuario no puede ser vacío.", nameof(userId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("El ID de tenant no puede ser vacío.", nameof(tenantId));

        if (productId == Guid.Empty)
            throw new ArgumentException("El ID de producto no puede ser vacío.", nameof(productId));

        try
        {
            var referenceId = Guid.NewGuid().ToString("N")[..12].ToUpper();

            // Order.CreateExternalReference sets CoolerId = Guid.Empty intentionally:
            // external orders do not require a physical cooler and exist only for payment tracking.
            var order = Order.CreateExternalReference(userId, tenantId, referenceId);

            await _context.Orders.AddAsync(order, ct);
            await _context.SaveChangesAsync(ct);
            return referenceId;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"No se pudo crear la referencia de pedido externo para el usuario {userId}. Verifique la conectividad con la base de datos.",
                ex);
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
