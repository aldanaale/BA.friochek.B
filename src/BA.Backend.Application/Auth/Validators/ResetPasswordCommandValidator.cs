using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Common.Validators;
using FluentValidation;

namespace BA.Backend.Application.Auth.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token es requerido");

        RuleFor(x => x.NewPassword).ApplyPasswordRules();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmacion de contrasena es requerida")
            .Equal(x => x.NewPassword).WithMessage("Las contrasenas no coinciden");
    }
}
