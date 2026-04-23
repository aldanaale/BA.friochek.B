using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Transportista.DTOs;


namespace BA.Backend.Application.Transportista.Interfaces;

public interface ITransportistaRepository
{
    Task<List<RouteStopDto>> GetDailyRouteAsync(Guid transportistId, DateTime routeDate);
    Task<List<TransportistaRouteDto>> GetPendingRouteStopsAsync(Guid transportistId, Guid tenantId);
    Task<MachineDetailDto> GetMachineByNfcTagAsync(string nfcTagId);
    Task ValidateNfcTagAsync(string nfcTagId, Guid expectedMachineId);
    Task<DeliveryResultDto> RegisterDeliveryAsync(RegisterDeliveryCommand command);
    Task<WastePickupResultDto> RegisterWastePickupAsync(RegisterWastePickupCommand command);
    Task<SupportTicketResultDto> CreateSupportTicketAsync(CreateSupportTicketCommand command);
    Task<List<MovementSummaryDto>> GetMachineHistoryAsync(Guid machineId, DateTime? from, DateTime? to, MovementType? type, int page, int size);
    Task<List<SupportTicketResultDto>> GetPendingTicketsByRouteAsync(Guid transportistId, DateTime routeDate);
}

public interface INfcValidationService
{
    Task ValidateTagAsync(string scannedTagId, Guid machineId);
    Task<bool> IsTagRegisteredAsync(string nfcTagId);
}

public interface IPhotoStorageService
{
    Task<string> UploadPhotoAsync(Stream photoStream, string fileName);
    Task DeletePhotoAsync(string photoUrl);
}
