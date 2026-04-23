using BA.Backend.Application.Users.Commands;
using BA.Backend.Application.Users.DTOs;
using BA.Backend.Application.Exceptions;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException("Tenant no existe");

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, request.TenantId, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException("El email ya está registrado en este tenant");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            Role = request.Role,
            IsActive = true,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

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
