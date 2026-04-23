using BA.Backend.Domain.Entities;

namespace BA.Backend.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, Guid tenantId, string sessionId);
    (string Token, DateTime ExpiresAt) GenerateRefreshToken(Guid userId, Guid tenantId, string sessionId);
    TokenValidationResult? ValidateToken(string token);
    (string Token, DateTime ExpiresAt) GenerateNfcScanToken(string tagId, Guid coolerId, Guid storeId, Guid tenantId);
    NfcTokenValidationResult? ValidateNfcToken(string token);
}

public class TokenValidationResult
{
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = null!;
    public Guid TenantId { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "Cliente";
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class NfcTokenValidationResult
{
    public string TagId { get; set; } = null!;
    public Guid CoolerId { get; set; }
    public Guid StoreId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
