using BA.Backend.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BA.Backend.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CoolerId { get; private set; }
    public string NfcTagId { get; private set; } = null!;
    public string Status { get; private set; } = "PorPagar";
    public int Total { get; private set; }
    public DateTime? DispatchDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public User User { get; private set; } = null!;
    public Cooler Cooler { get; private set; } = null!;

    private Order() { }

    public static Order Create(Guid tenantId, Guid userId, Guid coolerId, string nfcTagId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            CoolerId = coolerId,
            NfcTagId = nfcTagId,
            Status = "PorPagar",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, string productName, int quantity, int unitPrice, int coolerCapacity)
    {
        ValidateCapacity(quantity, coolerCapacity);

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(OrderItem.Create(Id, productId, productName, quantity, unitPrice));
        }

        UpdateTotal();
    }

    public void UpdateItem(Guid itemId, int newQuantity, int coolerCapacity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return;

        int currentTotalQuantity = _items.Sum(i => i.Quantity) - item.Quantity + newQuantity;
        if (currentTotalQuantity > coolerCapacity)
        {
            throw new DomainException("CAPACITY_EXCEEDED", "El pedido supera la capacidad del cooler");
        }

        item.UpdateQuantity(newQuantity);
        UpdateTotal();
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            UpdateTotal();
        }
    }

    private void ValidateCapacity(int newQuantity, int coolerCapacity)
    {
        int currentTotalQuantity = _items.Sum(i => i.Quantity);
        if (currentTotalQuantity + newQuantity > coolerCapacity)
        {
            throw new DomainException("CAPACITY_EXCEEDED", "El pedido supera la capacidad del cooler");
        }
    }

    private void UpdateTotal()
    {
        Total = _items.Sum(i => i.Subtotal);
    }

    public void Confirm()
    {
        if (!_items.Any())
            throw new DomainException("EMPTY_ORDER", "No se puede confirmar un pedido sin items");

        Status = "PorPagar";
        DispatchDate = null;
    }

    public void MarkAsDelivered(string tagId, Dictionary<Guid, int>? deliveredQuantities = null)
    {
        if (this.NfcTagId != tagId)
            throw new DomainException("NFC_MISMATCH", "El tag escaneado no coincide con el del cooler.");
        
        if (deliveredQuantities != null)
        {
            foreach (var item in _items)
            {
                if (deliveredQuantities.TryGetValue(item.Id, out int deliveredQty))
                {
                    if (deliveredQty >= 0 && deliveredQty <= item.Quantity)
                    {
                        item.UpdateQuantity(deliveredQty);
                    }
                }
            }
            UpdateTotal();
        }

        Status = "Entregado";
    }
}
