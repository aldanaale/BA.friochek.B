using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

public interface IOperationCertificateRepository
{
    Task<OperationCertificate?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(OperationCertificate certificate, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
