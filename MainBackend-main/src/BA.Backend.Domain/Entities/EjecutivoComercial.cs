using System;
using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

/// <summary>
/// Perfil extendido para usuarios con rol EjecutivoComercial.
/// Gestiona la relación con clientes, genera órdenes y hace seguimiento del pipeline de ventas.
/// Tabla: EjecutivosComerciales (PK = UserId)
/// </summary>
public class EjecutivoComercial : ITenantEntity
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Territorio o región comercial asignada al ejecutivo.</summary>
    public string? Territory { get; set; }

    /// <summary>Si el ejecutivo está activo y puede operar en el sistema.</summary>
    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Navegación
    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
