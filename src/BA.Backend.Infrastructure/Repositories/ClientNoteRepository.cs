using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Infrastructure.Repositories;

public class ClientNoteRepository : IClientNoteRepository
{
    private readonly ApplicationDbContext _context;

    public ClientNoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientNote?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ClientNotes.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<ClientNote>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
    {
        return await _context.ClientNotes
            .Where(n => n.StoreId == storeId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ClientNote note, CancellationToken ct = default)
    {
        await _context.ClientNotes.AddAsync(note, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
