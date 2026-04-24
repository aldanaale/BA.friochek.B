using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class OperationCertificate : IBaseEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RouteStopId { get; set; }
    public Guid UserId { get; set; }

    public string SignatureBase64 { get; set; } = null!;
    public string IpAddress { get; set; } = null!;
    public string DeviceFingerprint { get; set; } = null!;
    public string ServerHash { get; set; } = null!;
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public DateTime AcceptanceTimestamp { get; set; } = DateTime.UtcNow;

    // IBaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navegación
    public Tenant Tenant { get; set; } = null!;
    public RouteStop RouteStop { get; set; } = null!;
    public User User { get; set; } = null!;
}
