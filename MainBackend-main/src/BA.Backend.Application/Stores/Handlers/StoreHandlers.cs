using BA.Backend.Application.Stores.Commands;
using BA.Backend.Application.Stores.DTOs;
using BA.Backend.Application.Stores.Queries;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Application.Stores.Handlers;

/// <summary>
/// Handler unificado para el CRUD de Stores.
/// FIX: reemplazados todos los Console.WriteLine por ILogger estructurado.
/// </summary>
public class StoreHandlers :
    IRequestHandler<CreateStoreCommand, StoreDto>,
    IRequestHandler<UpdateStoreCommand, StoreDto>,
    IRequestHandler<DeleteStoreCommand, bool>,
    IRequestHandler<GetAllStoresQuery, IEnumerable<StoreDto>>,
    IRequestHandler<GetStoreByIdQuery, StoreDto?>
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<StoreHandlers> _logger;

    public StoreHandlers(IStoreRepository storeRepository, ILogger<StoreHandlers> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creando tienda '{Name}' para tenant {TenantId}", request.Name, request.TenantId);

        var store = new Store
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            Address = request.Address,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow,
            City = request.City ?? string.Empty,
            District = request.District ?? string.Empty
        };

        await _storeRepository.AddAsync(store, ct);
        _logger.LogInformation("Tienda '{Name}' creada con ID {StoreId}", store.Name, store.Id);

        return MapToDto(store);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public async Task<StoreDto> Handle(UpdateStoreCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Actualizando tienda {StoreId}", request.Id);

        var store = await _storeRepository.GetByIdAsync(request.Id, ct);

        if (store == null || store.TenantId != request.TenantId)
        {
            _logger.LogWarning("Tienda {StoreId} no encontrada o no pertenece al tenant {TenantId}",
                request.Id, request.TenantId);
            throw new KeyNotFoundException($"Tienda {request.Id} no encontrada");
        }

        store.Name = request.Name;
        store.Address = request.Address;
        store.ContactName = request.ContactName;
        store.ContactPhone = request.ContactPhone;
        store.Latitude = request.Latitude;
        store.Longitude = request.Longitude;
        store.IsActive = request.IsActive;
        store.City = request.City ?? string.Empty;
        store.District = request.District ?? string.Empty;

        await _storeRepository.UpdateAsync(store, ct);
        _logger.LogInformation("Tienda {StoreId} actualizada", store.Id);

        return MapToDto(store);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public async Task<bool> Handle(DeleteStoreCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Eliminando tienda {StoreId}", request.Id);

        var store = await _storeRepository.GetByIdAsync(request.Id, ct);

        if (store == null || store.TenantId != request.TenantId)
        {
            _logger.LogWarning("Tienda {StoreId} no encontrada para eliminar", request.Id);
            return false;
        }

        await _storeRepository.DeleteAsync(request.Id, ct);
        _logger.LogInformation("Tienda {StoreId} eliminada", request.Id);

        return true;
    }

    // ── GetAll ────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<StoreDto>> Handle(GetAllStoresQuery request, CancellationToken ct)
    {
        _logger.LogDebug("Listando tiendas para tenant {TenantId}", request.TenantId);
        var stores = await _storeRepository.GetByTenantIdAsync(request.TenantId, ct);
        return stores.Select(MapToDto);
    }

    // ── GetById ───────────────────────────────────────────────────────────────
    public async Task<StoreDto?> Handle(GetStoreByIdQuery request, CancellationToken ct)
    {
        _logger.LogDebug("Buscando tienda {StoreId}", request.Id);
        var store = await _storeRepository.GetByIdAsync(request.Id, ct);

        if (store == null || store.TenantId != request.TenantId)
        {
            _logger.LogDebug("Tienda {StoreId} no encontrada", request.Id);
            return null;
        }

        return MapToDto(store);
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static StoreDto MapToDto(Store store) => new(
        store.Id,
        store.Name,
        store.Address,
        store.ContactName,
        store.ContactPhone,
        store.Latitude,
        store.Longitude,
        store.IsActive,
        store.CreatedAt
    );
}
