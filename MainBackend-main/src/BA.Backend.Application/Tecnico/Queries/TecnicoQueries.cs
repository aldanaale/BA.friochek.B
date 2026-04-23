
using MediatR;
using BA.Backend.Application.Tecnico.DTOs;

namespace BA.Backend.Application.Tecnico.Queries;

public record GetTicketsAsignadosQuery(Guid TecnicoId) : IRequest<List<TicketAsignadoDto>>;
public record GetHistorialTecnicoByNfcQuery(Guid TecnicoId, string NfcCode) : IRequest<List<HistorialTecnicoDto>>;
public record GetCierreReparacionQuery(Guid TecnicoId, Guid TicketId) : IRequest<CierreReparacionDto>;
