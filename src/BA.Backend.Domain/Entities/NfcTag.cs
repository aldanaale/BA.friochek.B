using System;

namespace BA.Backend.Domain.Entities;

public class NfcTag
{
    public string TagId { get; set; } = null!;
    public Guid CoolerId { get; set; }
    public string SecurityHash { get; set; } = null!;
    public bool IsEnrolled { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? EnrolledAt { get; set; }

    public Cooler Cooler { get; set; } = null!;
}
