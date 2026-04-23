using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Transportista.DTOs;


namespace BA.Backend.Application.Transportista.Interfaces;

public interface ITransportistaRepository
{
    Task<List<TransportistaRouteStopDto>> GetDailyRouteAsync(Guid transportistaId, DateTime routeDate, Guid tenantId);
    Task<List<TransportistaRouteDto>> GetPendingRouteStopsAsync(Guid transportistaId, Guid tenantId);
    Task<CoolerDetailDto> GetCoolerByNfcTagAsync(string nfcTagId);
    Task ValidateNfcTagAsync(string nfcTagId, Guid expectedCoolerId);
    Task<DeliveryResultDto> RegisterDeliveryAsync(RegisterDeliveryCommand command);
    Task<WastePickupResultDto> RegisterWastePickupAsync(RegisterWastePickupCommand command);
    Task<SupportTicketResultDto> CreateSupportTicketAsync(CreateSupportTicketCommand command);
    Task<List<MovementSummaryDto>> GetCoolerHistoryAsync(Guid coolerId, DateTime? from, DateTime? to, MovementType? type, int page, int size);
    Task<List<SupportTicketResultDto>> GetPendingTicketsByRouteAsync(Guid transportistaId, DateTime routeDate);
}

public interface IPhotoStorageService
{
    Task<string> UploadPhotoAsync(Stream photoStream, string fileName);
    Task DeletePhotoAsync(string photoUrl);
}
