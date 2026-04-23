using BA.Backend.Application.Users.Commands;
using BA.Backend.Application.Users.DTOs;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Enums;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("Usuario no existe");

        if (user.TenantId != request.TenantId)
            throw new InvalidOperationException("No tienes permiso para modificar este usuario");

        user.FullName = request.FullName;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            LastLoginAt = user.LastLoginAt
        };
    }
}
