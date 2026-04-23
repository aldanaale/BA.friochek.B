using MediatR;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Exceptions;
using BA.Backend.Domain.Repositories;
using BA.Backend.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Transportista.Handlers;

public class MermaCommandHandler : IRequestHandler<MermaCommand, Guid>
{
    private readonly IMermaRepository _repository;
    private readonly IFileStorageService _storage;
    private readonly IJwtTokenService _jwtService;
    private readonly IGeoLocationService _geoService;
    private readonly ICoolerRepository _coolerRepository;
    private readonly IStoreRepository _storeRepository;

    public MermaCommandHandler(
        IMermaRepository repository, 
        IFileStorageService storage, 
        IJwtTokenService jwtService,
        IGeoLocationService geoService,
        ICoolerRepository coolerRepository,
        IStoreRepository storeRepository)
    {
        _repository = repository;
        _storage = storage;
        _jwtService = jwtService;
        _geoService = geoService;
        _coolerRepository = coolerRepository;
        _storeRepository = storeRepository;
    }

    public async Task<Guid> Handle(MermaCommand request, CancellationToken ct)
    {
        var nfcValidation = _jwtService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null) throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        if (nfcValidation.CoolerId != request.CoolerId)
        {
            throw new DomainException("NFC_MISMATCH", "El tag escaneado no corresponde al cooler indicado en la merma.");
        }

        // Validación de Geofencing (200m)
        var cooler = await _coolerRepository.GetByIdAsync(request.CoolerId, ct);
        if (cooler == null) throw new DomainException("COOLER_NOT_FOUND", "No se encontró el cooler especificado.");

        var store = await _storeRepository.GetByIdAsync(cooler.StoreId, ct);
        if (store != null && store.Latitude.HasValue && store.Longitude.HasValue)
        {
            var isWithinRange = _geoService.IsWithinRange(
                request.Latitude, request.Longitude,
                store.Latitude.Value, store.Longitude.Value,
                200.0);

            if (!isWithinRange)
                throw new DomainException("OUT_OF_RANGE", "El transportista se encuentra fuera del rango permitido (200m) para registrar la merma.");
        }

        string photoUrl;
        using (var stream = request.Photo.OpenReadStream())
        {
            photoUrl = await _storage.UploadPhotoAsync(stream, request.Photo.FileName, request.TenantId);
        }

        var merma = Merma.Create(
            request.TenantId,
            request.TransportistaId,
            request.CoolerId,
            request.ProductId,
            request.ProductName,
            request.Quantity,
            request.Reason,
            photoUrl,
            request.Description,
            nfcValidation.TagId
        );

        await _repository.AddAsync(merma, ct);
        await _repository.SaveChangesAsync(ct);

        return merma.Id;
    }
}

