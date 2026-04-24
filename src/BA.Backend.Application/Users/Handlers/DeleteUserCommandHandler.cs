using BA.Backend.Application.Users.Commands;
using BA.Backend.Domain.Repositories;
using BA.Backend.Application.Exceptions;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, request.TenantId, cancellationToken);
        if (user == null)
            throw new UserNotFoundException("Usuario no existe");

        user.IsActive = false;
        user.IsDeleted = true;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Unit.Value;
    }
}
