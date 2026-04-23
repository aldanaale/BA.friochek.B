using FluentValidation;
using MediatR;

// IMPORTANTE: NO importar FluentValidation.ValidationException aquí.
// Usamos EXCLUSIVAMENTE la excepción custom de la capa Application
// para que el GlobalExceptionHandler la capture y devuelva 400.

namespace BA.Backend.Application;

/// <summary>
/// Behavior de MediatR que ejecuta FluentValidation antes de cada handler.
/// Si hay errores, lanza <see cref="BA.Backend.Application.Exceptions.ValidationException"/>
/// (custom), que el GlobalExceptionHandler mapea correctamente a HTTP 400.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Agrupar errores por propiedad y lanzar la excepción CUSTOM
        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray()
            );

        throw new Exceptions.ValidationException(errors);
    }
}
