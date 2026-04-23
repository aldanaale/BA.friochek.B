using System;
using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class TechSupportRequest : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? TechnicianId { get; set; }
    public string? NfcTagId { get; set; }
    public Guid CoolerId { get; set; }

    public string FaultType { get; set; } = null!;
    public string Description { get; set; } = null!;

    public string PhotoUrls { get; set; } = "[]";

    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = "Pendiente"; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public User? Technician { get; set; }
    public Cooler Cooler { get; set; } = null!;
    public NfcTag? NfcTag { get; set; }

    public void Resolve(string closingComment, string? photoUrl, DateTime resolvedAt)
    {
        Status = "Resuelto";
        Description += $"\n{closingComment}";
        UpdatedAt = resolvedAt;

        if (photoUrl is not null)
        {
            List<string> photos;
            try
            {
                photos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(PhotoUrls) ?? new();
            }
            catch
            {
                photos = new();
            }
            photos.Add(photoUrl);
            PhotoUrls = System.Text.Json.JsonSerializer.Serialize(photos);
        }
    }
}
