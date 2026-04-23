using BA.Backend.Application.Tecnico.Interfaces;
using BA.Backend.Application.Tecnico.DTOs;
using Microsoft.AspNetCore.Http;

namespace BA.Backend.Infrastructure.Repositories;

public class TecnicoRepository : ITecnicoRepository
{
    public Task<List<TicketAsignadoDto>> GetTicketsByTecnicoIdAsync(Guid tecnicoId) 
        => Task.FromResult(new List<TicketAsignadoDto>());

    public Task<List<HistorialTecnicoDto>> GetHistorialByNfcAsync(Guid tecnicoId, string nfcCode) 
        => Task.FromResult(new List<HistorialTecnicoDto>());

    public Task<CierreReparacionDto> GetCierreReparacionAsync(Guid tecnicoId, Guid ticketId) 
        => Task.FromResult(new CierreReparacionDto(Guid.NewGuid(), "Stub", DateTime.Now));

    public Task<RegistroActividadDto> RegistrarFallaAsync(Guid tecnicoId, Guid maquinaId, string descripcion) 
        => Task.FromResult(new RegistroActividadDto(Guid.NewGuid(), "Falla reportada", DateTime.Now));

    public Task<RegistroActividadDto> CambiarRepuestoAsync(Guid tecnicoId, Guid maquinaId, Guid repuestoId) 
        => Task.FromResult(new RegistroActividadDto(Guid.NewGuid(), "Repuesto cambiado", DateTime.Now));

    public Task<RegistroActividadDto> ReportarFaltaStockAsync(Guid tecnicoId, Guid repuestoId, string motivo) 
        => Task.FromResult(new RegistroActividadDto(Guid.NewGuid(), "Falta de stock reportada", DateTime.Now));

    public Task<RegistroActividadDto> SubirEvidenciaAsync(Guid tecnicoId, Guid ticketId, IFormFile archivo) 
        => Task.FromResult(new RegistroActividadDto(Guid.NewGuid(), "Evidencia subida", DateTime.Now));

    public Task<bool> ValidarNfcAsync(Guid tecnicoId, string nfcCode) 
        => Task.FromResult(true);

    public Task<RegistroActividadDto> CertificarReparacionAsync(Guid tecnicoId, Guid ticketId, string comentarios) 
        => Task.FromResult(new RegistroActividadDto(Guid.NewGuid(), "Reparación certificada", DateTime.Now));
}