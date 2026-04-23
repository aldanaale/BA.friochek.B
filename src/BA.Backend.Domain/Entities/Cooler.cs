using System;

namespace BA.Backend.Domain.Entities;

public class Cooler
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string SerialNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Capacity { get; set; }
    public string Status { get; set; } = "Operativo"; // Operativo, En Reparación, Inactivo
    
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMaintenanceAt { get; set; }

    public Store Store { get; set; } = null!;
    public NfcTag? NfcTag { get; set; }
}
