using Microsoft.AspNetCore.Http;

namespace BA.Backend.WebAPI.DTOs.Cliente;

public record CreateTechSupportRequest(
    string NfcAccessToken,
    string FaultType,
    string Description,
    DateTime ScheduledDate,
    IFormFileCollection? Photos = null
);

public record ReportDamagedTagRequest(Guid CoolerId, string Description);

public record CreateOrderRequest(string NfcAccessToken);
public record AddItemRequest(Guid ProductId, int Quantity);
public record UpdateItemRequest(int Quantity);
public record RetailerPedidoRequest(Guid UserId, List<CoolerPedidoItemRequest> Coolers);
public record CoolerPedidoItemRequest(Guid CoolerId, List<PedidoProductItemRequest> Items);
public record PedidoProductItemRequest(Guid ProductId, int Quantity);
public record LaunchExternalOrderRequest(Guid ProductId);
