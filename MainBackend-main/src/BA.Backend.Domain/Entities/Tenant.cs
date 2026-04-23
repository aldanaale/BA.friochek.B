using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class Tenant : IBaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public bool IsActive { get; set; }
    
    // Configuración del Hub de Integración Universal
    public int IntegrationType { get; set; } = 0; // 0: Manual/Interno, 1: Savory, 2: RestAPI
    public string? IntegrationConfigJson { get; set; } // Credenciales/Parámetros (JSON)
    public string? ExternalOrderUrl { get; set; } // Link al portal oficial del Tenant
    public string? RedirectTemplate { get; set; } // Plantilla dinámica para Deep Linking (ej: portal.cl/buy?sku={sku})

    // ── IBaseEntity ──────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
