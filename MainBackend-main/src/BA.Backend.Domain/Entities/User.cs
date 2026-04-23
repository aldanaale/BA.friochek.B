using BA.Backend.Domain.Common;
using BA.Backend.Domain.Enums;

namespace BA.Backend.Domain.Entities;

public class User : BaseEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string FullName => $"{Name} {LastName}";

    public UserRole Role { get; set; }

    /// <summary>
    /// Sub-tipo de cliente. Solo aplica cuando Role == Cliente.
    /// Permite distinguir Retail, Wholesale, Chain, Horeca, Institutional, Vending.
    /// </summary>
    public ClientType? ClientType { get; set; }

    /// <summary>
    /// Sub-tipo de transportista. Solo aplica cuando Role == Transportista.
    /// Permite distinguir ProductCarrier, MachineCarrier, FreightForwarder, LastMile.
    /// </summary>
    public TransportType? TransportType { get; set; }

    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public string? ActiveSessionId { get; set; }
    public string? CurrentDeviceFingerprint { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Guid? StoreId { get; set; }

    public virtual Store? Store { get; set; }

    public bool CanLogin()
    {
        return IsActive && !IsLocked;
    }

    public void RegisterSession(string sessionId, string deviceFingerprint)
    {
        ActiveSessionId = sessionId;
        CurrentDeviceFingerprint = deviceFingerprint;
        LastLoginAt = DateTime.UtcNow;
    }
}

