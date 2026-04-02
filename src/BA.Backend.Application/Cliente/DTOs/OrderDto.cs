using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Cliente.DTOs;

public record ClientOrderDto(
    Guid Id,
    string Status,
    int Total,
    DateTime CreatedAt,
    DateTime? DispatchDate,
    List<ClientOrderItemDto> Items
);

public record ClientOrderSummaryDto(
    Guid OrderId,
    string Status,
    int Total,
    DateTime CreatedAt,
    DateTime? DispatchDate
);

public record ClientOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    int UnitPrice,
    int Subtotal
);
