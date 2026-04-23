using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Users.DTOs;
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

public record GetMyOrdersQuery(Guid UserId, Guid TenantId, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResultDto<ClientOrderSummaryDto>>;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PagedResultDto<ClientOrderSummaryDto>>
{
    private readonly string _connectionString;

    public GetMyOrdersQueryHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<PagedResultDto<ClientOrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken ct)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        
        var offset = (request.PageNumber - 1) * request.PageSize;

        const string countSql = "SELECT COUNT(*) FROM dbo.Orders WHERE UserId = @UserId AND TenantId = @TenantId";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { request.UserId, request.TenantId });

        const string sql = @"
            SELECT 
                Id AS OrderId, Status, Total, CreatedAt, DispatchDate
            FROM dbo.Orders 
            WHERE UserId = @UserId AND TenantId = @TenantId
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var orders = await connection.QueryAsync<ClientOrderSummaryDto>(sql, new { 
            request.UserId, 
            request.TenantId, 
            Offset = offset, 
            PageSize = request.PageSize 
        });

        return new PagedResultDto<ClientOrderSummaryDto>
        {
            Items = orders.ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
