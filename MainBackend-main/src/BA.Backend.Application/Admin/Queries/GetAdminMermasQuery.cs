using BA.Backend.Application.Users.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Admin.Queries;

public record GetAdminMermasQuery(Guid TenantId, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResultDto<AdminMermaDto>>;

public record AdminMermaDto(
    Guid Id,
    Guid CoolerId,
    string ProductName,
    int Quantity,
    string Reason,
    string? Description,
    DateTime CreatedAt,
    string? PhotoUrl
);

public class GetAdminMermasQueryHandler : IRequestHandler<GetAdminMermasQuery, PagedResultDto<AdminMermaDto>>
{
    private readonly IMermaRepository _mermaRepository;

    public GetAdminMermasQueryHandler(IMermaRepository mermaRepository)
    {
        _mermaRepository = mermaRepository;
    }

    public async Task<PagedResultDto<AdminMermaDto>> Handle(GetAdminMermasQuery request, CancellationToken ct)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (mermas, totalCount) = await _mermaRepository.GetPagedAsync(request.TenantId, pageNumber, pageSize, ct);

        return new PagedResultDto<AdminMermaDto>
        {
            Items = mermas.Select(m => new AdminMermaDto(
                m.Id,
                m.CoolerId,
                m.ProductName,
                m.Quantity,
                m.Reason,
                m.Description,
                m.CreatedAt,
                m.PhotoUrl
            )).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
