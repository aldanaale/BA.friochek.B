using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class OperationCertificateRepository : IOperationCertificateRepository
{
    private readonly ApplicationDbContext _context;

    public OperationCertificateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationCertificate?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.OperationCertificates
            .Include(c => c.User)
            .Include(c => c.Tenant)
            .Include(c => c.RouteStop)
                .ThenInclude(rs => rs.Store)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(OperationCertificate certificate, CancellationToken ct)
    {
        await _context.OperationCertificates.AddAsync(certificate, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}
