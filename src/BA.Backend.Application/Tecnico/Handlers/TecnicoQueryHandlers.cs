
using MediatR;
using BA.Backend.Application.Tecnico.Queries;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Interfaces;

namespace BA.Backend.Application.Tecnico.Handlers;

public class TecnicoQueryHandlers :
    IRequestHandler<GetTicketsAsignadosQuery, List<TicketAsignadoDto>>,
    IRequestHandler<GetHistorialTecnicoByNfcQuery, List<HistorialTecnicoDto>>,
    IRequestHandler<GetCierreReparacionQuery, CierreReparacionDto>
{
    private readonly ITecnicoRepository _repository;

    public TecnicoQueryHandlers(ITecnicoRepository repository) => _repository = repository;

    public async Task<List<TicketAsignadoDto>> Handle(GetTicketsAsignadosQuery request, CancellationToken cancellationToken)
        => await _repository.GetTicketsByTecnicoIdAsync(request.TecnicoId);

    public async Task<List<HistorialTecnicoDto>> Handle(GetHistorialTecnicoByNfcQuery request, CancellationToken cancellationToken)
        => await _repository.GetHistorialByNfcAsync(request.TecnicoId, request.NfcCode);

    public async Task<CierreReparacionDto> Handle(GetCierreReparacionQuery request, CancellationToken cancellationToken)
        => await _repository.GetCierreReparacionAsync(request.TecnicoId, request.TicketId);
}
