using System;
using BA.Backend.Domain.Common;
using BA.Backend.Domain.Enums;

namespace BA.Backend.Domain.Entities;

/// <summary>
/// Perfil extendido para usuarios con rol Supervisor.
/// Supervisa operaciones SDA, visitas técnicas y alertas de demanda.
/// Puede aprobar casos de arriendo dentro de su zona/tenant.
/// Tabla: Supervisores (PK = UserId)
/// </summary>
public class Supervisor : ITenantEntity
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Zona geográfica o área de responsabilidad del supervisor.</summary>
    public string? Zone { get; set; }

    /// <summary>Si el supervisor está activo/disponible para recibir alertas.</summary>
    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Navegación
    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
