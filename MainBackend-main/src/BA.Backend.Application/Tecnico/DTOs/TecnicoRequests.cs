namespace BA.Backend.Application.Tecnico.DTOs;

public record ReEnrollNfcRequestDto(
    Guid CoolerId,
    string OldNfcUid,
    string NewNfcUid
);

public record CertificarReparacionRequestDto(
    Guid TicketId,
    string Comentarios,
    string NfcAccessToken
);
