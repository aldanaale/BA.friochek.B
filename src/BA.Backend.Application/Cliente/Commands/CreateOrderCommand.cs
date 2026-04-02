using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record CreateOrderCommand(string NfcAccessToken, Guid UserId, Guid TenantId) : IRequest<Guid>;

public class CreateOrderCommandHandler(INfcTagRepository nfcTagRepository, IOrderRepository orderRepository, IJwtTokenService jwtTokenService) 
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var nfcValidation = jwtTokenService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null)
            throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        if (nfcValidation.TenantId != request.TenantId)
            throw new UnauthorizedAccessException("TENANT_MISMATCH");

        var tag = await nfcTagRepository.GetByTagIdAsync(nfcValidation.TagId, request.TenantId, ct);
        if (tag == null)
            throw new KeyNotFoundException("NFC_NOT_FOUND");

        if (!tag.IsEnrolled)
            throw new InvalidOperationException("NFC_NOT_ACTIVE");

        var order = Order.Create(
            request.TenantId, 
            request.UserId, 
            nfcValidation.CoolerId, 
            tag.TagId
        );

        await orderRepository.AddAsync(order, ct);
        await orderRepository.SaveChangesAsync(ct);

        return order.Id;
    }
}
