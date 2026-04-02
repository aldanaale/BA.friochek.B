using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Cliente.Queries;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Handlers;

public class GetClientHomeQueryHandler : IRequestHandler<GetClientHomeQuery, ClientHomeDto>
{
    private readonly string _connectionString;

    public GetClientHomeQueryHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    private sealed class OrderRow
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = null!;
        public int Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DispatchDate { get; set; }
        public Guid CoolerId { get; set; }
        public Guid? NfcTagId { get; set; }
    }

    public async Task<ClientHomeDto> Handle(GetClientHomeQuery request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);

        var response = new ClientHomeDto();

        const string userSql = @"
            SELECT u.FullName, u.Email, u.StoreId
            FROM dbo.Users u
            WHERE u.Id = @UserId AND u.TenantId = @TenantId";

        var userResult = await connection.QueryFirstOrDefaultAsync(userSql, new { request.UserId, request.TenantId });
        var fullName = userResult?.FullName as string ?? string.Empty;
        var email = userResult?.Email as string ?? string.Empty;
        var storeId = userResult?.StoreId as Guid?;

        response.UserFullName = fullName;
        response.User.Email = email;

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var names = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            response.User.Nombre = names.Length > 0 ? names[0] : string.Empty;
            response.User.Apellido = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;
        }

        string coolerSql;
        object coolerParams;

        if (storeId.HasValue)
        {
            const string storeSql = @"
                SELECT Name, Address
                FROM dbo.Stores
                WHERE Id = @StoreId AND TenantId = @TenantId AND IsActive = 1";

            var storeResult = await connection.QueryFirstOrDefaultAsync(storeSql, new { StoreId = storeId.Value, request.TenantId });
            response.Tienda.Nombre = storeResult?.Name as string ?? string.Empty;
            response.Tienda.Direccion = storeResult?.Address as string ?? string.Empty;

            coolerSql = @"
                SELECT Id AS CoolerId,
                       Model,
                       Status,
                       LastMaintenanceAt AS LastRevisionAt
                FROM dbo.Coolers
                WHERE StoreId = @StoreId";
            coolerParams = new { StoreId = storeId.Value };
        }
        else
        {
            response.Tienda.Nombre = string.Empty;
            response.Tienda.Direccion = string.Empty;

            coolerSql = @"
                SELECT c.Id AS CoolerId,
                       c.Model,
                       c.Status,
                       c.LastMaintenanceAt AS LastRevisionAt
                FROM dbo.Coolers c
                INNER JOIN dbo.Stores s ON c.StoreId = s.Id
                WHERE s.TenantId = @TenantId AND s.IsActive = 1";
            coolerParams = new { request.TenantId };
        }

        var coolers = (await connection.QueryAsync<CoolerDto>(coolerSql, coolerParams)).ToList();
        response.Coolers = coolers;
        response.TotalCoolers = coolers.Count;
        response.OperationalCoolers = coolers.Count(c => c.Status == "Operativo");
        response.FaultyCoolers = coolers.Count(c => c.Status != "Operativo");

        const string ordersSql = @"
            SELECT 
                Id AS OrderId, 
                Status, 
                Total, 
                CreatedAt,
                DispatchDate,
                CoolerId,
                NfcTagId
            FROM dbo.Orders 
            WHERE UserId = @UserId AND TenantId = @TenantId AND Status != 'Pagado'
            ORDER BY CreatedAt DESC";

        var orderResults = await connection.QueryAsync<OrderRow>(ordersSql, new { request.UserId, request.TenantId });
        response.ActiveOrders = orderResults.Select(o => new HomeOrderDto
        {
            OrderId = o.OrderId,
            Status = o.Status,
            Title = o.Status switch
            {
                "PorPagar" => "Pedido pendiente de pago",
                "Entregado" => "Pedido entregado",
                _ => $"Pedido {o.OrderId.ToString().Substring(0, 8)}"
            },
            Description = o.NfcTagId.HasValue && o.NfcTagId.Value != Guid.Empty
                ? $"Cooler {o.CoolerId} - Tag {o.NfcTagId.Value}"
                : $"Cooler {o.CoolerId}",
            CreatedAt = o.CreatedAt,
            DispatchDate = o.DispatchDate,
            IsInProgress = o.Status != "Entregado"
        }).ToList();
        response.Orders = response.ActiveOrders;

        response.CurrentOrdersCount = response.ActiveOrders.Count;

        const string techRequestsSql = @"
            SELECT 
                Id AS RequestId, 
                FaultType, 
                Status, 
                ScheduledDate 
            FROM dbo.TechSupportRequests 
            WHERE UserId = @UserId AND TenantId = @TenantId
            ORDER BY ScheduledDate DESC";

        try
        {
            var techRequests = await connection.QueryAsync<TechRequestDto>(techRequestsSql, new { request.UserId, request.TenantId });
            response.TechRequests = techRequests.ToList();
            response.OpenAssistanceCount = response.TechRequests.Count(r => r.Status != "Completado");
        }
        catch (SqlException)
        {
            response.TechRequests = new List<TechRequestDto>();
            response.OpenAssistanceCount = 0;
        }

        response.SupportPhone = "+56 9 1234 5678";
        response.SupportEmail = "soporte@ba-backend.com";

        return response;
    }
}
