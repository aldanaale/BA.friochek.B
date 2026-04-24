using BA.Backend.Application.PlatformAdmin.Commands;
using BA.Backend.Application.Exceptions;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.PlatformAdmin.Handlers;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar que el slug no exista
        var existing = await _tenantRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existing != null)
            throw new BadRequestException($"El slug '{request.Slug}' ya está en uso.");

        // 2. Crear el tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            ExternalOrderUrl = request.ExternalOrderUrl,
            IntegrationType = request.IntegrationType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        
        return tenant.Id;
    }
}
