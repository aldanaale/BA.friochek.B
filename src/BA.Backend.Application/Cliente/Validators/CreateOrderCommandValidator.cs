using BA.Backend.Application.Cliente.Commands;
using FluentValidation;

namespace BA.Backend.Application.Cliente.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.NfcAccessToken)
            .NotEmpty().WithMessage("El token NFC es requerido.")
            .MaximumLength(200).WithMessage("El token NFC no puede exceder 200 caracteres.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("El ID de tenant es requerido.");
    }
}
