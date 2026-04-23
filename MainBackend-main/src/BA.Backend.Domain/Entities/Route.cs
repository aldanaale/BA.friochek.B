using System;
using System.Collections.Generic;
using BA.Backend.Domain.Common;

namespace BA.Backend.Domain.Entities;

/// <summary>
/// Provee la agrupación de pedidos asignados a un transportista para una fecha determinada.
/// </summary>
public class Route : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public Guid TransportistaId { get; private set; }
    public DateTime Date { get; private set; }
    public string Status { get; private set; } = "Pendiente"; // Pendiente, EnProgreso, Completada
    public DateTime CreatedAt { get; private set; }

    private readonly List<RouteStop> _stops = new();
    public virtual IReadOnlyCollection<RouteStop> Stops => _stops.AsReadOnly();

    public virtual User Transportista { get; private set; } = null!;

    private Route() { }

    public static Route Create(Guid tenantId, Guid transportistaId, DateTime date)
    {
        return new Route
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportistaId = transportistaId,
            Date = date,
            Status = "Pendiente",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddStop(Guid storeId, int stopOrder)
    {
        _stops.Add(RouteStop.Create(Id, storeId, stopOrder));
    }

    public void MarkAsInProgress()
    {
        Status = "EnProgreso";
    }

    public void Complete()
    {
        Status = "Completada";
    }

    public void UpdateStatus(string status)
    {
        Status = status;
    }
}
