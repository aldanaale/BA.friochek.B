using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Common.Validators;
using FluentValidation;

namespace BA.Backend.Application.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).ApplyEmailRules();
        RuleFor(x => x.Password).ApplyBasicPasswordRules();

        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty()
            .WithMessage("El dispositivo no se pudo identificar");
    }
}
