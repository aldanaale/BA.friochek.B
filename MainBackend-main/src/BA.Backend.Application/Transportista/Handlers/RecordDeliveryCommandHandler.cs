using MediatR;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Exceptions;
using BA.Backend.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using BA.Backend.Domain.Entities;

namespace BA.Backend.Application.Transportista.Handlers;

public class DeliveryCommandHandler : IRequestHandler<DeliveryCommand, bool>
{
    private readonly IDeliveryRepository _repository;
    private readonly IJwtTokenService _jwtService;
    private readonly IGeoLocationService _geoService;
    private readonly ICurrentTenantService _tenantService;
    private readonly IStoreRepository _storeRepository;
    private readonly ICoolerRepository _coolerRepository;
    private readonly IOperationCertificateRepository _certificateRepository;
    private readonly ICertificateSignerService _signerService;

    public DeliveryCommandHandler(
        IDeliveryRepository repository, 
        IJwtTokenService jwtService,
        IGeoLocationService geoService,
        ICurrentTenantService tenantService,
        IStoreRepository storeRepository,
        ICoolerRepository coolerRepository,
        IOperationCertificateRepository certificateRepository,
        ICertificateSignerService signerService)
    {
        _repository = repository;
        _jwtService = jwtService;
        _geoService = geoService;
        _tenantService = tenantService;
        _storeRepository = storeRepository;
        _coolerRepository = coolerRepository;
        _certificateRepository = certificateRepository;
        _signerService = signerService;
    }

    public async Task<bool> Handle(DeliveryCommand request, CancellationToken ct)
    {
        var stop = await _repository.GetRouteStopByIdAsync(request.RouteStopId, ct);

        if (stop == null)
            throw new DomainException("NOT_FOUND", "No se encontró la parada de ruta especificada.");

        // 1. Validación de Geofencing (200m)
        var store = await _storeRepository.GetByIdAsync(stop.StoreId, ct);
        if (store == null)
            throw new DomainException("STORE_NOT_FOUND", "No se encontró la tienda asociada a la parada.");

        if (store.Latitude.HasValue && store.Longitude.HasValue)
        {
            var isWithinRange = _geoService.IsWithinRange(
                request.Latitude, request.Longitude,
                store.Latitude.Value, store.Longitude.Value,
                200.0);

            if (!isWithinRange)
                throw new DomainException("OUT_OF_RANGE", "El transportista se encuentra fuera del rango permitido (200m) para realizar la entrega.");
        }

        // 2. Validación de NFC
        var nfcValidation = _jwtService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null)
            throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        // Validar que el cooler pertenezca a la tienda
        var cooler = await _coolerRepository.GetByIdAsync(nfcValidation.CoolerId, ct);
        if (cooler == null || cooler.StoreId != stop.StoreId)
            throw new DomainException("NFC_MISMATCH", "El tag escaneado no corresponde a un cooler de esta tienda.");

        // 3. Registro de Entrega
        stop.MarkAsCompleted();

        // 4. Generación de Certificado de Operación
        if (!string.IsNullOrEmpty(request.SignatureBase64))
        {
            var ip = _tenantService.IpAddress ?? "Unknown";
            var fp = _tenantService.DeviceFingerprint ?? "Unknown";

            var certificate = new OperationCertificate
            {
                Id = Guid.NewGuid(),
                TenantId = stop.TenantId,
                RouteStopId = stop.Id,
                UserId = Guid.Parse(_tenantService.UserId!),
                SignatureBase64 = request.SignatureBase64,
                IpAddress = ip,
                DeviceFingerprint = fp,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AcceptanceTimestamp = DateTime.UtcNow,
                ServerHash = _signerService.SignCertificate(stop.Id, request.Latitude, request.Longitude, ip, fp)
            };

            await _certificateRepository.AddAsync(certificate, ct);
        }

        await _repository.SaveChangesAsync(ct);

        return true;
    }
}
