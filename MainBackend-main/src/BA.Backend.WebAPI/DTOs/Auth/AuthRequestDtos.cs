namespace BA.Backend.WebAPI.DTOs.Auth;

// La definición de LoginRequestDto fue movida a su propio archivo LoginRequestDto.cs

public record ForgotPasswordRequestDto
{
    /// <example>admin@savory.cl</example>
    public string Email { get; set; } = null!;
}

public record ResetPasswordRequestDto
{
    /// <example>tok_123abc456def</example>
    public string Token { get; init; } = null!;

    /// <example>NewPass123!</example>
    public string NewPassword { get; init; } = null!;

    /// <example>NewPass123!</example>
    public string ConfirmPassword { get; init; } = null!;
}
