using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

/// <summary>
/// Productos disponibles por Tenant.
/// FIX F7.6: implementa ITenantEntity para que el Global Query Filter
/// aplique el filtro de tenant automáticamente (antes no se aplicaba).
/// </summary>
public class Product : IBaseEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }  // ← ITenantEntity
    public string Name { get; set; } = null!;

    /// <summary>Valores válidos: Venta | Servicio | Insumo</summary>
    public string Type { get; set; } = "Venta";
    public int Price { get; set; }
    public string? ExternalSku { get; set; } // Referencia al sistema externo (Savory/Mayorista)
    public int Stock { get; set; } = 0; // Stock disponible para entrega
    public bool IsActive { get; set; } = true;

    // ── IBaseEntity ──────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // ── Navegación ──────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
}
