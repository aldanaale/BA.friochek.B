namespace BA.Backend.Application.Admin.DTOs;

public record AdminDashboardStatsDto(
    int TotalActiveOrders,
    int TotalCoolers,
    int TotalMermasToday,
    int TotalPendingTechTickets,
    int TotalStores
);
