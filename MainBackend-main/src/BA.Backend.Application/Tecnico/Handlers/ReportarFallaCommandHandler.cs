using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Interfaces;

namespace BA.Backend.Application.Tecnico.Handlers;

public sealed class ReportarFallaCommandHandler : IRequestHandler<ReportarFallaCommand, RegistroActividadDto>
{
    private readonly ITecnicoRepository _repository;

    public ReportarFallaCommandHandler(ITecnicoRepository repository)
        => _repository = repository;

    public Task<RegistroActividadDto> Handle(ReportarFallaCommand request, CancellationToken cancellationToken)
        => _repository.RegistrarFallaAsync(request.TecnicoId, request.MaquinaId, request.Descripcion);
}
