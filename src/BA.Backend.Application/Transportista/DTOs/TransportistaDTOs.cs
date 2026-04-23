namespace BA.Backend.Application.Transportista.DTOs;

public record RouteStopDto(
    Guid LocalId,
    string LocalName,
    string Address,
    List<MachineSummaryDto> Machines,
    bool HasPendingDelivery,
    bool HasActiveAlert);

public record MachineSummaryDto(
    Guid MachineId,
    string NfcTagId,
    string Model,
    MachineStatus Status,
    DateTime? LastMaintenanceDate);

public record MachineDetailDto(
    Guid MachineId,
    string NfcTagId,
    string Model,
    string LocalName,
    MachineStatus Status,
    DateTime? LastMaintenanceDate,
    List<MovementSummaryDto> RecentMovements);

public class DeliveryResultDto
{
    public Guid DeliveryId { get; set; }
    public int MachinesIncluded { get; set; }
    public int TotalProductsDelivered { get; set; }
    public DateTime DeliveredAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public record WastePickupResultDto(
    Guid PickupId,
    Guid MachineId,
    int TotalItemsRemoved,
    string PhotoEvidenceUrl,
    DateTime PickedUpAt);

public record SupportTicketResultDto(
    Guid TicketId,
    string TicketNumber,
    Guid MachineId,
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
    public Guid? MachineId { get; set; }
    public string? ErrorMessage { get; set; }
}

