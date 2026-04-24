using MediatR;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Exceptions;

namespace BA.Backend.Application.Transportista.Queries;

public record GetDeliveryCertificatePdfQuery(Guid CertificateId) : IRequest<byte[]>;

public class GetDeliveryCertificatePdfQueryHandler : IRequestHandler<GetDeliveryCertificatePdfQuery, byte[]>
{
    private readonly IOperationCertificateRepository _certificateRepository;
    private readonly IPdfReportService _pdfService;

    public GetDeliveryCertificatePdfQueryHandler(
        IOperationCertificateRepository certificateRepository,
        IPdfReportService pdfService)
    {
        _certificateRepository = certificateRepository;
        _pdfService = pdfService;
    }

    public async Task<byte[]> Handle(GetDeliveryCertificatePdfQuery request, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(request.CertificateId, cancellationToken);
        
        if (certificate == null)
            throw new DomainException("CERTIFICATE_NOT_FOUND", "El certificado de operación no existe.");

        var storeName = certificate.RouteStop?.Store?.Name ?? "Tienda Desconocida";

        var pdfBytes = _pdfService.GenerateDeliveryCertificatePdf(
            certificate.Id,
            certificate.Tenant?.Name ?? "Tenant Desconocido",
            certificate.User?.FullName ?? "Transportista Desconocido",
            storeName,
            certificate.AcceptanceTimestamp,
            certificate.Latitude,
            certificate.Longitude,
            certificate.IpAddress,
            certificate.SignatureBase64,
            certificate.ServerHash
        );

        return pdfBytes;
    }
}
