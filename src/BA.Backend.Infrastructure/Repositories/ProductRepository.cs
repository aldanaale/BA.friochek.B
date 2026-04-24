using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _context.Products
            .Where(p => p.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
    }
}
