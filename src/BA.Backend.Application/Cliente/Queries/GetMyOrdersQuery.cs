using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Users.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Cliente.Queries;

public record GetMyOrdersQuery(Guid UserId, Guid TenantId, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResultDto<ClientOrderSummaryDto>>;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PagedResultDto<ClientOrderSummaryDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetMyOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResultDto<ClientOrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken ct)
    {
        var (orders, totalCount) = await _orderRepository.GetPagedByUserIdAsync(
            request.UserId, request.TenantId, request.PageNumber, request.PageSize, ct);

        var items = orders.Select(o => new ClientOrderSummaryDto
        {
            OrderId = o.Id,
            Status = o.Status,
            Total = o.Items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = o.CreatedAt,
            DispatchDate = o.DispatchDate
        }).ToList();

        return new PagedResultDto<ClientOrderSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
