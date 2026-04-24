using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class Store : IBaseEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string District { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    // IBaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navegación
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Cooler> Coolers { get; set; } = new List<Cooler>();
}
