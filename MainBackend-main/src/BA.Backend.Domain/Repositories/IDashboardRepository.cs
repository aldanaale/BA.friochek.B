namespace BA.Backend.Domain.Repositories;

public interface IDashboardRepository
{
    Task<AdminDashboardStats> GetAdminDashboardStatsAsync(Guid tenantId, CancellationToken ct = default);
    Task<SupervisorDashboardStats> GetSupervisorDashboardStatsAsync(Guid tenantId, string? zone = null, CancellationToken ct = default);
    Task<EjecutivoDashboardStats> GetEjecutivoDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task<ClientDashboardStats> GetClientDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task<TechnicianDashboardStats> GetTechnicianDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task<DeliveryDashboardStats> GetDeliveryDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task<RetailerDashboardStats> GetRetailerDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task<List<TechnicianWorkloadDto>> GetTechnicianWorkloadsAsync(Guid tenantId, CancellationToken ct = default);
}

public record TechnicianDashboardStats(
    string Id,
    string Name,
    string LastName,
    string Email,
    string TenantId,
    List<TechnicianTaskRecord> Tasks
);

public record TechnicianTaskRecord(
    Guid Id,
    string StoreName,
    string Address,
    string Commune,
    string AssistanceType,
    string Status,
    double Lat,
    double Lng
);

public record DeliveryDashboardStats(
    string Id,
    string Name,
    string LastName,
    string Email,
    string TenantId
);

public record RetailerDashboardStats(
    string Id,
    string Name,
    string LastName,
    string Email,
    string TenantId,
    string StoreName,
    string StoreAddress,
    Guid? StoreId,
    List<RetailerCoolerRecord> Coolers,
    List<RetailerTechRecord> TechRequests
);

public record RetailerCoolerRecord(
    Guid Id,
    string Model,
    string Status,
    DateTime? LastMaintenanceAt,
    int Capacity,
    string Name
);

public record RetailerTechRecord(
    Guid Id,
    string FaultType,
    DateTime ScheduledDate,
    string Status
);

public record ClientDashboardStats(
    string FullName,
    string Email,
    string TiendaNombre,
    string TiendaDireccion,
    List<CoolerSummaryRecord> Coolers,
    List<TechRequestRecord> TechRequests
);

public record CoolerSummaryRecord(
    Guid Id,
    string Model,
    string Status,
    DateTime? LastRevisionAt
);

public record TechRequestRecord(
    Guid Id,
    string FaultType,
    string Status,
    DateTime ScheduledDate
);

public record AdminDashboardStats(
    int ActiveCoolers,
    int MermasToday,
    int PendingTickets,
    int TotalStores
);

public record SupervisorDashboardStats(
    int ActiveTechnicians,
    int PendingTickets,
    int RepairsToday,
    List<RecentAlertRecord> RecentAlerts
);

public record RecentAlertRecord(
    Guid Id,
    string Type,
    string Message,
    DateTime CreatedAt,
    string Severity
);

public record EjecutivoDashboardStats(
    int ActiveClients
);

public record TechnicianWorkloadDto(
    Guid Id,
    string FullName,
    string Email,
    int ActiveTickets,
    bool IsActive
);
