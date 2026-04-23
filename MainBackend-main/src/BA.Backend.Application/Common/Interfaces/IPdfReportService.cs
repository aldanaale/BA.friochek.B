namespace BA.Backend.Application.Common.Interfaces;

public interface IPdfReportService
{
    byte[] GenerateDeliveryCertificatePdf(
        Guid certificateId,
        string tenantName,
        string transporterName,
        string storeName,
        DateTime acceptanceTimestamp,
        double latitude,
        double longitude,
        string ipAddress,
        string signatureBase64,
        string serverHash);
}
