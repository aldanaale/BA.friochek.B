
using BA.Backend.Infrastructure.Services;

namespace BA.Backend.Infrastructure.Services;

public class NotificacionService : INotificacionService
{
    public async Task EnviarNotificacionAsync(string mensaje, Guid tecnicoId)
    {
        await Task.CompletedTask;
    }
} 
