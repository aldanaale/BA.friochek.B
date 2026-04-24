namespace BA.Backend.Application.Common.DTOs;

/// <summary>
/// DTO compartido con datos basicos de usuario.
/// Reemplaza las definiciones duplicadas de UserDto en:
///   - Cliente/DTOs/ClientHomeDto.cs
///   - cualquier otro namespace que necesite nombre + apellido + email
/// </summary>
public class UserSummaryDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
