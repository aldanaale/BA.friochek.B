using BA.Backend.Application.Cliente.Commands;
using FluentValidation;

namespace BA.Backend.Application.Cliente.Validators;

public class UpdateOrderItemCommandValidator : AbstractValidator<UpdateOrderItemCommand>
{
    public UpdateOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("El ID del pedido es requerido.");
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("El ID del item es requerido.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.")
            .LessThanOrEqualTo(999).WithMessage("La cantidad no puede exceder 999 unidades.");
    }
}
