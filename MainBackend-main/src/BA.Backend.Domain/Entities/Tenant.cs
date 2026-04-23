using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class Tenant : IBaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public bool IsActive { get; set; }
    
    public int IntegrationType { get; set; } = 0;
    public string? IntegrationConfigJson { get; set; }
    public string? ExternalOrderUrl { get; set; }
    public string? RedirectTemplate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
