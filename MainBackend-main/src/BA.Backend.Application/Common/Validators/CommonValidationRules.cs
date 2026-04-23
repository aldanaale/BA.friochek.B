using FluentValidation;

namespace BA.Backend.Application.Common.Validators;

/// <summary>
/// Reglas de validacion reutilizables para evitar duplicacion
/// entre LoginCommandValidator, ForgotPasswordCommandValidator,
/// ResetPasswordCommandValidator y otros futuros validators.
/// </summary>
public static class CommonValidationRules
{
    /// <summary>
    /// Aplica validacion de email: NotEmpty + formato valido.
    /// Usar con: RuleFor(x => x.Email).ApplyEmailRules();
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("El correo electronico es requerido")
            .EmailAddress().WithMessage("El formato del correo electronico es invalido");
    }

    /// <summary>
    /// Aplica validacion completa de contrasena:
    /// NotEmpty + minimo 8 chars + mayuscula + minuscula + numero + especial.
    /// Usar con: RuleFor(x => x.Password).ApplyPasswordRules();
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("La contrasena es requerida")
            .MinimumLength(8).WithMessage("La contrasena debe tener al menos 8 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage("La contrasena debe contener mayuscula, minuscula, numero y caracter especial");
    }

    /// <summary>
    /// Validacion basica de contrasena (solo NotEmpty + minimo 8 chars).
    /// Para login donde no se valida complejidad.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyBasicPasswordRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("La contrasena es requerida")
            .MinimumLength(8).WithMessage("La contrasena debe tener al menos 8 caracteres");
    }
}
