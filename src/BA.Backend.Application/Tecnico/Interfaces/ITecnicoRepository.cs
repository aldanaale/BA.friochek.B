
using BA.Backend.Application.Tecnico.DTOs;
using Microsoft.AspNetCore.Http;

namespace BA.Backend.Application.Tecnico.Interfaces;

public interface ITecnicoRepository
{
    Task<List<TicketAsignadoDto>> GetTicketsByTecnicoIdAsync(Guid tecnicoId);
    Task<List<HistorialTecnicoDto>> GetHistorialByNfcAsync(Guid tecnicoId, string nfcCode);
    Task<CierreReparacionDto> GetCierreReparacionAsync(Guid tecnicoId, Guid ticketId);
    Task<RegistroActividadDto> RegistrarFallaAsync(Guid tecnicoId, Guid maquinaId, string descripcion);
    Task<RegistroActividadDto> CambiarRepuestoAsync(Guid tecnicoId, Guid maquinaId, Guid repuestoId);
    Task<RegistroActividadDto> ReportarFaltaStockAsync(Guid tecnicoId, Guid repuestoId, string motivo);
    Task<RegistroActividadDto> SubirEvidenciaAsync(Guid tecnicoId, Guid ticketId, IFormFile archivo);
    Task<bool> ValidarNfcAsync(Guid tecnicoId, string nfcCode);
    Task<RegistroActividadDto> CertificarReparacionAsync(Guid tecnicoId, Guid ticketId, string comentarios);
} 
