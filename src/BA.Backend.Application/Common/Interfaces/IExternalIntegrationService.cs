using BA.Backend.Domain.Enums;

namespace BA.Backend.Application.Common.Interfaces;

public interface IExternalIntegrationService
{
    IntegrationType Type { get; }
    Task<int> GetStockAsync(Guid tenantId, string externalSku, CancellationToken ct);
    Task<IEnumerable<BA.Backend.Application.Common.Models.ExternalProductDto>> GetExternalCatalogAsync(Guid tenantId, CancellationToken ct);
    Task<string> GetRedirectUrlAsync(Guid tenantId, Guid userId, string? additionalParams, string? redirectTemplate, CancellationToken ct);
    Task<bool> PingExternalAsync(Guid tenantId, CancellationToken ct);
}
