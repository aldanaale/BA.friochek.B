using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

public interface IOrderRepository
{
    /// <summary>
    /// Obtiene un pedido por su ID sin cargar los ítems relacionados.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un pedido con sus ítems, filtrado por tenant para garantizar el aislamiento de datos.
    /// </summary>
    Task<Order?> GetByIdWithItemsAsync(Guid id, Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Retorna todos los pedidos de un usuario dentro del tenant, ordenados por fecha descendente.
    /// </summary>
    Task<List<Order>> GetByUserIdAsync(Guid userId, Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Retorna los pedidos de un usuario paginados, incluyendo el conteo total para la paginación del cliente.
    /// </summary>
    Task<(List<Order> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId, Guid tenantId, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Agrega un nuevo pedido al contexto. Requiere llamar a <see cref="SaveChangesAsync"/> para persistir.
    /// </summary>
    Task AddAsync(Order order, CancellationToken ct = default);

    /// <summary>
    /// Marca el pedido como modificado en el contexto. Requiere llamar a <see cref="SaveChangesAsync"/> para persistir.
    /// </summary>
    Task UpdateAsync(Order order, CancellationToken ct = default);

    /// <summary>
    /// Elimina un pedido por su ID. No lanza excepción si el pedido no existe.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Crea una referencia de pedido externo (sin cooler físico) para pedidos que se originan
    /// fuera de la plataforma NFC y retorna el ID de referencia externo generado.
    /// </summary>
    Task<string> CreateExternalOrderReferenceAsync(
        Guid userId, Guid tenantId, Guid productId, string redirectUrl, CancellationToken ct = default);

    /// <summary>
    /// Persiste todos los cambios pendientes en la unidad de trabajo actual.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
