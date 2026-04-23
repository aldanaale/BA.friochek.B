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

    public MermaCommandHandler(IMermaRepository repository, IFileStorageService storage, IJwtTokenService jwtService)
    {
        _repository = repository;
        _storage = storage;
        _jwtService = jwtService;
    }

    public async Task<Guid> Handle(MermaCommand request, CancellationToken ct)
    {
        var nfcValidation = _jwtService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null) throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        if (nfcValidation.CoolerId != request.CoolerId)
        {
            throw new DomainException("NFC_MISMATCH", "El tag escaneado no corresponde al cooler indicado en la merma.");
        }

        string photoUrl;
        using (var stream = request.Photo.OpenReadStream())
        {
            photoUrl = await _storage.UploadPhotoAsync(stream, request.Photo.FileName, request.TenantId);
        }

        var merma = Merma.Create(
            request.TenantId,
            request.TransportistId,
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

