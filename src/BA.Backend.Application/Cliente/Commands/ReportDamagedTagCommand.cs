using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record ReportDamagedTagCommand(
    Guid CoolerId,
    string Description,
    Guid UserId,
    Guid TenantId
) : IRequest<Guid>;

public class ReportDamagedTagCommandHandler : IRequestHandler<ReportDamagedTagCommand, Guid>
{
    private readonly INfcTagRepository _nfcRepo;
    private readonly ITechSupportRepository _techRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ReportDamagedTagCommandHandler(
        INfcTagRepository nfcRepo,
        ITechSupportRepository techRepo,
        IUnitOfWork unitOfWork)
    {
        _nfcRepo = nfcRepo;
        _techRepo = techRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ReportDamagedTagCommand request, CancellationToken ct)
    {
        var tag = await _nfcRepo.GetByCoolerIdAsync(request.CoolerId, request.TenantId, ct);
        if (tag == null)
            throw new KeyNotFoundException("NFC_NOT_FOUND");

        tag.IsEnrolled = false;
        await _nfcRepo.UpdateAsync(tag, ct);

        var techRequest = new TechSupportRequest
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            UserId = request.UserId,
            NfcTagId = tag.TagId,
            CoolerId = request.CoolerId,
            FaultType = "TagDañado",
            Description = request.Description,
            PhotoUrls = "[]",
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            Status = "Pendiente",
            CreatedAt = DateTime.UtcNow
        };

        await _techRepo.AddAsync(techRequest, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return techRequest.Id;
    }
}
