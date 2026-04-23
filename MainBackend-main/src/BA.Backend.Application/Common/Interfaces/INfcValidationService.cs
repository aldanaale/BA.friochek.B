namespace BA.Backend.Application.Common.Interfaces;

public interface INfcValidationService
{
    Task ValidateTagAsync(string scannedTagId, Guid coolerId);
    Task<bool> IsTagRegisteredAsync(string nfcTagId);
}
