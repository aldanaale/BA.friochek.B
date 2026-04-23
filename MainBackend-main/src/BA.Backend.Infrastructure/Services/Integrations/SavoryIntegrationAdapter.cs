using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Enums;

namespace BA.Backend.Infrastructure.Services.Integrations;

/// <summary>
/// Adaptador específico para Savory (Futura integración real).
/// </summary>
public class SavoryIntegrationAdapter : IExternalIntegrationService
{
    public IntegrationType Type => IntegrationType.SavoryDirect;

    public Task<int> GetStockAsync(Guid tenantId, string externalSku, CancellationToken ct)
    {
        // Por ahora se comporta como Mock, pero aquí irá la llamada HTTP a Savory
        return Task.FromResult(42); 
    }

    public Task<IEnumerable<BA.Backend.Application.Common.Models.ExternalProductDto>> GetExternalCatalogAsync(Guid tenantId, CancellationToken ct)
    {
        // Boilerplate para futura integración real
        return Task.FromResult<IEnumerable<BA.Backend.Application.Common.Models.ExternalProductDto>>(new List<BA.Backend.Application.Common.Models.ExternalProductDto>());
    }

    public Task<string> GetRedirectUrlAsync(Guid tenantId, Guid userId, string? additionalParams, string? redirectTemplate, CancellationToken ct)
    {
        // Si no hay plantilla, usamos el link base por defecto
        if (string.IsNullOrWhiteSpace(redirectTemplate))
            return Task.FromResult("https://www.savory.cl/portal-pedidos");

        // Procesamos la plantilla (Deep Linking - Opción B)
        var finalUrl = redirectTemplate.Replace("{sku}", additionalParams ?? "");
        return Task.FromResult(finalUrl);
    }

    public Task<bool> PingExternalAsync(Guid tenantId, CancellationToken ct) => Task.FromResult(true);
}
