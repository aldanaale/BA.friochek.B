
using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Tecnico.DTOs;
using BA.Backend.Application.Tecnico.Interfaces;
using BA.Backend.Domain.Repositories;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Exceptions;
using System.Text.Json;

namespace BA.Backend.Application.Tecnico.Handlers;

public class TecnicoCommandHandlers :
    IRequestHandler<ReportarFallaCommand, RegistroActividadDto>,
    IRequestHandler<CambiarRepuestoCommand, RegistroActividadDto>,
    IRequestHandler<FaltaStockRepuestoCommand, RegistroActividadDto>,
    IRequestHandler<SubirEvidenciaFotograficaCommand, RegistroActividadDto>,
    IRequestHandler<ValidarNfcCommand, bool>,
    IRequestHandler<CertificarReparacionCommand, bool>,
    IRequestHandler<ReEnrollNfcCommand, bool>
{
    private readonly ITecnicoRepository _repository;
    private readonly ITechSupportRepository _techRepo;
    private readonly INfcTagRepository _nfcRepo;
    private readonly IFileStorageService _storage;
    private readonly IJwtTokenService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public TecnicoCommandHandlers(
        ITecnicoRepository repository,
        ITechSupportRepository techRepo,
        INfcTagRepository nfcRepo,
        IFileStorageService storage,
        IJwtTokenService jwtService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _techRepo = techRepo;
        _nfcRepo = nfcRepo;
        _storage = storage;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegistroActividadDto> Handle(ReportarFallaCommand request, CancellationToken cancellationToken)
        => await _repository.RegistrarFallaAsync(request.TecnicoId, request.MaquinaId, request.Descripcion);

    public async Task<RegistroActividadDto> Handle(CambiarRepuestoCommand request, CancellationToken cancellationToken)
        => await _repository.CambiarRepuestoAsync(request.TecnicoId, request.MaquinaId, request.RepuestoId);

    public async Task<RegistroActividadDto> Handle(FaltaStockRepuestoCommand request, CancellationToken cancellationToken)
        => await _repository.ReportarFaltaStockAsync(request.TecnicoId, request.RepuestoId, request.Motivo);

    public async Task<RegistroActividadDto> Handle(SubirEvidenciaFotograficaCommand request, CancellationToken cancellationToken)
        => await _repository.SubirEvidenciaAsync(request.TecnicoId, request.TicketId, request.Archivo);

    public async Task<bool> Handle(ValidarNfcCommand request, CancellationToken cancellationToken)
        => await _repository.ValidarNfcAsync(request.TecnicoId, request.NfcCode);

    public async Task<bool> Handle(CertificarReparacionCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar Token Físico NFC
        var nfcValidation = _jwtService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null) throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        // 2. Obtener Ticket Original
        var ticket = await _techRepo.GetByIdAsync(request.TicketId, request.TenantId, cancellationToken);
        if (ticket == null) throw new KeyNotFoundException("SUPPORT_TICKET_NOT_FOUND");

        // 3. Validar consistencia NFC <-> Ticket
        if (ticket.CoolerId != nfcValidation.CoolerId)
        {
            throw new DomainException("NFC_MISMATCH", "El tag escaneado no corresponde al cooler del ticket.");
        }

        // 4. Subir la Foto de prueba (Checklist)
        string photoUrl;
        using (var stream = request.Photo.OpenReadStream())
        {
            photoUrl = await _storage.UploadPhotoAsync(stream, request.Photo.FileName, request.TenantId);
        }

        // 5. Cerrar Ticket
        ticket.Status = "Resuelto";
        
        // Agregar foto a la lista de URLs 
        var existingPhotos = string.IsNullOrEmpty(ticket.PhotoUrls) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(ticket.PhotoUrls) ?? new List<string>();
        existingPhotos.Add(photoUrl);
        ticket.PhotoUrls = JsonSerializer.Serialize(existingPhotos);
        ticket.Description += $"\n[Cierre por Técnico - {DateTime.UtcNow:yyyy-MM-dd}]: {request.Comentarios}";

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task<bool> Handle(ReEnrollNfcCommand request, CancellationToken cancellationToken)
    {
        var oldTag = await _nfcRepo.GetByTagIdAsync(request.OldNfcUid, request.TenantId, cancellationToken);
        if (oldTag != null)
        {
            oldTag.IsEnrolled = false;
            await _nfcRepo.UpdateAsync(oldTag, cancellationToken);
        }

        var newTag = new BA.Backend.Domain.Entities.NfcTag
        {
            TagId = request.NewNfcUid,
            CoolerId = request.CoolerId,
            IsEnrolled = true,
            CreatedAt = DateTime.UtcNow,
            EnrolledAt = DateTime.UtcNow,
            SecurityHash = "REENROLLED_" + Guid.NewGuid().ToString("N") // Simple hash mock for now
        };

        await _nfcRepo.AddAsync(newTag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
