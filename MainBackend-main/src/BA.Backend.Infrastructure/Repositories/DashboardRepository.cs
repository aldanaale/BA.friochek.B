using BA.Backend.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BA.Backend.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly string _connectionString;

    public DashboardRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<AdminDashboardStats> GetAdminDashboardStatsAsync(Guid tenantId, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sql = @"
            -- Coolers activos del tenant
            SELECT COUNT(1) FROM dbo.Coolers c INNER JOIN dbo.Stores s ON c.StoreId = s.Id WHERE s.TenantId = @TenantId AND c.IsDeleted = 0;
            -- Mermas de hoy
            SELECT COUNT(1) FROM dbo.Mermas m WHERE CAST(m.CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE) AND m.TenantId = @TenantId;
            -- Tickets técnicos pendientes/asignados
            SELECT COUNT(1) FROM dbo.TechSupportRequests t WHERE t.Status IN ('Pendiente', 'Asignado') AND (SELECT s.TenantId FROM dbo.Coolers c INNER JOIN dbo.Stores s ON c.StoreId = s.Id WHERE c.Id = t.CoolerId) = @TenantId;
            -- Total tiendas activas
            SELECT COUNT(1) FROM dbo.Stores s WHERE s.TenantId = @TenantId AND s.IsActive = 1;
        ";

        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: ct));

        return new AdminDashboardStats(
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>()
        );
    }

    public async Task<SupervisorDashboardStats> GetSupervisorDashboardStatsAsync(Guid tenantId, string? zone = null, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sql = @"
            -- Técnicos activos (que pertenezcan al tenant)
            SELECT COUNT(1) FROM dbo.Users u WHERE u.Role = 4 AND u.TenantId = @TenantId AND u.IsActive = 1;

            -- Tickets pendientes/asig para este tenant
            SELECT COUNT(1) FROM dbo.TechSupportRequests t
            INNER JOIN dbo.Coolers c ON t.CoolerId = c.Id
            INNER JOIN dbo.Stores s ON c.StoreId = s.Id
            WHERE t.Status IN ('Pendiente', 'Asignado') AND s.TenantId = @TenantId;

            -- Reparaciones (cierres) de hoy
            SELECT COUNT(1) FROM dbo.TechSupportRequests t
            INNER JOIN dbo.Coolers c ON t.CoolerId = c.Id
            INNER JOIN dbo.Stores s ON c.StoreId = s.Id
            WHERE t.Status = 'Completado'
              AND CAST(t.UpdatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
              AND s.TenantId = @TenantId;

            -- Alertas recientes (Placeholder: por ahora mermas críticas o fallas reportadas hoy)
            SELECT TOP 5 t.Id, 'Technical_Issue' AS Type, t.Description AS Message, t.CreatedAt, 'High' AS Severity
            FROM dbo.TechSupportRequests t
            INNER JOIN dbo.Coolers c ON t.CoolerId = c.Id
            INNER JOIN dbo.Stores s ON c.StoreId = s.Id
            WHERE s.TenantId = @TenantId AND t.Status = 'Pendiente'
            ORDER BY t.CreatedAt DESC;
        ";

        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: ct));

        var activeTechnicians = await multi.ReadSingleAsync<int>();
        var pendingTickets = await multi.ReadSingleAsync<int>();
        var repairsToday = await multi.ReadSingleAsync<int>();
        var recentAlerts = (await multi.ReadAsync<RecentAlertRecord>()).ToList();

        return new SupervisorDashboardStats(activeTechnicians, pendingTickets, repairsToday, recentAlerts);
    }

    public async Task<EjecutivoDashboardStats> GetEjecutivoDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sql = @"
            -- Clientes activos del tenant
            SELECT COUNT(1) FROM dbo.Stores s WHERE s.TenantId = @TenantId AND s.IsActive = 1;
        ";

        var activeClients = await connection.QuerySingleAsync<int>(new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: ct));

        return new EjecutivoDashboardStats(activeClients);
    }

    public async Task<List<TechnicianWorkloadDto>> GetTechnicianWorkloadsAsync(Guid tenantId, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT
                u.Id,
                u.Name + ' ' + u.LastName AS FullName,
                u.Email,
                u.IsActive,
                (SELECT COUNT(*) FROM dbo.TechSupportRequests t
                 WHERE t.TechnicianId = u.Id AND t.Status NOT IN ('Completado', 'Cancelado')) AS ActiveTickets
            FROM dbo.Users u
            WHERE u.TenantId = @tenantId AND u.Role = 4 AND u.IsDeleted = 0
            ORDER BY u.Name";

        var result = await connection.QueryAsync<TechnicianWorkloadDto>(new CommandDefinition(sql, new { tenantId }, cancellationToken: ct));
        return result.ToList();
    }

    public async Task<ClientDashboardStats> GetClientDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sql = @"
            -- Datos del usuario
            SELECT (Name + ' ' + LastName) AS FullName, Email, StoreId FROM dbo.Users WHERE Id=@UserId AND TenantId=@TenantId AND IsDeleted = 0;

            -- Tech requests
            SELECT Id, FaultType, Status, ScheduledDate
            FROM dbo.TechSupportRequests
            WHERE UserId=@UserId AND TenantId=@TenantId
            ORDER BY ScheduledDate DESC;
        ";

        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, new { UserId = userId, TenantId = tenantId }, cancellationToken: ct));

        var user = await multi.ReadFirstOrDefaultAsync<dynamic>();
        var techRequests = (await multi.ReadAsync<TechRequestRecord>()).ToList();

        string fullName = user?.FullName ?? "";
        string email = user?.Email ?? "";
        Guid? storeId = user?.StoreId;
        string tiendaNombre = "";
        string tiendaDireccion = "";
        List<CoolerSummaryRecord> coolers = new();

        if (storeId.HasValue)
        {
            const string storeSql = @"
                SELECT Name, Address FROM dbo.Stores WHERE Id=@Id AND TenantId=@TenantId AND IsActive=1;
                SELECT Id, Model, Status, LastMaintenanceAt AS LastRevisionAt FROM dbo.Coolers WHERE StoreId=@Id;
            ";
            using var storeMulti = await connection.QueryMultipleAsync(new CommandDefinition(storeSql, new { Id = storeId.Value, TenantId = tenantId }, cancellationToken: ct));
            var store = await storeMulti.ReadFirstOrDefaultAsync<dynamic>();
            tiendaNombre = store?.Name ?? "";
            tiendaDireccion = store?.Address ?? "";
            coolers = (await storeMulti.ReadAsync<CoolerSummaryRecord>()).ToList();
        }
        else
        {
            const string coolersSql = @"
                SELECT c.Id, c.Model, c.Status, c.LastMaintenanceAt AS LastRevisionAt
                FROM dbo.Coolers c
                INNER JOIN dbo.Stores s ON c.StoreId=s.Id
                WHERE s.TenantId=@TenantId AND s.IsActive=1";
            coolers = (await connection.QueryAsync<CoolerSummaryRecord>(new CommandDefinition(coolersSql, new { TenantId = tenantId }, cancellationToken: ct))).ToList();
        }

        return new ClientDashboardStats(fullName, email, tiendaNombre, tiendaDireccion, coolers, techRequests);
    }

    public async Task<TechnicianDashboardStats> GetTechnicianDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sqlUser = "SELECT Id, Name, LastName, Email, TenantId FROM dbo.Users WHERE Id = @UserId AND TenantId = @TenantId";
        var user = await connection.QueryFirstOrDefaultAsync<dynamic>(new CommandDefinition(sqlUser, new { UserId = userId, TenantId = tenantId }, cancellationToken: ct));
        if (user == null) throw new UnauthorizedAccessException("Usuario no encontrado");

        const string sqlTasks = @"
            SELECT R.Id, S.Name as StoreName, S.Address, S.District as Commune, R.FaultType as AssistanceType, R.Status, S.Latitude as Lat, S.Longitude as Lng
            FROM dbo.TechSupportRequests R
            INNER JOIN dbo.Coolers C ON R.CoolerId = C.Id
            INNER JOIN dbo.Stores S ON C.StoreId = S.Id
            WHERE R.TenantId = @TenantId AND R.Status != 'Completado'";

        var tasks = (await connection.QueryAsync<TechnicianTaskRecord>(new CommandDefinition(sqlTasks, new { TenantId = tenantId }, cancellationToken: ct))).ToList();

        return new TechnicianDashboardStats(
            user.Id.ToString(),
            (string)user.Name,
            (string)user.LastName,
            (string)user.Email,
            user.TenantId.ToString(),
            tasks
        );
    }

    public async Task<DeliveryDashboardStats> GetDeliveryDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sqlUser = "SELECT Id, Name, LastName, Email, TenantId FROM dbo.Users WHERE Id = @UserId AND TenantId = @TenantId";
        var user = await connection.QueryFirstOrDefaultAsync<dynamic>(new CommandDefinition(sqlUser, new { UserId = userId, TenantId = tenantId }, cancellationToken: ct));
        if (user == null) throw new UnauthorizedAccessException("Usuario no encontrado");

        return new DeliveryDashboardStats(
            user.Id.ToString(),
            (string)user.Name,
            (string)user.LastName,
            (string)user.Email,
            user.TenantId.ToString()
        );
    }

    public async Task<RetailerDashboardStats> GetRetailerDashboardStatsAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sqlUser = @"
            SELECT U.Id, U.Name as Nombre, U.LastName as Apellido, U.Email, U.TenantId, S.Name as StoreName, S.Address as StoreAddress, S.Id as StoreId
            FROM dbo.Users U
            LEFT JOIN dbo.Stores S ON U.StoreId = S.Id
            WHERE U.Id = @UserId AND U.TenantId = @TenantId";

        var userData = await connection.QueryFirstOrDefaultAsync<dynamic>(new CommandDefinition(sqlUser, new { UserId = userId, TenantId = tenantId }, cancellationToken: ct));
        if (userData == null) throw new UnauthorizedAccessException("Usuario no encontrado");

        const string sqlCoolers = @"
            SELECT Id, Model, Status, LastMaintenanceAt, Capacity, Name
            FROM dbo.Coolers
            WHERE StoreId = @StoreId AND TenantId = @TenantId";

        var coolers = (await connection.QueryAsync<RetailerCoolerRecord>(new CommandDefinition(sqlCoolers, new { StoreId = (Guid?)userData.StoreId, TenantId = tenantId }, cancellationToken: ct))).ToList();

        const string sqlTech = @"
            SELECT Id, FaultType, ScheduledDate, Status
            FROM dbo.TechSupportRequests
            WHERE UserId = @UserId AND Status != 'Completado'";

        var tech = (await connection.QueryAsync<RetailerTechRecord>(new CommandDefinition(sqlTech, new { UserId = userId }, cancellationToken: ct))).ToList();

        return new RetailerDashboardStats(
            userData.Id.ToString(),
            (string)userData.Nombre,
            (string)userData.Apellido,
            (string)userData.Email,
            userData.TenantId.ToString(),
            (string)(userData.StoreName ?? "N/A"),
            (string)(userData.StoreAddress ?? "N/A"),
            (Guid?)userData.StoreId,
            coolers,
            tech
        );
    }
}
