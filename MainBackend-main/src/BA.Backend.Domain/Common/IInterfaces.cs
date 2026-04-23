namespace BA.Backend.Domain.Common;

public interface IBaseEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
