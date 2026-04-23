using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);
    }

    /// <summary>
    /// Búsqueda global de usuario (solo para administración de alto nivel).
    /// </summary>
    public async Task<User?> GetGlobalByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);
    }

    /// <summary>
    /// Búsqueda específica de usuario por email y TenantId.
    /// Utiliza IgnoreQueryFilters() para permitir la autenticación antes de que el 
    /// TenantId sea inyectado en el ClaimsPrincipal del contexto.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId && !u.IsDeleted, ct);
    }
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Users.ToListAsync(ct);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = _context.Users
            .Where(u => u.TenantId == tenantId && !u.IsDeleted);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        var user = await GetByIdAsync(id, tenantId, ct);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
        }
    }
}

