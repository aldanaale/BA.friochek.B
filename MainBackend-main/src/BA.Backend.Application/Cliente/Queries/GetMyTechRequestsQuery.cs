using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Users.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Queries;

public record GetMyTechRequestsQuery(
    Guid UserId,
    Guid TenantId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResultDto<TechSupportDto>>;

public class GetMyTechRequestsQueryHandler : IRequestHandler<GetMyTechRequestsQuery, PagedResultDto<TechSupportDto>>
{
    private readonly ITechSupportRepository _techSupportRepository;

    public GetMyTechRequestsQueryHandler(ITechSupportRepository techSupportRepository)
    {
        _techSupportRepository = techSupportRepository;
    }

    public async Task<PagedResultDto<TechSupportDto>> Handle(GetMyTechRequestsQuery request, CancellationToken ct)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (requests, totalCount) = await _techSupportRepository.GetPagedByUserIdAsync(request.UserId, request.TenantId, pageNumber, pageSize, ct);

        var items = requests.Select(r => new TechSupportDto(
            r.Id,
            r.CoolerId,
            r.FaultType,
            r.Description,
            r.Status,
            r.ScheduledDate,
            r.CreatedAt,
            JsonSerializer.Deserialize<List<string>>(r.PhotoUrls ?? "[]") ?? new List<string>()
        )).ToList();

        return new PagedResultDto<TechSupportDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
