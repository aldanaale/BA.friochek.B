using BA.Backend.Domain.DTOs; 
 
namespace BA.Backend.Domain.Repositories; 
 
public interface ITecnicoRepository_Obsolete 
{ 
    Task<List<TicketAsignadoDto>> GetTicketsByTecnicoIdAsync(Guid tecnicoId); 
    Task<List<HistorialTecnicoDto>> GetHistorialByNfcAsync(Guid tecnicoId, string nfcCode); 
    Task<CierreReparacionDto> GetCierreReparacionAsync(Guid tecnicoId, Guid ticketId); 
    Task<RegistroActividadDto> RegistrarFallaAsync(Guid tecnicoId, Guid maquinaId, string descripcion); 
    Task<RegistroActividadDto> CambiarRepuestoAsync(Guid tecnicoId, Guid maquinaId, Guid repuestoId); 
    Task<RegistroActividadDto> ReportarFaltaStockAsync(Guid tecnicoId, Guid repuestoId, string motivo); 
    Task<RegistroActividadDto> SubirEvidenciaAsync(Guid tecnicoId, Guid ticketId, object archivo); 
    Task<bool> ValidarNfcAsync(Guid tecnicoId, string nfcCode); 
    Task<RegistroActividadDto> CertificarReparacionAsync(Guid tecnicoId, Guid ticketId, string comentarios); 
} 
