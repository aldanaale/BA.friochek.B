using MediatR;
using BA.Backend.Application.Transportista.DTOs;

namespace BA.Backend.Application.Transportista.Commands;

public record RegisterDeliveryCommand(
    Guid RouteStopId,
    List<CoolerDeliveryItem> Deliveries,
    string ConfirmationNfcTagId,
    DateTime DeliveredAt,
    Guid TransportistaId) : IRequest<DeliveryResultDto>;

public record CoolerDeliveryItem(
    Guid CoolerId,
    string NfcTagId,
    List<ProductDelivery> Products);

public record ProductDelivery(
    Guid ProductId,
    int QuantitySuggested,
    int QuantityDelivered);

public record RegisterWastePickupCommand(
    Guid CoolerId,
    string NfcTagId,
    List<WasteItem> Items,
    string PhotoEvidenceUrl,
    string ConfirmationNfcTagId,
    DateTime PickedUpAt,
    Guid TransportistaId) : IRequest<WastePickupResultDto>;

public record WasteItem(
    Guid ProductId,
    int QuantityRemoved);

public record CreateSupportTicketCommand(
    Guid CoolerId,
    string NfcTagId,
    TicketCategory Category,
    string Description,
    string? PhotoEvidenceUrl,
    Guid TransportistaId) : IRequest<SupportTicketResultDto>;

public record ValidateNfcTagCommand(
    Guid CoolerId,
    string ScannedNfcTagId,
    DateTime ValidatedAt,
    Guid TransportistaId) : IRequest<NfcValidationResultDto>;
