using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Enums;

namespace BA.Backend.Infrastructure.Services.Integrations;

/// <summary>
/// Adaptador de Pruebas (Mock) para validar el flujo del Hub sin dependencias externas.
/// </summary>
public class MockIntegrationAdapter : IExternalIntegrationService
{
    public IntegrationType Type => IntegrationType.LocalManual;

    public Task<int> GetStockAsync(Guid tenantId, string externalSku, CancellationToken ct)
    {
        // Simulamos stock aleatorio para pruebas (entre 10 y 99)
        var randomStock = new Random().Next(10, 100);
        return Task.FromResult(randomStock);
    }

    public Task<IEnumerable<BA.Backend.Application.Common.Models.ExternalProductDto>> GetExternalCatalogAsync(Guid tenantId, CancellationToken ct)
    {
        var mockCatalog = new List<BA.Backend.Application.Common.Models.ExternalProductDto>
        {
            new("SKU-COL-001", "Savory Mega Almendras", 2500, "Venta", true, "Paleta de helado de vainilla con cobertura de chocolate y almendras"),
            new("SKU-COL-002", "Savory Danky 21", 1800, "Venta", true, "Cono de helado premium"),
            new("SKU-COL-003", "Savory Centella", 500, "Venta", true, "Helado de agua clásico chileno")
        };
        return Task.FromResult<IEnumerable<BA.Backend.Application.Common.Models.ExternalProductDto>>(mockCatalog);
    }

    public Task<string> GetRedirectUrlAsync(Guid tenantId, Guid userId, string? additionalParams, string? redirectTemplate, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(redirectTemplate))
            return Task.FromResult("https://mock-portal.savory.cl/pedidos?u=" + userId);

        var finalUrl = redirectTemplate.Replace("{sku}", additionalParams ?? "");
        return Task.FromResult(finalUrl);
    }

    public Task<bool> PingExternalAsync(Guid tenantId, CancellationToken ct) => Task.FromResult(true);
}
