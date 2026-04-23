using System;

namespace BA.Backend.Domain.Entities;

public class TechSupportRequest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string? NfcTagId { get; set; }
    public Guid CoolerId { get; set; }
    
    public string FaultType { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public string PhotoUrls { get; set; } = "[]"; 
    
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = "Pendiente"; // Empieza siempre en Pendiente
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Cooler Cooler { get; set; } = null!;
    public NfcTag? NfcTag { get; set; }
}
