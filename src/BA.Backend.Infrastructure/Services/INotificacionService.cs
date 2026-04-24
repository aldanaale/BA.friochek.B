
namespace BA.Backend.Infrastructure.Services;

public interface INotificacionService
{
    Task EnviarNotificacionAsync(string mensaje, Guid tecnicoId);
    Task NotificarUsuarioAsync(Guid userId, string titulo, string mensaje);
    Task NotificarTenantAsync(Guid tenantId, string titulo, string mensaje);
    Task NotificarRolAsync(Guid tenantId, string rol, string titulo, string mensaje);
}
