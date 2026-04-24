namespace BA.Backend.WebAPI.DTOs.Nfc;

/// <summary>
/// Parámetros para validar un tag NFC.
/// </summary>
/// <param name="NfcUid">Identificador único del chip NFC (ID físico o UID).</param>
public record ValidateNfcRequest(string NfcUid);

/// <summary>
/// Parámetros para enrolar un tag NFC a un cooler.
/// </summary>
/// <param name="NfcUid">Identificador único del chip NFC.</param>
/// <param name="CoolerId">ID del cooler al que se va a asociar.</param>
public record EnrollNfcRequest(string NfcUid, Guid CoolerId);
