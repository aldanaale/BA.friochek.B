
namespace BA.Backend.Application.Tecnico.DTOs;

public record TicketAsignadoDto(Guid Id, string Descripcion, DateTime FechaAsignacion);
public record HistorialTecnicoDto(Guid Id, string Accion, DateTime Fecha);
public record CierreReparacionDto(Guid Id, string Comentarios, DateTime FechaCierre);
public record RegistroActividadDto(Guid Id, string Mensaje, DateTime Fecha);
