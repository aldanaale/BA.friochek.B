namespace BA.Backend.Application.Nfc.DTOs;

public record ValidateNfcRequestDto(string NfcUid);

public record EnrollNfcRequestDto(string NfcUid, Guid CoolerId);
