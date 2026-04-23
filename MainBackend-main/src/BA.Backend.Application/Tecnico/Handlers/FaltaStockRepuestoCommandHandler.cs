using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Interfaces;

namespace BA.Backend.Application.Tecnico.Handlers;

public sealed class FaltaStockRepuestoCommandHandler : IRequestHandler<FaltaStockRepuestoCommand, RegistroActividadDto>
{
    private readonly ITecnicoRepository _repository;

    public FaltaStockRepuestoCommandHandler(ITecnicoRepository repository)
        => _repository = repository;

    public Task<RegistroActividadDto> Handle(FaltaStockRepuestoCommand request, CancellationToken cancellationToken)
        => _repository.ReportarFaltaStockAsync(request.TecnicoId, request.RepuestoId, request.Motivo);
}
