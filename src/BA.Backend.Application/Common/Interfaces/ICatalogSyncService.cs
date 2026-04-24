namespace BA.Backend.Application.Common.Interfaces;

public interface ICatalogSyncService
{
    Task<int> SyncTenantCatalogAsync(Guid tenantId, CancellationToken ct);
}
