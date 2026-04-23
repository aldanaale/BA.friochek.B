using BA.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Infrastructure.Services;

public class CatalogSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CatalogSyncWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12);

    public CatalogSyncWorker(IServiceProvider serviceProvider, ILogger<CatalogSyncWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CatalogSyncWorker iniciado. Ciclo: {Interval}", _checkInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<ICatalogSyncService>();
                var context = scope.ServiceProvider.GetRequiredService<BA.Backend.Infrastructure.Data.ApplicationDbContext>();

                // Obtener todos los Tenants con integración activa
                // IntegrationType > 0 significa que no es Manual/Interno
                var tenantsToSync = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                    context.Tenants.Where(t => t.IsActive && t.IntegrationType > 0), 
                    stoppingToken);

                _logger.LogInformation("Iniciando ciclo de sincronización automática para {Count} tenants.", tenantsToSync.Count);

                foreach (var tenant in tenantsToSync)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    
                    try 
                    {
                        await syncService.SyncTenantCatalogAsync(tenant.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fallo al sincronizar tenant {TenantId} en ciclo automático.", tenant.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el ciclo del CatalogSyncWorker.");
            }

            // Esperar al siguiente ciclo (Manejo de cancelación grácil)
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CatalogSyncWorker deteniéndose grácilmente por cancelación.");
                break; // Salir del bucle while
            }
        }
    }
}
