using System.ComponentModel.DataAnnotations;

namespace BA.Backend.Application.Users.DTOs;

/// <summary>
/// Parámetros para la creación de un nuevo usuario en el sistema.
/// </summary>
public class CreateUserDto
{
    /// <summary>Correo electrónico institucional. Ejemplo: usuario@empresa.com</summary>
    [Required(ErrorMessage = "Email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Nombre completo del usuario.</summary>
    [Required(ErrorMessage = "FullName es requerido")]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Contraseña de acceso (mínimo 8 caracteres).</summary>
    [Required(ErrorMessage = "Password es requerido")]
    [MinLength(8, ErrorMessage = "Password mínimo 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Rol del usuario en el sistema. 1: Admin, 2: Cliente, 3: Transportista, 4: Tecnico, 6: Supervisor, 7: Vendedor.</summary>
    [Required(ErrorMessage = "Role es requerido")]
    [Range(1, 7)]
    public int Role { get; set; }
}
