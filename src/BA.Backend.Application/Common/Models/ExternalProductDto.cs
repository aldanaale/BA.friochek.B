namespace BA.Backend.Application.Common.Models;

public record ExternalProductDto(
    string ExternalSku,
    string Name,
    int Price,
    string Type, // Venta, Servicio, Insumo
    bool IsAvailable,
    string? Description = null,
    string? Category = null
);
