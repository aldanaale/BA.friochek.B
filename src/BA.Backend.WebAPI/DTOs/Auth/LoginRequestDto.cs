namespace BA.Backend.WebAPI.DTOs.Auth;

/// <summary>
/// DTO para la solicitud de inicio de sesión simplificada.
/// El sistema detecta automáticamente la empresa en base al email único.
/// </summary>
public record LoginRequestDto
{
    /// <example>admin@savory.cl</example>
    public string Email { get; init; } = null!;

    /// <example>DevPass123!</example>
    public string Password { get; init; } = null!;
}
