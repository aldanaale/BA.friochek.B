using BA.Backend.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace BA.Backend.Domain.Entities;

public class NfcTag : IBaseEntity, ITenantEntity
{
    [Key]
    public string TagId { get; set; } = null!;

    public Guid TenantId { get; set; }
    public Guid CoolerId { get; set; }

    public string SecurityHash { get; set; } = null!;

    public bool IsEnrolled { get; set; } = false;

    /// <summary>
    /// Estado del tag. Valores válidos: Pendiente | Instalado | Activo | Inactivo | Danado | DadoDeBaja
    /// </summary>
    public string Status { get; set; } = "Pendiente";

    public DateTime? EnrolledAt { get; set; }

    // IBaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navegación
    public Cooler? Cooler { get; set; }
}
