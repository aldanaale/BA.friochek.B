using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Infrastructure.Settings;
using System.Security.Cryptography;
using System.Text;

namespace BA.Backend.Infrastructure.Services;

public class CertificateSignerService : ICertificateSignerService
{
    private readonly JwtSettings _jwtSettings;

    public CertificateSignerService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public string SignCertificate(Guid entityId, double latitude, double longitude, string ipAddress, string deviceFingerprint)
    {
        var payload = $"{entityId}|{latitude}|{longitude}|{ipAddress}|{deviceFingerprint}";
        return GenerateHash(payload, _jwtSettings.SecretKey);
    }

    public bool VerifyCertificate(Guid entityId, double latitude, double longitude, string ipAddress, string deviceFingerprint, string hash)
    {
        var expectedHash = SignCertificate(entityId, latitude, longitude, ipAddress, deviceFingerprint);
        return expectedHash == hash;
    }

    private string GenerateHash(string payload, string secret)
    {
        var encoding = new UTF8Encoding();
        var keyByte = encoding.GetBytes(secret);
        var messageBytes = encoding.GetBytes(payload);

        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            var hashMessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashMessage);
        }
    }
}
