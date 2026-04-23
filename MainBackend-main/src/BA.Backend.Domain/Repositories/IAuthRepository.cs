using BA.Backend.Domain.Models;

namespace BA.Backend.Domain.Repositories;

public interface IAuthRepository
{
    Task<LoginResult> GetLoginDataAsync(string email, string? tenantSlug, CancellationToken ct);
}
