namespace BA.Backend.Application.Stores.DTOs;

public record StoreDto(
    Guid Id,
    string Name,
    string Address,
    string? ContactName,
    string? ContactPhone,
    double? Latitude,
    double? Longitude,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateStoreDto(
    string Name,
    string Address,
    string? ContactName,
    string? ContactPhone,
    double? Latitude,
    double? Longitude,
    string? City = null,
    string? District = null
);

public record UpdateStoreDto(
    string Name,
    string Address,
    string? ContactName,
    string? ContactPhone,
    double? Latitude,
    double? Longitude,
    bool IsActive,
    string? City = null,
    string? District = null
);
