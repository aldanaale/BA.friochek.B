using MediatR;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Exceptions;
using BA.Backend.Domain.Repositories;

namespace BA.Backend.Application.Transportista.Handlers;

internal sealed class RegisterDeliveryCommandHandler : IRequestHandler<RegisterDeliveryCommand, DeliveryResultDto>
{
    private readonly BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository _repository;

    public RegisterDeliveryCommandHandler(BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeliveryResultDto> Handle(RegisterDeliveryCommand request, CancellationToken cancellationToken)
    {
        if (!request.Deliveries.Any(d => d.NfcTagId == request.ConfirmationNfcTagId))
        {
            throw new ValidationException("El NFC de confirmación no coincide con ningún cooler de la entrega.");
        }

        foreach (var delivery in request.Deliveries)
        {
            if (delivery.Products.Any(p => p.QuantityDelivered < 0))
            {
                throw new ValidationException("Las cantidades entregadas no pueden ser negativas.");
            }
        }

        return await _repository.RegisterDeliveryAsync(request);
    }
}

internal sealed class RegisterWastePickupCommandHandler : IRequestHandler<RegisterWastePickupCommand, WastePickupResultDto>
{
    private readonly BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository _repository;
    private readonly INfcValidationService _nfcService;

    public RegisterWastePickupCommandHandler(BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository repository, INfcValidationService nfcService)
    {
        _repository = repository;
        _nfcService = nfcService;
    }

    public async Task<WastePickupResultDto> Handle(RegisterWastePickupCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhotoEvidenceUrl))
        {
            throw new ValidationException("La foto de evidencia es obligatoria para el retiro de mermas.");
        }

        await _nfcService.ValidateTagAsync(request.NfcTagId, request.CoolerId);

        return await _repository.RegisterWastePickupAsync(request);
    }
}

internal sealed class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, SupportTicketResultDto>
{
    private readonly BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository _repository;
    private readonly INfcValidationService _nfcService;

    public CreateSupportTicketCommandHandler(BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository repository, INfcValidationService nfcService)
    {
        _repository = repository;
        _nfcService = nfcService;
    }

    public async Task<SupportTicketResultDto> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 20)
        {
            throw new ValidationException("La descripción del ticket debe tener al menos 20 caracteres.");
        }

        await _nfcService.ValidateTagAsync(request.NfcTagId, request.CoolerId);

        return await _repository.CreateSupportTicketAsync(request);
    }
}

internal sealed class ValidateNfcTagCommandHandler : IRequestHandler<ValidateNfcTagCommand, NfcValidationResultDto>
{
    private readonly INfcValidationService _nfcService;
    private readonly INfcTagRepository _nfcTagRepository;
    private readonly ICoolerRepository _coolerRepository;

    public ValidateNfcTagCommandHandler(
        INfcValidationService nfcService,
        INfcTagRepository nfcTagRepository,
        ICoolerRepository coolerRepository)
    {
        _nfcService = nfcService;
        _nfcTagRepository = nfcTagRepository;
        _coolerRepository = coolerRepository;
    }

    public async Task<NfcValidationResultDto> Handle(ValidateNfcTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _nfcTagRepository.GetByTagIdAsync(request.ScannedNfcTagId, cancellationToken);
        if (tag == null || !tag.IsEnrolled)
            throw new KeyNotFoundException("NFC_NOT_FOUND");

        var cooler = await _coolerRepository.GetByIdAsync(tag.CoolerId, cancellationToken);
        if (cooler == null)
            throw new KeyNotFoundException("COOLER_NOT_FOUND");

        return new NfcValidationResultDto
        {
            IsValid = true,
            CoolerId = cooler.Id,
            ErrorMessage = null
        };
    }
}
