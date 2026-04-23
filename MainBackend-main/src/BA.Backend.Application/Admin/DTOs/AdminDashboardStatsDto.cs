namespace BA.Backend.Application.Admin.DTOs;

/// <summary>
/// Estadísticas globales para el dashboard de administración.
/// </summary>
public record AdminDashboardStatsDto(
    int ActiveCoolers,
    int MermasToday,
    int PendingTickets,
    int TotalStores
);

/// <summary>
/// Resumen de actividad global (scans, tickets, envíos).
/// </summary>
public record AdminGlobalActivityDto(
    int ScansToday,
    int NewTickets,
    int DeliveriesPending
);

/// <summary>
/// Representa un evento de actividad reciente en el sistema.
/// </summary>
public record AdminActivityDto(
    Guid Id,
    string Type,
    string Title,
    string Description,
    DateTime CreatedAt,
    string Icon
);
