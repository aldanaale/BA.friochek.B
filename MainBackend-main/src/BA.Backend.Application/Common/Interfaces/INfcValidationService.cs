namespace BA.Backend.Application.Common.Interfaces;

/// <summary>
/// Servicio compartido de validacion NFC.
/// Centraliza la logica de verificar tag -> enrolled -> cooler
/// que antes estaba duplicada en Cliente, Tecnico y Transportista.
/// </summary>
public interface INfcValidationService
{
    /// <summary>Valida que el tag exista, este enrolled y corresponda al cooler esperado.</summary>
    Task ValidateTagAsync(string scannedTagId, Guid coolerId);

    /// <summary>Verifica si un tag esta registrado en el sistema.</summary>
    Task<bool> IsTagRegisteredAsync(string nfcTagId);
}
