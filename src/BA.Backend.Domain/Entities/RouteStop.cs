using System;

namespace BA.Backend.Domain.Entities;

/// <summary>
/// Indica una parada específica dentro de una ruta, vinculada a un pedido y una tienda.
/// </summary>
public class RouteStop
{
    public Guid Id { get; private set; }
    public Guid RouteId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid StoreId { get; private set; }
    public int StopOrder { get; private set; }
    public string Status { get; private set; } = "Pendiente"; // Pendiente, Entregado, Fallido
    public DateTime? ArrivalAt { get; private set; }
    public string? Notes { get; private set; }

    public virtual Route Route { get; private set; } = null!;
    public virtual Order Order { get; private set; } = null!;
    public virtual Store Store { get; private set; } = null!;

    private RouteStop() { }

    public static RouteStop Create(Guid routeId, Guid orderId, Guid storeId, int stopOrder)
    {
        return new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            OrderId = orderId,
            StoreId = storeId,
            StopOrder = stopOrder,
            Status = "Pendiente"
        };
    }

    public void MarkAsCompleted()
    {
        Status = "Entregado";
        ArrivalAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = "Fallido";
        ArrivalAt = DateTime.UtcNow;
        Notes = reason;
    }
}
