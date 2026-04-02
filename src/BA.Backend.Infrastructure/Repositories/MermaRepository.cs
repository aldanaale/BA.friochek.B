using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Infrastructure.Repositories;

/// <summary>
/// Implementación basada en EF Core del repositorio de mermas.
/// </summary>
public class MermaRepository : IMermaRepository
{
    private readonly ApplicationDbContext _context;

    public MermaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Merma merma, CancellationToken ct)
    {
        await _context.Mermas.AddAsync(merma, ct);
    }

    public async Task<NfcTag?> GetInstalledNfcTagByCoolerAsync(Guid coolerId, CancellationToken ct)
    {
        return await _context.NfcTags
            .FirstOrDefaultAsync(n => n.CoolerId == coolerId && n.IsEnrolled == true, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}
