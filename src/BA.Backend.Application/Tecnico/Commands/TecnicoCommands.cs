
using MediatR;
using BA.Backend.Application.Tecnico.DTOs;
using Microsoft.AspNetCore.Http;

namespace BA.Backend.Application.Tecnico.Commands;

public record ReportarFallaCommand(Guid TecnicoId, Guid MaquinaId, string Descripcion) : IRequest<RegistroActividadDto>;
public record CambiarRepuestoCommand(Guid TecnicoId, Guid MaquinaId, Guid RepuestoId) : IRequest<RegistroActividadDto>;
public record FaltaStockRepuestoCommand(Guid TecnicoId, Guid RepuestoId, string Motivo) : IRequest<RegistroActividadDto>;
public record SubirEvidenciaFotograficaCommand(Guid TecnicoId, Guid TicketId, IFormFile Archivo) : IRequest<RegistroActividadDto>;
public record ValidarNfcCommand(Guid TecnicoId, string NfcCode) : IRequest<bool>;
public record CertificarReparacionCommand(
    Guid TenantId,
    Guid TecnicoId, 
    Guid TicketId, 
    string Comentarios, 
    IFormFile Photo,
    string NfcAccessToken
) : IRequest<bool>;

public record ReEnrollNfcCommand(
    Guid TenantId,
    Guid TecnicoId,
    Guid CoolerId,
    string OldNfcUid,
    string NewNfcUid
) : IRequest<bool>;
