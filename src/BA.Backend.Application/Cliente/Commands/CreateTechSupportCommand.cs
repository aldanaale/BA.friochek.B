using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Commands;

public record CreateTechSupportCommand(
    string NfcAccessToken,
    string FaultType,
    string Description,
    DateTime ScheduledDate,
    IFormFileCollection Photos,
    Guid UserId,
    Guid TenantId
) : IRequest<Guid>;

public class CreateTechSupportCommandHandler : IRequestHandler<CreateTechSupportCommand, Guid>
    {
        private readonly ITechSupportRepository _techRepo;
        private readonly INfcTagRepository _nfcRepo;
        private readonly IFileStorageService _fileService;
        private readonly IJwtTokenService _jwtService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTechSupportCommandHandler(
            ITechSupportRepository techRepo, 
            INfcTagRepository nfcRepo,
            IFileStorageService fileService,
            IJwtTokenService jwtService,
            IUnitOfWork unitOfWork)
        {
            _techRepo = techRepo;
            _nfcRepo = nfcRepo;
            _fileService = fileService;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }
    public async Task<Guid> Handle(CreateTechSupportCommand request, CancellationToken ct)
    {
        var nfcValidation = _jwtService.ValidateNfcToken(request.NfcAccessToken);
        if (nfcValidation == null) throw new UnauthorizedAccessException("NFC_TOKEN_INVALID_OR_EXPIRED");

        var tag = await _nfcRepo.GetByTagIdAsync(nfcValidation.TagId, request.TenantId, ct);
        if (tag == null) throw new KeyNotFoundException("NFC_NOT_FOUND");

        var urls = new List<string>();
        foreach (var photo in request.Photos)
        {
            if (photo.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                using var stream = photo.OpenReadStream();
                var url = await _fileService.UploadPhotoAsync(stream, fileName, request.TenantId);
                urls.Add(url);
            }
        }

        var techRequest = new TechSupportRequest
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            UserId = request.UserId,
            NfcTagId = tag.TagId,
            CoolerId = tag.CoolerId,
            FaultType = request.FaultType,
            Description = request.Description,
            PhotoUrls = JsonSerializer.Serialize(urls),
            ScheduledDate = request.ScheduledDate,
            Status = "Pendiente",
            CreatedAt = DateTime.UtcNow
        };

        await _techRepo.AddAsync(techRequest, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return techRequest.Id;
    }
}
