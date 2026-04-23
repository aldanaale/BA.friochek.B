using BA.Backend.Application.Stores.DTOs;
using MediatR;

namespace BA.Backend.Application.Stores.Commands;

public record CreateStoreCommand(
    string Name,
    string Address,
    string? ContactName,
    string? ContactPhone,
    double? Latitude,
    double? Longitude,
    Guid TenantId,
    string? City,
    string? District
) : IRequest<StoreDto>;

public record UpdateStoreCommand(
    Guid Id,
    string Name,
    string Address,
    string? ContactName,
    string? ContactPhone,
    double? Latitude,
    double? Longitude,
    bool IsActive,
    Guid TenantId,
    string? City,
    string? District
) : IRequest<StoreDto>;

public record DeleteStoreCommand(
    Guid Id,
    Guid TenantId
) : IRequest<bool>;
