using System;
using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class UserSession : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string DeviceId { get; set; } = null!;
    public string DeviceFingerprint { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? InvalidationReason { get; set; }
    public string? ClosureReason { get; set; }
    public DateTime? InvalidatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public User User { get; set; } = null!;
}
