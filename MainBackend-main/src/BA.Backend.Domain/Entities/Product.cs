using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class Product : IBaseEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = "Venta";
    public int Price { get; set; }
    public string? ExternalSku { get; set; }
    public int Stock { get; set; } = 0;
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
}
