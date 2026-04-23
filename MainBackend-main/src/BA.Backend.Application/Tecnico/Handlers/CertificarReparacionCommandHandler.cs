using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Exceptions;
using System.Text.Json;

namespace BA.Backend.Application.Tecnico.Handlers;

public sealed class CertificarReparacionCommandHandler : IRequestHandler<CertificarReparacionCommand, bool>
{
    private readonly ITechSupportRepository _techRepo;
    private readonly IFileStorageService _storage;
    private readonly IJwtTokenService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public CertificarReparacionCommandHandler(
        ITechSupportRepository techRepo,
        IFileStorageService storage,
        IJwtTokenService jwtService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime)
    {
        _techRepo = techRepo;
        _storage = storage;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<bool> Handle(CertificarReparacionCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar Token Fisico NFC
        var nfcValidation = _jwtService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null)
            throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        // 2. Obtener Ticket Original
        var ticket = await _techRepo.GetByIdAsync(request.TicketId, request.TenantId, cancellationToken);
        if (ticket == null)
            throw new KeyNotFoundException("SUPPORT_TICKET_NOT_FOUND");

        // 3. Validar consistencia NFC <-> Ticket
        if (ticket.CoolerId != nfcValidation.CoolerId)
            throw new DomainException("NFC_MISMATCH", "El tag escaneado no corresponde al cooler del ticket.");

        // 4. Subir Foto de prueba
        string photoUrl;
        using (var stream = request.Photo.OpenReadStream())
        {
            photoUrl = await _storage.UploadPhotoAsync(stream, request.Photo.FileName, request.TenantId);
        }

        // 5. Cerrar Ticket
        ticket.Status = "Resuelto";

        var existingPhotos = string.IsNullOrEmpty(ticket.PhotoUrls)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(ticket.PhotoUrls) ?? new List<string>();

        existingPhotos.Add(photoUrl);
        ticket.PhotoUrls = JsonSerializer.Serialize(existingPhotos);
        ticket.Description += $"\n[Cierre por Tecnico - {_dateTime.UtcNow:yyyy-MM-dd}]: {request.Comentarios}";

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
