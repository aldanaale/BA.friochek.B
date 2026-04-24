using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Interfaces;

namespace BA.Backend.Application.Tecnico.Handlers;

public sealed class CambiarRepuestoCommandHandler : IRequestHandler<CambiarRepuestoCommand, RegistroActividadDto>
{
    private readonly ITecnicoRepository _repository;

    public CambiarRepuestoCommandHandler(ITecnicoRepository repository)
        => _repository = repository;

    public Task<RegistroActividadDto> Handle(CambiarRepuestoCommand request, CancellationToken cancellationToken)
        => _repository.CambiarRepuestoAsync(request.TecnicoId, request.MaquinaId, request.RepuestoId);
}
