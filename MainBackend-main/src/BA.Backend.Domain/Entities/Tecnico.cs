using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class Tecnico : ITenantEntity
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Specialty { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
