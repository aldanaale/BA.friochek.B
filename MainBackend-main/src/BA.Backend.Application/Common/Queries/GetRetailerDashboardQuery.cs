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

public record GetRetailerDashboardQuery(Guid UserId, Guid TenantId) : IRequest<RetailerHomeResponse>;

public class GetRetailerDashboardHandler : IRequestHandler<GetRetailerDashboardQuery, RetailerHomeResponse>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetRetailerDashboardHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<RetailerHomeResponse> Handle(GetRetailerDashboardQuery request, CancellationToken ct)
    {
        var stats = await _dashboardRepository.GetRetailerDashboardStatsAsync(request.UserId, request.TenantId, ct);

        var userDto = new FrontendUserDto(
            stats.Id,
            "retailer",
            stats.TenantId,
            stats.Name,
            stats.LastName,
            stats.Email,
            "N/A",
            stats.StoreName
        );

        var tiendaDto = new TiendaDto(stats.StoreName, stats.StoreAddress);

        var coolers = stats.Coolers.Select(c => new CoolerFrontendDto(
            c.Id.ToString(),
            c.Model,
            c.Status.ToLower() == "activo" ? "operativo" : c.Status.ToLower(),
            c.LastMaintenanceAt != null ? c.LastMaintenanceAt.Value.ToString("dd-MM-yyyy") : "Sin revisión",
            c.Capacity,
            c.Name
        )).ToList();

        var tech = stats.TechRequests.Select(r => new TechRequestFrontendDto(
            r.Id.ToString(),
            r.FaultType,
            r.ScheduledDate.ToString("dd-MM-yyyy"),
            MapearStatusSoporte(r.Status)
        )).ToList();

        return new RetailerHomeResponse(userDto, tiendaDto, coolers, tech);
    }

    private static string MapearStatusSoporte(string status)
    {
        return status.ToLower() switch {
            "pendiente" => "pending", "enasignacion" => "pending", "asignado" => "in_progress",
            "enreparacion" => "in_progress", "completado" => "completed", _ => "pending"
        };
    }
}
