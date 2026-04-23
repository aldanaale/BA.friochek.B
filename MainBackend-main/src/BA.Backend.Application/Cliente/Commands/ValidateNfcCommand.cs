using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record ValidateNfcCommand(string NfcUid, Guid TenantId) : IRequest<NfcValidationResultDto>;

public class ValidateNfcCommandHandler : IRequestHandler<ValidateNfcCommand, NfcValidationResultDto>
{
    private readonly INfcTagRepository _nfcTagRepository;
    private readonly ICoolerRepository _coolerRepository;
    private readonly IJwtTokenService _jwtService;

    public ValidateNfcCommandHandler(INfcTagRepository nfcTagRepository, ICoolerRepository coolerRepository, IJwtTokenService jwtService)
    {
        _nfcTagRepository = nfcTagRepository;
        _coolerRepository = coolerRepository;
        _jwtService = jwtService;
    }

    public async Task<NfcValidationResultDto> Handle(ValidateNfcCommand request, CancellationToken ct)
    {
        var tag = await _nfcTagRepository.GetByTagIdAsync(request.NfcUid, request.TenantId, ct);
        if (tag == null)
        {
            throw new KeyNotFoundException("NFC_NOT_FOUND");
        }

        if (!tag.IsEnrolled)
        {
            throw new InvalidOperationException("NFC_NOT_ACTIVE");
        }

        var cooler = await _coolerRepository.GetByIdAsync(tag.CoolerId, request.TenantId, ct);
        if (cooler == null)
        {
            throw new KeyNotFoundException("COOLER_NOT_FOUND");
        }

        var tokenResult = _jwtService.GenerateNfcScanToken(tag.TagId, cooler.Id, cooler.StoreId, request.TenantId);

        return new NfcValidationResultDto(
            Guid.NewGuid(),
            cooler.Id,
            cooler.StoreId,
            cooler.Model,
            cooler.Capacity,
            cooler.Capacity,
            cooler.CreatedAt,
            tokenResult.Token
        );
    }
}
