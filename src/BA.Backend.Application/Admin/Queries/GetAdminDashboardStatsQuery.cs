using BA.Backend.Application.Admin.DTOs;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BA.Backend.Application.Admin.Queries;

public record GetAdminDashboardStatsQuery(Guid TenantId) : IRequest<AdminDashboardStatsDto>;

public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly string _connectionString;

    public GetAdminDashboardStatsQueryHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken ct)
    {
        using var connection = new SqlConnection(_connectionString);

        // Utilizamos Dapper QueryMultiple para traer todos los contadores en 1 sola llamada a la base de datos (alta eficiencia).
        const string sql = @"
            SELECT COUNT(1) FROM dbo.Orders o WHERE o.TenantId = @TenantId AND o.Status NOT IN ('Entregado', 'Cancelado');
            SELECT COUNT(1) FROM dbo.Coolers c INNER JOIN dbo.Stores s ON c.StoreId = s.Id WHERE s.TenantId = @TenantId AND c.IsActive = 1;
            SELECT COUNT(1) FROM dbo.Mermas m WHERE CAST(m.CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE);
            SELECT COUNT(1) FROM dbo.TechSupportRequests t WHERE t.Status IN ('Pendiente', 'Asignado') AND (SELECT s.TenantId FROM dbo.Coolers c INNER JOIN dbo.Stores s ON c.StoreId = s.Id WHERE c.Id = t.CoolerId) = @TenantId;
            SELECT COUNT(1) FROM dbo.Stores s WHERE s.TenantId = @TenantId AND s.IsActive = 1;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { TenantId = request.TenantId });

        var activeOrders = await multi.ReadSingleAsync<int>();
        var activeCoolers = await multi.ReadSingleAsync<int>();
        var mermasToday = await multi.ReadSingleAsync<int>();
        var pendingTickets = await multi.ReadSingleAsync<int>();
        var totalStores = await multi.ReadSingleAsync<int>();

        return new AdminDashboardStatsDto(
            activeOrders,
            activeCoolers,
            mermasToday,
            pendingTickets,
            totalStores
        );
    }
}
