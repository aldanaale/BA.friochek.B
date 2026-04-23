using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Cliente.Queries;
using BA.Backend.Application.Common.DTOs;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

// Alias para resolver ambiguedad entre Cliente.DTOs.TiendaDto y Common.DTOs.TiendaDto
using ClienteTiendaDto = BA.Backend.Application.Cliente.DTOs.TiendaDto;

namespace BA.Backend.Application.Cliente.Handlers;

public class GetClientHomeQueryHandler : IRequestHandler<GetClientHomeQuery, ClientHomeDto>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IConfiguration _configuration;

    public GetClientHomeQueryHandler(IDashboardRepository dashboardRepository, IConfiguration configuration)
    {
        _dashboardRepository = dashboardRepository;
        _configuration = configuration;
    }

    public async Task<ClientHomeDto> Handle(GetClientHomeQuery request, CancellationToken ct)
    {
        var stats = await _dashboardRepository.GetClientDashboardStatsAsync(request.UserId, request.TenantId, ct);

        var response = new ClientHomeDto
        {
            UserFullName = stats.FullName,
            User = new UserSummaryDto
            {
                Email = stats.Email,
                Nombre = stats.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "",
                Apellido = stats.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault() ?? ""
            },
            Tienda = new ClienteTiendaDto
            {
                Nombre = stats.TiendaNombre,
                Direccion = stats.TiendaDireccion
            },
            Coolers = stats.Coolers.Select(c => new CoolerDto
            {
                CoolerId = c.Id,
                Model = c.Model,
                Status = c.Status,
                LastRevisionAt = c.LastRevisionAt
            }).ToList(),
            ActiveOrders = stats.ActiveOrders.Select(o => new HomeOrderDto
            {
                OrderId = o.OrderId,
                Status = o.Status,
                Title = o.Status == "PorPagar" ? "Pedido pendiente de pago" : $"Pedido {o.OrderId.ToString()[..8]}",
                Description = $"Cooler {o.CoolerId}",
                CreatedAt = o.CreatedAt,
                DispatchDate = o.DispatchDate,
                IsInProgress = o.Status != "Entregado"
            }).ToList(),
            TechRequests = stats.TechRequests.Select(r => new TechRequestDto
            {
                RequestId = r.Id,
                FaultType = r.FaultType,
                Status = r.Status,
                ScheduledDate = r.ScheduledDate
            }).ToList()
        };

        response.Orders = response.ActiveOrders;
        response.CurrentOrdersCount = response.ActiveOrders.Count;
        response.TotalCoolers = response.Coolers.Count;
        response.OperationalCoolers = response.Coolers.Count(c => c.Status == "Activo");
        response.FaultyCoolers = response.Coolers.Count(c => c.Status != "Activo");
        response.OpenAssistanceCount = response.TechRequests.Count(r => r.Status != "Completado");

        response.SupportPhone = _configuration["Support:Phone"] ?? string.Empty;
        response.SupportEmail = _configuration["Support:Email"] ?? string.Empty;
        return response;
    }
}
