using BA.Backend.Application.Cliente.Commands;
using FluentValidation;

namespace BA.Backend.Application.Cliente.Validators;

public class AddOrderItemCommandValidator : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("El ID del pedido es requerido.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("El ID del producto es requerido.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.")
            .LessThanOrEqualTo(999).WithMessage("La cantidad no puede exceder 999 unidades.");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("El ID de tenant es requerido.");
    }
}
