using System;
using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

/// <summary>
/// Historial completo de sesiones con auditoría de dispositivos.
/// FIX: propiedad de navegación `user` (minúscula) renombrada a `User` (PascalCase)
/// para seguir la convención C# y evitar confusión al configurar EF Core.
/// JwtToken eliminado — duplicaba AccessToken.
/// </summary>
public class UserSession : ITenantEntity
{
    /// <summary>ID único de la sesión.</summary>
    public Guid Id { get; set; }

    /// <summary>Usuario al que pertenece esta sesión.</summary>
    public Guid UserId { get; set; }

    /// <summary>Tenant del usuario.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Identificador lógico del dispositivo (fingerprint o UUID generado).</summary>
    public string DeviceId { get; set; } = null!;

    /// <summary>Hash SHA-256 del User-Agent + Accept-Language.</summary>
    public string DeviceFingerprint { get; set; } = null!;

    /// <summary>Token JWT de acceso.</summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>Momento en que se emitió el token.</summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>Expiración del token.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Última vez que esta sesión realizó una petición.</summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>Fecha de creación del registro.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Indica si la sesión sigue activa.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Razón por la que se invalidó (si corresponde).</summary>
    public string? InvalidationReason { get; set; }

    /// <summary>Razón de cierre de sesión voluntario.</summary>
    public string? ClosureReason { get; set; }

    /// <summary>Timestamp de invalidación.</summary>
    public DateTime? InvalidatedAt { get; set; }

    /// <summary>Timestamp de cierre.</summary>
    public DateTime? ClosedAt { get; set; }

    // ── Navegación ──────────────────────────────────────────────────────────
    // FIX: era `user` (minúscula). EF Core usa convención PascalCase.

    /// <summary>Referencia al usuario propietario de la sesión.</summary>
    public User User { get; set; } = null!;
}
