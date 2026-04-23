using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

public class OrderItem : BaseEntity
{
    // EF Core requires a parameterless constructor. Keep it internal — items are created via Order.AddItem().
    internal OrderItem() { }

    public Guid Id { get; internal set; }
    public Guid OrderId { get; internal set; }
    public Guid ProductId { get; internal set; }
    public string ProductName { get; internal set; } = string.Empty;

    // Quantity is intentionally internal set so Order domain methods can mutate it without making
    // it fully public. EF Core can still write to it via the internal accessor.
    public int Quantity { get; internal set; }

    public decimal UnitPrice { get; internal set; }

    public decimal Subtotal => Quantity * UnitPrice;

    // Navigation
    public Order Order { get; private set; } = null!;
}
