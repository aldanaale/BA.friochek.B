using System;

namespace BA.Backend.Application.Transportista.DTOs;

public record TransportistaDto(
    Guid UserId,
    Guid TenantId,
    string Email,
    string FullName,
    bool IsAvailable,
    string? VehiclePlate,
    DateTime CreatedAt
);

public record CreateTransportistaDto(Guid UserId, string? VehiclePlate);
public record UpdateTransportistaDto(bool IsAvailable, string? VehiclePlate);
