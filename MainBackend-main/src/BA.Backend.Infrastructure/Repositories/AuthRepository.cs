using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Models;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Settings;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BA.Backend.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly string _connectionString;

    public AuthRepository(DatabaseSettings settings)
    {
        _connectionString = settings.DefaultConnection;
    }

    public async Task<LoginResult> GetLoginDataAsync(string email, string? tenantSlug, CancellationToken ct)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);

        const string megaSql = @"
            DECLARE @Uid UNIQUEIDENTIFIER;
            DECLARE @Tid UNIQUEIDENTIFIER;
            DECLARE @Sid UNIQUEIDENTIFIER;

            -- 1. Buscar Usuario y Tenant IDs
            IF @Slug IS NULL OR @Slug = ''
            BEGIN
                -- Búsqueda global por Email (Seleccionamos el primer usuario activo encontrado)
                SELECT TOP 1 @Uid = u.Id, @Tid = u.TenantId, @Sid = u.StoreId
                FROM dbo.Users u
                JOIN dbo.Tenants t ON u.TenantId = t.Id
                WHERE u.Email = @Email AND u.IsDeleted = 0 AND t.IsActive = 1;
            END
            ELSE
            BEGIN
                -- Búsqueda específica por Email y Empresa
                SELECT @Uid = u.Id, @Tid = t.Id, @Sid = u.StoreId
                FROM dbo.Users u
                JOIN dbo.Tenants t ON u.TenantId = t.Id
                WHERE u.Email = @Email AND t.Slug = @Slug AND u.IsDeleted = 0 AND t.IsActive = 1;
            END

            -- 2. Retornar User
            SELECT * FROM dbo.Users WHERE Id = @Uid;

            -- 3. Retornar Tenant
            SELECT * FROM dbo.Tenants WHERE Id = @Tid;

            -- 4. Retornar Sesiones Activas
            SELECT * FROM dbo.UserSessions WHERE UserId = @Uid AND IsActive = 1;

            -- 5. Datos de Cliente
            IF @Sid IS NOT NULL
            BEGIN
                -- Store
                SELECT * FROM dbo.Stores WHERE Id = @Sid AND IsActive = 1;
                
                -- Coolers
                SELECT * FROM dbo.Coolers WHERE StoreId = @Sid AND IsDeleted = 0;
            END";

        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(megaSql, new { Email = email, Slug = tenantSlug }, cancellationToken: ct));

        var result = new LoginResult();
        result.User = await multi.ReadFirstOrDefaultAsync<User>();
        result.Tenant = await multi.ReadFirstOrDefaultAsync<Tenant>();
        result.ActiveSessions = (await multi.ReadAsync<UserSession>()).ToList();

        if (result.User?.StoreId != null && !multi.IsConsumed)
        {
            result.Store = await multi.ReadFirstOrDefaultAsync<Store>();
            result.Coolers = (await multi.ReadAsync<Cooler>()).ToList();
        }

        return result;
    }
}
