using BA.Backend.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace BA.Backend.Infrastructure.Services;

public class DeviceFingerprintService : IDeviceFingerprintService
{
    public string ComputeFingerprint(string userAgent, string acceptLanguage)
    {
        var combined = $"{userAgent}:{acceptLanguage}";
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hashedBytes);
    }
}
