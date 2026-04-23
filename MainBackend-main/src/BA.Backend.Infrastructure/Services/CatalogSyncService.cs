using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Infrastructure.Services;

public class CatalogSyncService : ICatalogSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly IIntegrationFactory _integrationFactory;
    private readonly ILogger<CatalogSyncService> _logger;

    public CatalogSyncService(
        ApplicationDbContext context,
        IIntegrationFactory integrationFactory,
        ILogger<CatalogSyncService> logger)
    {
        _context = context;
        _integrationFactory = integrationFactory;
        _logger = logger;
    }

    public async Task<int> SyncTenantCatalogAsync(Guid tenantId, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        int added = 0;
        int updated = 0;
        int statusCode = 200;
        string? errorMessage = null;
        string summary = "";

        try
        {
            _logger.LogInformation("Iniciando sincronización para el Tenant: {TenantId}", tenantId);

            var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, ct);
            if (tenant == null)
            {
                statusCode = 404;
                errorMessage = "Tenant no encontrado";
                return 0;
            }

            var adapter = _integrationFactory.Create(tenant);
            var externalProducts = await adapter.GetExternalCatalogAsync(tenantId, ct);

            var existingProducts = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .ToDictionaryAsync(p => p.ExternalSku!, p => p, ct);

            foreach (var extProd in externalProducts)
            {
                if (string.IsNullOrWhiteSpace(extProd.ExternalSku)) continue;

                if (existingProducts.TryGetValue(extProd.ExternalSku, out var existing))
                {
                    existing.Name = extProd.Name;
                    existing.Price = extProd.Price;
                    existing.Type = extProd.Type;
                    existing.IsActive = extProd.IsAvailable;
                    updated++;
                }
                else
                {
                    var newProduct = new Product
                    {
                        TenantId = tenantId,
                        ExternalSku = extProd.ExternalSku,
                        Name = extProd.Name,
                        Price = extProd.Price,
                        Type = extProd.Type,
                        IsActive = extProd.IsAvailable
                    };
                    _context.Products.Add(newProduct);
                    added++;
                }
            }

            await _context.SaveChangesAsync(ct);
            summary = $"{added} agregados, {updated} actualizados.";
            _logger.LogInformation("Sincronización completada: {Summary}", summary);
        }
        catch (Exception ex)
        {
            statusCode = 500;
            errorMessage = ex.Message;
            summary = "Fallo crítico durante la sincronización";
            _logger.LogError(ex, "Error sincronizando catálogo para Tenant {TenantId}", tenantId);
        }
        finally
        {
            sw.Stop();
            var log = new IntegrationLog
            {
                TenantId = tenantId,
                Endpoint = "SyncCatalog",
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                LatencyMs = sw.Elapsed.TotalMilliseconds,
                ResultSummary = summary,
                CreatedAt = DateTime.UtcNow
            };
            _context.IntegrationLogs.Add(log);
            await _context.SaveChangesAsync(ct);
        }

        return added + updated;
    }
}
