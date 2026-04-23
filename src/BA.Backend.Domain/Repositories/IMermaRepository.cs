using BA.Backend.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Domain.Repositories;

/// <summary>
/// Interfaz para la persistencia y consulta de mermas (productos retirados).
/// </summary>
public interface IMermaRepository
{
    Task AddAsync(Merma merma, CancellationToken ct);
    Task<NfcTag?> GetInstalledNfcTagByCoolerAsync(Guid coolerId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
