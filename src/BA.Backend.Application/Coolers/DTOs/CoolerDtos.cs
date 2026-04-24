using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Coolers.DTOs;

public record CoolerDto(
    Guid Id,
    Guid TenantId,
    Guid StoreId,
    string Name,
    string SerialNumber,
    string Model,
    int Capacity,
    string Status,
    DateTime? LastMaintenanceAt,
    DateTime CreatedAt,
    NfcTagDto? NfcTag
);

public record NfcTagDto(
    string TagId,
    string SecurityHash,
    bool IsEnrolled,
    string Status,
    DateTime? EnrolledAt
);

public record CreateCoolerDto
{
    public Guid StoreId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = null!;
    public string Model { get; init; } = null!;
    public int Capacity { get; init; }
    public string Status { get; init; } = "SinAsignar";
}

public record UpdateCoolerDto
{
    public string? Name { get; init; }
    public string? SerialNumber { get; init; }
    public string? Model { get; init; }
    public int? Capacity { get; init; }
    public string? Status { get; init; }
}

public record CoolerListDto(
    Guid Id,
    string Name,
    string SerialNumber,
    string Model,
    int Capacity,
    string Status,
    string StoreName
);