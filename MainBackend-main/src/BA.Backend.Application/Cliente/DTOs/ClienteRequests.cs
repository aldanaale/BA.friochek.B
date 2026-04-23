namespace BA.Backend.Application.Cliente.DTOs;

// --- OUTPUT DTOS (Internal/Response) ---

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Category, // Antes 'Type'
    decimal Price,
    bool IsActive,   // Antes 'IsAvailable'
    string? ExternalSku,
    int? StockExterno = null // Antes 'ExternalStock'
);

public record OrderDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    Guid? CoolerId,
    string Status,
    decimal Total,
    DateTime CreatedAt
);
