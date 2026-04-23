using BA.Backend.Application.Common.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Common.Queries;

// --- QUERIES ---
public record GetTechnicianDashboardQuery(Guid UserId, Guid TenantId) : IRequest<TechnicianHomeResponse>;
public record GetAdminDashboardQuery(Guid UserId, Guid TenantId) : IRequest<AdminHomeResponse>;
public record GetDeliveryDashboardQuery(Guid UserId, Guid TenantId) : IRequest<DeliveryHomeResponse>;

// --- HANDLERS ---

public class GetFrontendDashboardsHandler : 
    IRequestHandler<GetTechnicianDashboardQuery, TechnicianHomeResponse>,
    IRequestHandler<GetAdminDashboardQuery, AdminHomeResponse>,
    IRequestHandler<GetDeliveryDashboardQuery, DeliveryHomeResponse>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IUserRepository _userRepository;

    public GetFrontendDashboardsHandler(IDashboardRepository dashboardRepository, IUserRepository userRepository)
    {
        _dashboardRepository = dashboardRepository;
        _userRepository = userRepository;
    }

    // DASHBOARD TÉCNICO
    public async Task<TechnicianHomeResponse> Handle(GetTechnicianDashboardQuery request, CancellationToken ct)
    {
        var stats = await _dashboardRepository.GetTechnicianDashboardStatsAsync(request.UserId, request.TenantId, ct);

        var tasks = stats.Tasks.Select(t => new TaskFrontendDto(
            t.Id.ToString(),
            t.StoreName,
            t.Address,
            t.Commune,
            t.AssistanceType,
            t.Status.ToLower() == "completado" ? "completed" : "pending",
            t.Lat,
            t.Lng
        )).ToList();

        return new TechnicianHomeResponse(MapearUser(stats.Id, stats.Name, stats.LastName, stats.Email, stats.TenantId, "technician"), tasks);
    }

    // DASHBOARD ADMIN
    public async Task<AdminHomeResponse> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, request.TenantId, cancellationToken);
        if (user == null || user.TenantId != request.TenantId) throw new UnauthorizedAccessException("Usuario no encontrado");

        return new AdminHomeResponse(MapearUser(user.Id.ToString(), user.Name, user.LastName, user.Email, user.TenantId.ToString(), "admin"));
    }

    // DASHBOARD DELIVERY (TRANSPORTISTA)
    public async Task<DeliveryHomeResponse> Handle(GetDeliveryDashboardQuery request, CancellationToken ct)
    {
        var stats = await _dashboardRepository.GetDeliveryDashboardStatsAsync(request.UserId, request.TenantId, ct);

        return new DeliveryHomeResponse(MapearUser(stats.Id, stats.Name, stats.LastName, stats.Email, stats.TenantId, "delivery"));
    }

    private static FrontendUserDto MapearUser(string id, string name, string lastName, string email, string tenantId, string role)
    {
        return new FrontendUserDto(
            id,
            role,
            tenantId,
            name,
            lastName,
            email,
            "N/A"
        );
    }
}
