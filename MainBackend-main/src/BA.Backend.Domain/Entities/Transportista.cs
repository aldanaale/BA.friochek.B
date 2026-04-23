using System;
using BA.Backend.Domain.Common;
using BA.Backend.Domain.Enums;

namespace BA.Backend.Domain.Entities;

public class Transportista : ITenantEntity
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? VehiclePlate { get; set; }

    public TransportType? TransportType { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
