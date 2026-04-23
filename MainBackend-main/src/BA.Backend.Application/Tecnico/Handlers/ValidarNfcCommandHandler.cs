using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.Interfaces;

namespace BA.Backend.Application.Tecnico.Handlers;

public sealed class ValidarNfcCommandHandler : IRequestHandler<ValidarNfcCommand, bool>
{
    private readonly ITecnicoRepository _repository;

    public ValidarNfcCommandHandler(ITecnicoRepository repository)
        => _repository = repository;

    public Task<bool> Handle(ValidarNfcCommand request, CancellationToken cancellationToken)
        => _repository.ValidarNfcAsync(request.TecnicoId, request.NfcCode, cancellationToken);
}
