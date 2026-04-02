namespace BA.Backend.Application.Auth.DTOs;

public record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAt,
    string UserFullName,
    string Role,
    Guid UserId,
    Guid TenantId,
    bool SessionReplaced,
    string RedirectTo,
     ClienteDataDto? ClienteData = null
);

public record ClienteDataDto(
    StoreDataDto? Store,
    List<TicketDataDto> Tickets,
    List<OrderDataDto> ActiveOrders
);

public record StoreDataDto(
    Guid Id,
    string Name,
    string Address,
    string ContactName,
    string ContactPhone
)
{
    public List<CoolerDataDto> Coolers { get; init; } = new();
}

public record CoolerDataDto(
    Guid Id,
    string SerialNumber,
    string Model,
    int Capacity,
    string Status,
    DateTime? InstalledAt,
    DateTime? LastRevisionAt
);

public record TicketDataDto(
    Guid Id,
    Guid CoolerId,
    string FaultType,
    string Description,
    string Status,
    DateTime ScheduledDate,
    DateTime CreatedAt
);

public record OrderDataDto(
    Guid Id,
    Guid? CoolerId,
    Guid? NfcTagId,
    string Status,
    int Total,
    DateTime? DispatchDate,
    DateTime CreatedAt
);
