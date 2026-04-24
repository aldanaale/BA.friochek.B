using MediatR;
using BA.Backend.Application.Tecnico.Commands;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Entities;

namespace BA.Backend.Application.Tecnico.Handlers;

public sealed class ReEnrollNfcCommandHandler : IRequestHandler<ReEnrollNfcCommand, bool>
{
    private readonly ICoolerRepository _coolerRepo;
    private readonly INfcTagRepository _nfcRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public ReEnrollNfcCommandHandler(
        ICoolerRepository coolerRepo,
        INfcTagRepository nfcRepo,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime)
    {
        _coolerRepo = coolerRepo;
        _nfcRepo = nfcRepo;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<bool> Handle(ReEnrollNfcCommand request, CancellationToken cancellationToken)
    {
        // Validar que el cooler existe para evitar FK violation
        var cooler = await _coolerRepo.GetByIdWithTenantAsync(request.CoolerId, request.TenantId, cancellationToken);
        if (cooler == null)
            throw new KeyNotFoundException($"COOLER_NOT_FOUND: {request.CoolerId}");

        // Dar de baja el tag anterior si existe
        var oldTag = await _nfcRepo.GetByTagIdAsync(request.OldNfcUid, request.TenantId, cancellationToken);
        if (oldTag != null)
        {
            oldTag.IsEnrolled = false;
            await _nfcRepo.UpdateAsync(oldTag, cancellationToken);
        }

        // Si el nuevo UID ya existe, borrarlo primero
        var existingTag = await _nfcRepo.GetByTagIdAsync(request.NewNfcUid, request.TenantId, cancellationToken);
        if (existingTag != null)
            await _nfcRepo.DeleteAsync(existingTag.TagId, cancellationToken);

        // Crear nuevo tag
        var newTag = new NfcTag
        {
            TagId = request.NewNfcUid,
            CoolerId = request.CoolerId,
            IsEnrolled = true,
            Status = "Activo",
            CreatedAt = _dateTime.UtcNow,
            EnrolledAt = _dateTime.UtcNow,
            SecurityHash = "REENROLLED_" + Guid.NewGuid().ToString("N")
        };

        await _nfcRepo.AddAsync(newTag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
