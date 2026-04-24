using MediatR;
using System;

namespace BA.Backend.Application.PlatformAdmin.Commands;

public record CreateTenantCommand(
    string Name,
    string Slug,
    string? ExternalOrderUrl,
    int IntegrationType
) : IRequest<Guid>;
