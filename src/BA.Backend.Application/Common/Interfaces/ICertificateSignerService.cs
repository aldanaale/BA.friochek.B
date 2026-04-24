namespace BA.Backend.Application.Common.Interfaces;

public interface ICertificateSignerService
{
    string SignCertificate(Guid entityId, double latitude, double longitude, string ipAddress, string deviceFingerprint);
    bool VerifyCertificate(Guid entityId, double latitude, double longitude, string ipAddress, string deviceFingerprint, string hash);
}
