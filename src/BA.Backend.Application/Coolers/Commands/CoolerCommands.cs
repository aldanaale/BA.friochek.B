using MediatR;

namespace BA.Backend.Application.Coolers.Commands;

public record CreateCoolerCommand(
    Guid TenantId,
    Guid StoreId,
    string Name,
    string SerialNumber,
    string Model,
    int Capacity,
    string Status
) : IRequest<Guid>;

public record UpdateCoolerCommand(
    Guid Id,
    Guid TenantId,
    string? Name,
    string? SerialNumber,
    string? Model,
    int? Capacity,
    string? Status
) : IRequest<bool>;

public record DeleteCoolerCommand(Guid Id, Guid TenantId) : IRequest<bool>;

public record UpdateCoolerStatusCommand(Guid Id, Guid TenantId, string Status) : IRequest<bool>;