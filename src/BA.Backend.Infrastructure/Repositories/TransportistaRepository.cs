using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Transportista;
using BA.Backend.Infrastructure;
using BA.Backend.Application.Tecnico.Interfaces;

using Microsoft.Data.SqlClient;
using Dapper;
using BA.Backend.Infrastructure.Settings;

namespace BA.Backend.Infrastructure.Repositories;

public class TransportistaRepository : ITransportistaRepository
{
    private readonly string _connectionString;

    public TransportistaRepository(DatabaseSettings settings)
    {
        _connectionString = settings.ConnectionString;
    }

    public Task<List<RouteStopDto>> GetDailyRouteAsync(Guid transportistId, DateTime routeDate) 
        => Task.FromResult(new List<RouteStopDto>());

    public async Task<List<TransportistaRouteDto>> GetPendingRouteStopsAsync(Guid transportistId, Guid tenantId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT 
                rs.Id AS RouteStopId,
                o.Id AS OrderId,
                s.Name AS StoreName,
                s.Address AS StoreAddress,
                '' AS StoreCity,
                o.Total AS OrderTotal,
                (SELECT COUNT(*) FROM OrderItems WHERE OrderId = o.Id) AS ItemsCount,
                rs.Status AS Status,
                o.CreatedAt AS OrderDate
            FROM RouteStops rs
            INNER JOIN Routes r ON rs.RouteId = r.Id
            INNER JOIN Orders o ON rs.OrderId = o.Id
            INNER JOIN Stores s ON rs.StoreId = s.Id
            WHERE r.TransportistId = @TransportistId 
              AND r.TenantId = @TenantId
              AND rs.Status = 'Pendiente'
            ORDER BY o.CreatedAt ASC";

        var result = await connection.QueryAsync<TransportistaRouteDto>(sql, new { TransportistId = transportistId, TenantId = tenantId });
        return result.ToList();
    }

    public Task<MachineDetailDto> GetMachineByNfcTagAsync(string nfcTagId) 
        => Task.FromResult<MachineDetailDto>(null!);

    public Task ValidateNfcTagAsync(string nfcTagId, Guid expectedMachineId) 
        => Task.CompletedTask;

    public Task<DeliveryResultDto> RegisterDeliveryAsync(RegisterDeliveryCommand command) 
        => Task.FromResult(new DeliveryResultDto{DeliveryId = Guid.NewGuid(), MachinesIncluded = 1, TotalProductsDelivered = 50, DeliveredAt = DateTime.UtcNow, Status = "Stub"});
    public Task<WastePickupResultDto> RegisterWastePickupAsync(RegisterWastePickupCommand command) 
        => Task.FromResult(new WastePickupResultDto(Guid.NewGuid(), command.MachineId, 5, "StubPhoto", DateTime.UtcNow));
    public Task<SupportTicketResultDto> CreateSupportTicketAsync(CreateSupportTicketCommand command) 
        => Task.FromResult(new SupportTicketResultDto(Guid.NewGuid(), "TKT-001", command.MachineId, command.Category, TicketStatus.Open, DateTime.UtcNow));
    public Task<List<MovementSummaryDto>> GetMachineHistoryAsync(Guid machineId, DateTime? from, DateTime? to, MovementType? type, int page, int size) 
        => Task.FromResult(new List<MovementSummaryDto>());
    public Task<List<SupportTicketResultDto>> GetPendingTicketsByRouteAsync(Guid transportistId, DateTime routeDate) 
        => Task.FromResult(new List<SupportTicketResultDto>());
}

public class NfcValidationService : INfcValidationService
{
    public Task ValidateTagAsync(string scannedTagId, Guid machineId) 
        => Task.CompletedTask;

    public Task<bool> IsTagRegisteredAsync(string nfcTagId) 
        => Task.FromResult(true);
}
