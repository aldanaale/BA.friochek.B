namespace BA.Backend.Application.Transportista.DTOs;

public class TransportistaRouteStopDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<CoolerSummaryDto> Coolers { get; set; } = new();
    public bool HasPendingDelivery { get; set; }
    public bool HasActiveAlert { get; set; }
}

public record CoolerSummaryDto(
    Guid CoolerId,
    string NfcTagId,
    string Model,
    CoolerStatus Status,
    DateTime? LastMaintenanceDate);

public record TransportistaRouteDto(
    Guid RouteStopId,
    string StoreName,
    string StoreAddress,
    string StoreCity,
    string Status
);

public record CoolerDetailDto(
    Guid CoolerId,
    string NfcTagId,
    string Model,
    string StoreName,
    CoolerStatus Status,
    DateTime? LastMaintenanceDate,
    List<MovementSummaryDto> RecentMovements);

public class DeliveryResultDto
{
    public Guid DeliveryId { get; set; }
    public int CoolersIncluded { get; set; }
    public int TotalProductsDelivered { get; set; }
    public DateTime DeliveredAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public record WastePickupResultDto(
    Guid PickupId,
    Guid CoolerId,
    int TotalItemsRemoved,
    string PhotoEvidenceUrl,
    DateTime PickedUpAt);

public record SupportTicketResultDto(
    Guid TicketId,
    string TicketNumber,
    Guid CoolerId,
    TicketCategory Category,
    TicketStatus Status,
    DateTime CreatedAt);

public record MovementSummaryDto(
    Guid MovementId,
    MovementType MovementType,
    DateTime OccurredAt,
    string Description,
    string Status);

public class NfcValidationResultDto
{
    public bool IsValid { get; set; }
    public Guid? CoolerId { get; set; }
    public string? ErrorMessage { get; set; }
}

