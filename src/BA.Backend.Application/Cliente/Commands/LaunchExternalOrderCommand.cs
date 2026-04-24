using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Commands;

public record LaunchExternalOrderCommand(Guid ProductId, Guid UserId, Guid TenantId) : IRequest<ExternalOrderLaunchResult>;

public record ExternalOrderLaunchResult(string RedirectUrl, string OrderReferenceId);

public class LaunchExternalOrderCommandHandler : IRequestHandler<LaunchExternalOrderCommand, ExternalOrderLaunchResult>
{
    private readonly IOrderRepository _orderRepository;

    public LaunchExternalOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ExternalOrderLaunchResult> Handle(LaunchExternalOrderCommand request, CancellationToken ct)
    {
        var redirectUrl = $"https://tienda.friocheck.com/productos/{request.ProductId}";
        var referenceId = await _orderRepository.CreateExternalOrderReferenceAsync(
            request.UserId, request.TenantId, request.ProductId, redirectUrl, ct);

        return new ExternalOrderLaunchResult(redirectUrl, referenceId);
    }
}
