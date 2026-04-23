using BA.Backend.Domain.Enums;

namespace BA.Backend.Application.Users.DTOs;

/// <summary>
/// Información detallada de un usuario del sistema.
/// </summary>
public class UserDto
{
    /// <summary>ID único del usuario.</summary>
    public Guid Id { get; set; }
    /// <summary>Correo electrónico institucional.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Nombre completo.</summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>Rol asignado en el sistema.</summary>
    public UserRole Role { get; set; }
    /// <summary>Indica si la cuenta está habilitada.</summary>
    public bool IsActive { get; set; }
    /// <summary>Indica si la cuenta está bloqueada por intentos fallidos.</summary>
    public bool IsLocked { get; set; }
    /// <summary>Fecha y hora del último acceso.</summary>
    public DateTime? LastLoginAt { get; set; }
}
