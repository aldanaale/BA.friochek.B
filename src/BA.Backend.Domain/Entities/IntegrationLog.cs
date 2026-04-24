using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class IntegrationLog : IBaseEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    public string Endpoint { get; set; } = null!; // SyncCatalog, GetStock, etc.
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public double LatencyMs { get; set; }
    public string ResultSummary { get; set; } = null!;

    // IBaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
