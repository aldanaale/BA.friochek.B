
namespace BA.Backend.Infrastructure.Services;

public interface INotificacionService
{
    Task EnviarNotificacionAsync(string mensaje, Guid tecnicoId);
} 
