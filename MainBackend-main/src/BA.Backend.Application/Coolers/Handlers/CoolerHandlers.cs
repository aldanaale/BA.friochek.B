using BA.Backend.Application.Coolers.Commands;
using BA.Backend.Application.Coolers.DTOs;
using BA.Backend.Application.Coolers.Queries;
using BA.Backend.Application.Exceptions;
using BA.Backend.Application.Transportista;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Coolers.Handlers;

public class CoolerHandlers :
    IRequestHandler<GetAllCoolersQuery, IEnumerable<CoolerListDto>>,
    IRequestHandler<GetCoolerByIdQuery, CoolerDto?>,
    IRequestHandler<GetCoolerTagsQuery, NfcTagDto?>,
    IRequestHandler<CreateCoolerCommand, Guid>,
    IRequestHandler<UpdateCoolerCommand, bool>,
    IRequestHandler<DeleteCoolerCommand, bool>,
    IRequestHandler<UpdateCoolerStatusCommand, bool>
{
    private readonly ICoolerRepository _repository;
    private readonly IStoreRepository _storeRepository;

    public CoolerHandlers(ICoolerRepository repository, IStoreRepository storeRepository)
    {
        _repository = repository;
        _storeRepository = storeRepository;
    }

    public async Task<IEnumerable<CoolerListDto>> Handle(GetAllCoolersQuery request, CancellationToken ct)
    {
        var coolers = await _repository.GetAllByTenantAsync(request.TenantId, ct);
        return coolers.Select(c => new CoolerListDto(
            c.Id, c.Name, c.SerialNumber, c.Model, c.Capacity, c.Status, c.Store?.Name ?? "N/A"
        ));
    }

    public async Task<CoolerDto?> Handle(GetCoolerByIdQuery request, CancellationToken ct)
    {
        var cooler = await _repository.GetByIdWithTenantAsync(request.Id, request.TenantId, ct);
        if (cooler == null) return null;

        return new CoolerDto(
            cooler.Id, cooler.TenantId, cooler.StoreId, cooler.Name,
            cooler.SerialNumber, cooler.Model, cooler.Capacity, cooler.Status,
            cooler.LastMaintenanceAt, cooler.CreatedAt,
            cooler.NfcTag != null ? new NfcTagDto(
                cooler.NfcTag.TagId, cooler.NfcTag.SecurityHash,
                cooler.NfcTag.IsEnrolled, cooler.NfcTag.Status, cooler.NfcTag.EnrolledAt
            ) : null
        );
    }

    public async Task<NfcTagDto?> Handle(GetCoolerTagsQuery request, CancellationToken ct)
    {
        var cooler = await _repository.GetByIdWithTenantAsync(request.CoolerId, request.TenantId, ct);
        if (cooler?.NfcTag == null) return null;

        return new NfcTagDto(
            cooler.NfcTag.TagId, cooler.NfcTag.SecurityHash,
            cooler.NfcTag.IsEnrolled, cooler.NfcTag.Status, cooler.NfcTag.EnrolledAt
        );
    }

    public async Task<Guid> Handle(CreateCoolerCommand request, CancellationToken ct)
    {
        var store = await _storeRepository.GetByIdAsync(request.StoreId, ct);
        if (store == null || store.TenantId != request.TenantId)
            throw new KeyNotFoundException("Store no encontrado o no pertenece al tenant");

        var cooler = new Cooler
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StoreId = request.StoreId,
            Name = request.Name ?? string.Empty,
            SerialNumber = request.SerialNumber,
            Model = request.Model,
            Capacity = request.Capacity,
            Status = string.IsNullOrEmpty(request.Status) ? "SinAsignar" : request.Status,
            CreatedAt = DateTime.UtcNow
        };


        await _repository.AddAsync(cooler, ct);
        return cooler.Id;
    }

    public async Task<bool> Handle(UpdateCoolerCommand request, CancellationToken ct)
    {
        var cooler = await _repository.GetByIdWithTenantAsync(request.Id, request.TenantId, ct);
        if (cooler == null) return false;

        if (request.Name != null) cooler.Name = request.Name;
        if (request.Model != null) cooler.Model = request.Model;
        if (request.Capacity.HasValue && request.Capacity > 0) cooler.Capacity = request.Capacity.Value;
        if (request.Status != null)
        {
            // Normalizar 'Activo' a 'Operativo' (según dominio)
            cooler.Status = request.Status.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? "Operativo" : request.Status;
        }

        // Actualizar SerialNumber solo si cambia, verificando unicidad para evitar DbUpdateException por IX_Coolers_SerialNumber
        if (request.SerialNumber != null && !request.SerialNumber.Equals(cooler.SerialNumber, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _repository.GetBySerialNumberAsync(request.SerialNumber, ct);
            if (existing != null && existing.Id != cooler.Id)
                throw new Exceptions.ValidationException(new Dictionary<string, string[]>
                {
                    ["serialNumber"] = new[] { $"El SerialNumber '{request.SerialNumber}' ya está registrado en otro equipo." }
                });
            cooler.SerialNumber = request.SerialNumber;
        }

        await _repository.UpdateAsync(cooler, ct);
        return true;
    }

    public async Task<bool> Handle(DeleteCoolerCommand request, CancellationToken ct)
    {
        var cooler = await _repository.GetByIdWithTenantAsync(request.Id, request.TenantId, ct);
        if (cooler == null) return false;

        await _repository.DeleteAsync(request.Id, ct);
        return true;
    }

    public async Task<bool> Handle(UpdateCoolerStatusCommand request, CancellationToken ct)
    {
        var cooler = await _repository.GetByIdWithTenantAsync(request.Id, request.TenantId, ct);
        if (cooler == null) return false;

        var validStatuses = new[] { "SinAsignar", "Activo", "Inactivo", "Mantenimiento", "EnTransito", "FallaReportada", "DadoDeBaja" };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException("Status inválido");

        cooler.Status = request.Status;
        await _repository.UpdateAsync(cooler, ct);
        return true;
    }
}