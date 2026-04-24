using System;
using BA.Backend.Domain.Common;
using BA.Backend.Domain.Enums;

namespace BA.Backend.Domain.Entities;

public class Supervisor : ITenantEntity
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }

    public string? Zone { get; set; }
    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Navegación
    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
