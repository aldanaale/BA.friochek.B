using BA.Backend.Domain.Common;
using BA.Backend.Domain.Exceptions;

namespace BA.Backend.Domain.Entities;

public class Order : BaseEntity, ITenantEntity
{
    // EF Core requires a parameterless constructor; keep it private so only Create() is the public factory.
    private Order() { }

    public Guid Id { get; private set; }

    // TenantId must have a public setter to satisfy ITenantEntity (used by EF multi-tenancy filters).
    public Guid TenantId { get; set; }

    public Guid UserId { get; private set; }
    public Guid CoolerId { get; private set; }
    public string? NfcTagId { get; private set; }
    public string Status { get; private set; } = "PorPagar";
    public string? ExternalOrderId { get; private set; }
    public string? ExternalStatus { get; private set; }
    public DateTime? DispatchDate { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public Cooler Cooler { get; private set; } = null!;

    private readonly List<OrderItem> _items = new();

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Order Create(Guid userId, Guid coolerId, string? nfcTagId, Guid tenantId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("ORDER_INVALID_USER", "El ID de usuario no puede ser vacío.");

        if (coolerId == Guid.Empty)
            throw new DomainException("ORDER_INVALID_COOLER", "El ID de cooler no puede ser vacío.");

        if (tenantId == Guid.Empty)
            throw new DomainException("ORDER_INVALID_TENANT", "El ID de tenant no puede ser vacío.");

        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CoolerId = coolerId,
            NfcTagId = nfcTagId,
            TenantId = tenantId,
            Status = "PorPagar",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Order CreateExternalReference(Guid userId, Guid tenantId, string externalOrderId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("ORDER_INVALID_USER", "El ID de usuario no puede ser vacío.");

        if (tenantId == Guid.Empty)
            throw new DomainException("ORDER_INVALID_TENANT", "El ID de tenant no puede ser vacío.");

        if (string.IsNullOrWhiteSpace(externalOrderId))
            throw new DomainException("ORDER_INVALID_EXTERNAL_ID", "La referencia de pedido externo no puede ser vacía.");

        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            // CoolerId is intentionally Guid.Empty: external orders do not require a physical cooler.
            CoolerId = Guid.Empty,
            Status = "Externo",
            ExternalOrderId = externalOrderId,
            ExternalStatus = "Pendiente",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ORDER_ITEM_INVALID_PRODUCT", "El ID de producto no puede ser vacío.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("ORDER_ITEM_INVALID_NAME", "El nombre del producto es requerido.");

        if (quantity <= 0)
            throw new DomainException("ORDER_ITEM_INVALID_QUANTITY", "La cantidad debe ser mayor a 0.");

        if (unitPrice < 0)
            throw new DomainException("ORDER_ITEM_INVALID_PRICE", "El precio unitario no puede ser negativo.");

        if (Status != "PorPagar")
            throw new DomainException("ORDER_ALREADY_CONFIRMED", $"No se pueden agregar ítems a un pedido en estado '{Status}'.");

        _items.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = Id,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        });
    }

    /// <summary>
    /// Actualiza la cantidad de un ítem existente.
    /// </summary>
    public void UpdateItem(Guid itemId, int quantity)
    {
        if (itemId == Guid.Empty)
            throw new DomainException("ORDER_ITEM_INVALID_ID", "El ID de ítem no puede ser vacío.");

        if (quantity <= 0)
            throw new DomainException("ORDER_ITEM_INVALID_QUANTITY", "La cantidad debe ser mayor a 0.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Ítem {itemId} no encontrado en el pedido.");

        item.Quantity = quantity;
    }

    /// <summary>
    /// Elimina un ítem del pedido.
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        if (itemId == Guid.Empty)
            throw new DomainException("ORDER_ITEM_INVALID_ID", "El ID de ítem no puede ser vacío.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Ítem {itemId} no encontrado en el pedido.");

        _items.Remove(item);
    }

    /// <summary>
    /// Confirma el pedido. El pedido debe tener al menos un ítem.
    /// </summary>
    public void Confirm()
    {
        if (_items.Count == 0)
            throw new DomainException("ORDER_EMPTY", "No se puede confirmar un pedido sin ítems.");

        if (Status == "Confirmado")
            throw new DomainException("ORDER_ALREADY_CONFIRMED", "El pedido ya fue confirmado.");

        Status = "Confirmado";
    }

    /// <summary>
    /// Marca el pedido como entregado.
    /// </summary>
    public void MarkAsDelivered(string? nfcTagId = null)
    {
        if (Status == "Entregado")
            throw new DomainException("ORDER_ALREADY_DELIVERED", "El pedido ya fue marcado como entregado.");

        Status = "Entregado";
        NfcTagId = nfcTagId ?? NfcTagId;
        DispatchDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra la referencia del sistema externo para trazabilidad.
    /// </summary>
    public void SetExternalReference(string externalOrderId, string? externalStatus = null)
    {
        if (string.IsNullOrWhiteSpace(externalOrderId))
            throw new DomainException("ORDER_INVALID_EXTERNAL_ID", "La referencia de pedido externo no puede ser vacía.");

        ExternalOrderId = externalOrderId;
        ExternalStatus = externalStatus;
    }
}
