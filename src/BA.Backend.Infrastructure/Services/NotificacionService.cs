using BA.Backend.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Infrastructure.Services;

public class NotificacionService : INotificacionService
{
    private readonly IHubContext<NotificationHub, INotificationHubClient> _hubContext;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        IHubContext<NotificationHub, INotificationHubClient> hubContext,
        ILogger<NotificacionService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task EnviarNotificacionAsync(string mensaje, Guid tecnicoId)
    {
        // Wrapper de compatibilidad con la interfaz anterior
        await NotificarUsuarioAsync(tecnicoId, "Nuevo Mensaje", mensaje);
    }

    public async Task NotificarUsuarioAsync(Guid userId, string titulo, string mensaje)
    {
        _logger.LogInformation("Enviando notificación Push a Usuario {UserId}: {Titulo}", userId, titulo);
        
        // SignalR usa el NameIdentifier (UserId) si está configurado el UserIdProvider, 
        // pero por defecto podemos usar grupos o el User() selector si está autenticado.
        await _hubContext.Clients.User(userId.ToString())
            .ReceiveNotification(titulo, mensaje, "info");
    }

    public async Task NotificarTenantAsync(Guid tenantId, string titulo, string mensaje)
    {
        _logger.LogInformation("Enviando notificación masiva al Tenant {TenantId}: {Titulo}", tenantId, titulo);
        
        await _hubContext.Clients.Group(tenantId.ToString())
            .ReceiveNotification(titulo, mensaje, "broadcast");
    }

    public async Task NotificarRolAsync(Guid tenantId, string rol, string titulo, string mensaje)
    {
        _logger.LogInformation("Enviando notificación al Rol {Rol} en Tenant {TenantId}: {Titulo}", rol, tenantId, titulo);
        
        // Nota: Para notificar por rol de forma eficiente, podríamos crear sub-grupos "TenantId_RolName" 
        // en el OnConnectedAsync del Hub. Por ahora lo enviamos al Tenant completo o 
        // requeriría lógica adicional de filtrado.
        
        // Simulación: Enviamos al grupo del Tenant (los clientes deben filtrar por rol si es necesario)
        // Opcionalmente, implementar sub-grupos.
        await _hubContext.Clients.Group(tenantId.ToString())
            .ReceiveNotification(titulo, $"{rol.ToUpper()}: {mensaje}", "role_specific");
    }
}
