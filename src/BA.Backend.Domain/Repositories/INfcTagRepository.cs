using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

public interface INfcTagRepository
{
    Task<NfcTag?> GetByTagIdAsync(string tagId, Guid tenantId, CancellationToken ct);
    Task<NfcTag?> GetByTagIdAsync(string tagId, CancellationToken ct);
    Task<NfcTag?> GetByCoolerIdAsync(Guid coolerId, Guid tenantId, CancellationToken ct);
    Task AddAsync(NfcTag nfcTag, CancellationToken ct);
    Task UpdateAsync(NfcTag nfcTag, CancellationToken ct);
    Task DeleteAsync(string tagId, CancellationToken ct);
}
