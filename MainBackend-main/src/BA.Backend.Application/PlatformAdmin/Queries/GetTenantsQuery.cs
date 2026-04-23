using MediatR;
using System.Collections.Generic;

namespace BA.Backend.Application.PlatformAdmin.Queries;

public record GetTenantsQuery : IRequest<List<TenantDto>>;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? ExternalOrderUrl,
    int IntegrationType,
    bool IsActive,
    DateTime CreatedAt
);
