using System;

namespace BA.Backend.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; internal set; }
    public int UnitPrice { get; private set; }
    public int Subtotal { get; internal set; }

    public Order Order { get; private set; } = null!;

    private OrderItem() { }

    internal static OrderItem Create(Guid orderId, Guid productId, string productName, int quantity, int unitPrice)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = quantity * unitPrice
        };
    }

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(newQuantity));

        Quantity = newQuantity;
        Subtotal = Quantity * UnitPrice;
    }
}
