using System;

namespace BA.Backend.Application.Cliente.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Type,
    int Price,
    int Stock
);
