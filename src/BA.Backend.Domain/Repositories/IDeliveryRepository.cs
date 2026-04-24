using BA.Backend.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Domain.Repositories;

/// <summary>
/// Interfaz para la persistencia y consulta de datos relacionados con entregas.
/// </summary>
public interface IDeliveryRepository
{
    Task<RouteStop?> GetRouteStopByIdAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
