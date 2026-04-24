using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Queries;
using BA.Backend.Domain.Enums;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Transportista.Handlers;

public class TransportistaAdminHandler :
    IRequestHandler<GetAllTransportistasQuery, IEnumerable<TransportistaDto>>,
    IRequestHandler<GetTransportistaByIdQuery, TransportistaDto?>,
    IRequestHandler<CreateTransportistaCommand, Guid>,
    IRequestHandler<UpdateTransportistaCommand, bool>
{
    private readonly ITransportistaRepository _transportistaRepository;
    private readonly IUserRepository _userRepository;

    public TransportistaAdminHandler(ITransportistaRepository transportistaRepository, IUserRepository userRepository)
    {
        _transportistaRepository = transportistaRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<TransportistaDto>> Handle(GetAllTransportistasQuery request, CancellationToken ct)
    {
        var transportistas = await _transportistaRepository.GetAllByTenantAsync(request.TenantId, ct);
        var result = new List<TransportistaDto>();

        foreach (var t in transportistas)
        {
            if (t.User != null)
            {
                result.Add(new TransportistaDto(t.UserId, t.TenantId, t.User.Email, t.User.FullName, t.IsAvailable, t.VehiclePlate, t.CreatedAt));
            }
        }

        return result;
    }

    public async Task<TransportistaDto?> Handle(GetTransportistaByIdQuery request, CancellationToken ct)
    {
        var transportista = await _transportistaRepository.GetByIdAsync(request.Id, ct);
        if (transportista == null || transportista.TenantId != request.TenantId) return null;

        var user = await _userRepository.GetByIdAsync(request.Id, request.TenantId, ct);
        if (user == null) return null;

        return new TransportistaDto(transportista.UserId, transportista.TenantId, user.Email, user.FullName, transportista.IsAvailable, transportista.VehiclePlate, transportista.CreatedAt);
    }

    public async Task<Guid> Handle(CreateTransportistaCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, request.TenantId, ct);
        if (user == null || user.TenantId != request.TenantId)
            throw new KeyNotFoundException("Usuario no encontrado o no pertenece al tenant");

        if (user.Role != UserRole.Transportista)
            throw new ArgumentException("El usuario debe tener Role=3 (Transportista)");

        var transportista = new Domain.Entities.Transportista
        {
            UserId = request.UserId,
            TenantId = request.TenantId,
            IsAvailable = true,
            VehiclePlate = request.VehiclePlate,
            CreatedAt = DateTime.UtcNow
        };

        await _transportistaRepository.AddAsync(transportista, ct);
        return transportista.UserId;
    }

    public async Task<bool> Handle(UpdateTransportistaCommand request, CancellationToken ct)
    {
        var transportista = await _transportistaRepository.GetByIdAsync(request.Id, ct);
        if (transportista == null || transportista.TenantId != request.TenantId) return false;

        transportista.IsAvailable = request.IsAvailable;
        if (request.VehiclePlate != null)
            transportista.VehiclePlate = request.VehiclePlate;

        await _transportistaRepository.UpdateAsync(transportista, ct);
        return true;
    }
}
