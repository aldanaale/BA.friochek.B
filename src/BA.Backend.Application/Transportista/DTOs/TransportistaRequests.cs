using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Transportista.DTOs;

/// <summary>
/// Solicitud para registrar una entrega.
/// </summary>
public record RecordDeliveryRequest(
    Guid OrderId,
    Guid RouteStopId,
    string NfcAccessToken,
    List<DeliveredItemDto>? DeliveredItems
);

public record DeliveredItemDto(
    Guid OrderItemId,
    int Quantity
);

/// <summary>
/// Solicitud para registrar una merma (vía Form-Data).
/// </summary>
public class RecordMermaRequest
{
    public Guid CoolerId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public string Reason { get; set; } = null!;
    public string? Description { get; set; }
    public string NfcAccessToken { get; set; } = null!;
    public IFormFile Photo { get; set; } = null!;
}
