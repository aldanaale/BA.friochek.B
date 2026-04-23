using BA.Backend.Application.Users.DTOs;
using BA.Backend.Application.Users.Queries;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResultDto<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResultDto<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (users, totalCount) = await _userRepository.GetPagedAsync(request.TenantId, pageNumber, pageSize, cancellationToken);

        return new PagedResultDto<UserDto>
        {
            Items = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = $"{u.Name} {u.LastName}",
                Role = u.Role,
                IsActive = u.IsActive,
                IsLocked = u.IsLocked,
                LastLoginAt = u.LastLoginAt
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
