using System;

namespace BA.Backend.Domain.Entities;

public class Merma
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid TransportistId { get; private set; }
    public Guid CoolerId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public string Reason { get; private set; } = null!;
    public string PhotoUrl { get; private set; } = null!;
    public string? Description { get; private set; }
    public string ScannedNfcTagId { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    public virtual Cooler Cooler { get; private set; } = null!;

    private Merma() { }

    public static Merma Create(
        Guid tenantId, 
        Guid transportistId, 
        Guid coolerId, 
        Guid productId, 
        string productName, 
        int quantity, 
        string reason, 
        string photoUrl, 
        string? description, 
        string scannedNfcTagId)
    {
        return new Merma
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportistId = transportistId,
            CoolerId = coolerId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            Reason = reason,
            PhotoUrl = photoUrl,
            Description = description,
            ScannedNfcTagId = scannedNfcTagId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
