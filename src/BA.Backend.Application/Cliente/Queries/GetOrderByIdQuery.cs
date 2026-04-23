using BA.Backend.Application.Cliente.DTOs;
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

namespace BA.Backend.Application.Cliente.Queries;

public record GetOrderByIdQuery(Guid OrderId, Guid TenantId) : IRequest<ClientOrderDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ClientOrderDto?>
{
    private readonly string _connectionString;

    public GetOrderByIdQueryHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<ClientOrderDto?> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        
        const string orderSql = @"
            SELECT 
                Id, Status, Total, CreatedAt, DispatchDate
            FROM dbo.Orders 
            WHERE Id = @OrderId AND TenantId = @TenantId";

        var order = await connection.QueryFirstOrDefaultAsync<ClientOrderDto>(orderSql, new { request.OrderId, request.TenantId });
        
        if (order != null)
        {
            const string itemsSql = @"
                SELECT 
                    Id, ProductId, ProductName, Quantity, UnitPrice, Subtotal
                FROM dbo.OrderItems 
                WHERE OrderId = @OrderId";

            var items = await connection.QueryAsync<ClientOrderItemDto>(itemsSql, new { request.OrderId });
            return order with { Items = items.ToList() };
        }

        return null;
    }
}
