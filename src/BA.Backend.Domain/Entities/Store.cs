using System;

namespace BA.Backend.Domain.Entities;

public class Store
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Cooler> Coolers { get; set; } = new List<Cooler>();
}
