using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Interfaces;

namespace BA.Backend.Application.Tecnico.Handlers;

public sealed class SubirEvidenciaFotograficaCommandHandler : IRequestHandler<SubirEvidenciaFotograficaCommand, RegistroActividadDto>
{
    private readonly ITecnicoRepository _repository;

    public SubirEvidenciaFotograficaCommandHandler(ITecnicoRepository repository)
        => _repository = repository;

    public Task<RegistroActividadDto> Handle(SubirEvidenciaFotograficaCommand request, CancellationToken cancellationToken)
        => _repository.SubirEvidenciaAsync(request.TecnicoId, request.TicketId, request.Archivo);
}
