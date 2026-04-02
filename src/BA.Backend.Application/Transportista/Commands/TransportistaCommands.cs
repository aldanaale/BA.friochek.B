using MediatR;
using BA.Backend.Application.Transportista.DTOs;

namespace BA.Backend.Application.Transportista.Commands;

public record RegisterDeliveryCommand(
    Guid RouteStopId,
    List<DeliveryItem> Deliveries,
    string ConfirmationNfcTagId,
    DateTime DeliveredAt,
    Guid TransportistId) : IRequest<DeliveryResultDto>;

public record DeliveryItem(
    Guid MachineId,
    string NfcTagId,
    List<ProductDelivery> Products);

public record ProductDelivery(
    Guid ProductId,
    int QuantitySuggested,
    int QuantityDelivered);

public record RegisterWastePickupCommand(
    Guid MachineId,
    string NfcTagId,
    List<WasteItem> Items,
    string PhotoEvidenceUrl,
    string ConfirmationNfcTagId,
    DateTime PickedUpAt,
    Guid TransportistId) : IRequest<WastePickupResultDto>;

public record WasteItem(
    Guid ProductId,
    int QuantityRemoved);

public record CreateSupportTicketCommand(
    Guid MachineId,
    string NfcTagId,
    TicketCategory Category,
    string Description,
    string? PhotoEvidenceUrl,
    Guid TransportistId) : IRequest<SupportTicketResultDto>;

public record ValidateNfcTagCommand(
    Guid MachineId,
    string ScannedNfcTagId,
    DateTime ValidatedAt,
    Guid TransportistId) : IRequest<NfcValidationResultDto>;
