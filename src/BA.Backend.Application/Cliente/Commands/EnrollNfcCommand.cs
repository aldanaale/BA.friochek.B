using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record EnrollNfcCommand(string NfcUid, Guid CoolerId, Guid TenantId) : IRequest<string>;

public class EnrollNfcCommandHandler : IRequestHandler<EnrollNfcCommand, string>
{
    private readonly INfcTagRepository _nfcTagRepository;
    private readonly ICoolerRepository _coolerRepository;
    private readonly ITechSupportRepository _techSupportRepository;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollNfcCommandHandler(
        INfcTagRepository nfcTagRepository,
        ICoolerRepository coolerRepository,
        ITechSupportRepository techSupportRepository,
        ICurrentTenantService currentTenantService,
        IUnitOfWork unitOfWork)
    {
        _nfcTagRepository = nfcTagRepository;
        _coolerRepository = coolerRepository;
        _techSupportRepository = techSupportRepository;
        _currentTenantService = currentTenantService;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(EnrollNfcCommand request, CancellationToken ct)
    {
        var cooler = await _coolerRepository.GetByIdWithTenantAsync(request.CoolerId, request.TenantId, ct);
        if (cooler == null)
            throw new KeyNotFoundException("COOLER_NOT_FOUND");

        var existingTag = await _nfcTagRepository.GetByTagIdAsync(request.NfcUid, request.TenantId, ct);
        if (existingTag != null)
        {
            if (existingTag.IsEnrolled)
                throw new InvalidOperationException("NFC_ALREADY_ENROLLED");

            existingTag.CoolerId = request.CoolerId;
            existingTag.IsEnrolled = true;
            existingTag.EnrolledAt = DateTime.UtcNow;
            existingTag.SecurityHash = GenerateSecurityHash(request.NfcUid, request.CoolerId);

            await _nfcTagRepository.UpdateAsync(existingTag, ct);

            var installTicket = new BA.Backend.Domain.Entities.TechSupportRequest
            {
                Id = Guid.NewGuid(),
                CoolerId = request.CoolerId,
                FaultType = "Instalación de Tag NFC",
                Description = $"Verificar e instalar físicamente el tag NFC {request.NfcUid} en la máquina.",
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow
            };

            await _techSupportRepository.AddAsync(installTicket, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return existingTag.TagId;
        }

        var newTag = new NfcTag
        {
            TagId = request.NfcUid,
            CoolerId = request.CoolerId,
            SecurityHash = GenerateSecurityHash(request.NfcUid, request.CoolerId),
            IsEnrolled = true,
            CreatedAt = DateTime.UtcNow,
            EnrolledAt = DateTime.UtcNow
        };

        await _nfcTagRepository.AddAsync(newTag, ct);

        // Generar la orden de trabajo de Instalación para ser ejecutada por un Técnico
        var userIdStr = _currentTenantService.UserId;
        var userId = string.IsNullOrEmpty(userIdStr) ? Guid.Empty : Guid.Parse(userIdStr);

        var newInstallTicket = new BA.Backend.Domain.Entities.TechSupportRequest
        {
            Id = Guid.NewGuid(),
            CoolerId = request.CoolerId,
            TenantId = request.TenantId,
            UserId = userId,
            FaultType = "Instalacion de Tag NFC",
            Description = $"Verificar e instalar fisicamente el tag NFC {request.NfcUid} en la maquina.",
            Status = "Pendiente",
            CreatedAt = DateTime.UtcNow,
            ScheduledDate = DateTime.UtcNow.AddDays(1)
        };

        await _techSupportRepository.AddAsync(newInstallTicket, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return newTag.TagId;
    }

    private static string GenerateSecurityHash(string tagId, Guid coolerId)
    {
        using var sha256 = SHA256.Create();
        var raw = Encoding.UTF8.GetBytes($"{tagId}:{coolerId}");
        var hashed = sha256.ComputeHash(raw);
        return Convert.ToHexString(hashed);
    }
}
