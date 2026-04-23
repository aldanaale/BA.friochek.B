using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Common.Validators;
using FluentValidation;

namespace BA.Backend.Application.Auth.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).ApplyEmailRules();
    }
}
