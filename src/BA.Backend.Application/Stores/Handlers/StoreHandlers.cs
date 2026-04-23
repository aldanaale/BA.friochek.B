
using BA.Backend.Application.Stores.Commands;
using BA.Backend.Application.Stores.DTOs;
using BA.Backend.Application.Stores.Queries;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Stores.Handlers;

public class StoreHandlers : 
    IRequestHandler<CreateStoreCommand, StoreDto>,
    IRequestHandler<UpdateStoreCommand, StoreDto>,
    IRequestHandler<DeleteStoreCommand, bool>,
    IRequestHandler<GetAllStoresQuery, IEnumerable<StoreDto>>,
    IRequestHandler<GetStoreByIdQuery, StoreDto?>
{
    private readonly IStoreRepository _storeRepository;

    public StoreHandlers(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken ct)
    {
        Console.WriteLine("Creando una tienda nueva: " + request.Name);
        
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
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _storeRepository.AddAsync(store, ct);
        Console.WriteLine("Tienda guardada con exito en la base de datos");

        return MapToDto(store);
    }

    public async Task<StoreDto> Handle(UpdateStoreCommand request, CancellationToken ct)
    {
        Console.WriteLine("Buscando tienda para actualizar ID: " + request.Id);
        var store = await _storeRepository.GetByIdAsync(request.Id, ct);
        
        if (store == null || store.TenantId != request.TenantId)
        {
            Console.WriteLine("No se encontro la tienda o no pertenece a este tenant");
            throw new KeyNotFoundException("Tienda no encontrada");
        }

        store.Name = request.Name;
        store.Address = request.Address;
        store.ContactName = request.ContactName;
        store.ContactPhone = request.ContactPhone;
        store.Latitude = request.Latitude;
        store.Longitude = request.Longitude;
        store.IsActive = request.IsActive;

        await _storeRepository.UpdateAsync(store, ct);
        Console.WriteLine("Datos de la tienda actualizados correctamente");

        return MapToDto(store);
    }

    public async Task<bool> Handle(DeleteStoreCommand request, CancellationToken ct)
    {
        Console.WriteLine("Buscando tienda para borrar ID: " + request.Id);
        var store = await _storeRepository.GetByIdAsync(request.Id, ct);
        
        if (store == null || store.TenantId != request.TenantId)
        {
            Console.WriteLine("No se pudo borrar porque la tienda no existe o no es de este tenant");
            return false;
        }

        await _storeRepository.DeleteAsync(request.Id, ct);
        Console.WriteLine("Tienda eliminada de la base de datos");
        return true;
    }

    public async Task<IEnumerable<StoreDto>> Handle(GetAllStoresQuery request, CancellationToken ct)
    {
        Console.WriteLine("Cargando todas las tiendas para el Tenant: " + request.TenantId);
        var stores = await _storeRepository.GetByTenantIdAsync(request.TenantId, ct);
        
        return stores.Select(MapToDto);
    }

    public async Task<StoreDto?> Handle(GetStoreByIdQuery request, CancellationToken ct)
    {
        Console.WriteLine("Buscando detalle de la tienda ID: " + request.Id);
        var store = await _storeRepository.GetByIdAsync(request.Id, ct);
        
        if (store == null || store.TenantId != request.TenantId)
        {
            Console.WriteLine("Tienda no encontrada");
            return null;
        }

        return MapToDto(store);
    }

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
