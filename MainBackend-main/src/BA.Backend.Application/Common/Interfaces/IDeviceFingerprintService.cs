namespace BA.Backend.Application.Common.Interfaces;

public interface IDeviceFingerprintService
{
    string ComputeFingerprint(string userAgent, string acceptLanguage);
}
