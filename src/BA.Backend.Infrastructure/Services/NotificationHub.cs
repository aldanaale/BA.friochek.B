using BA.Backend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BA.Backend.Infrastructure.Services;

[Authorize]
public class NotificationHub : Hub<INotificationHubClient>
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrEmpty(tenantId))
        {
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario";
            
            // Unimos al usuario al grupo de su Tenant para difusión masiva dentro de la marca
            await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
            
            _logger.LogInformation("SignalR: Usuario {UserName} ({UserId}) conectado al Hub (Tenant: {TenantId})", 
                userName, userId, tenantId);

            // 1. Notificación personal de bienvenida
            await Clients.Caller.ReceiveNotification(
                "¡Bienvenido!", 
                $"Hola {userName}, has iniciado sesión correctamente en BA.FrioCheck.", 
                "success");

            // 2. Notificación al resto del Tenant (Opcional, pero pedida por el usuario "notificación de todo")
            await Clients.OthersInGroup(tenantId).ReceiveNotification(
                "Nueva Conexión",
                $"El usuario {userName} se ha unido a la plataforma.",
                "info");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("SignalR: Usuario {UserId} desconectado del Hub", userId);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Ping para mantener la conexión activa o verificar latencia.
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.ReceiveNotification("Pong", DateTime.UtcNow.ToString("o"), "ping");
    }
}
