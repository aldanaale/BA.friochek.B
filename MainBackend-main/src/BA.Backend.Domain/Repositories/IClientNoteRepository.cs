using BA.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Domain.Repositories;

public interface IClientNoteRepository
{
    Task<ClientNote?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ClientNote>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default);
    Task AddAsync(ClientNote note, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
