using BA.Backend.Application.Users.Commands;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;

    public LockUserCommandHandler(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Unit> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("Usuario no existe");

        if (user.TenantId != request.TenantId)
            throw new InvalidOperationException("No tienes permiso para bloquear este usuario");

        user.IsLocked = true;

        if (!string.IsNullOrEmpty(user.ActiveSessionId))
        {
            await _sessionRepository.InvalidateSessionAsync(Guid.Parse(user.ActiveSessionId), "Usuario bloqueado");
            user.ActiveSessionId = null;
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Unit.Value;
    }
}
