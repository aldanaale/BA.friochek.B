using System;
using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

/// <summary>
/// Representa una nota comercial o seguimiento realizado por un vendedor sobre un cliente (Store).
/// </summary>
public class ClientNote : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Store Store { get; set; } = null!;
    public User Author { get; set; } = null!;
}
