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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Queries;

public record GetMyTechRequestsQuery(
    Guid UserId, 
    Guid TenantId, 
    int PageNumber = 1, 
    int PageSize = 10
) : IRequest<PagedResultDto<TechSupportDto>>;

public class GetMyTechRequestsQueryHandler : IRequestHandler<GetMyTechRequestsQuery, PagedResultDto<TechSupportDto>>
{
    private readonly string _connectionString;

    public GetMyTechRequestsQueryHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<PagedResultDto<TechSupportDto>> Handle(GetMyTechRequestsQuery request, CancellationToken ct)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        
        var offset = (request.PageNumber - 1) * request.PageSize;

        const string countSql = "SELECT COUNT(*) FROM dbo.TechSupportRequests WHERE UserId = @UserId AND TenantId = @TenantId";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { request.UserId, request.TenantId });

        const string sql = @"
            SELECT 
                Id, CoolerId, FaultType, Description, Status, ScheduledDate, CreatedAt, PhotoUrls
            FROM dbo.TechSupportRequests 
            WHERE UserId = @UserId AND TenantId = @TenantId
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var dbResults = await connection.QueryAsync(sql, new { 
            request.UserId, 
            request.TenantId, 
            Offset = offset, 
            PageSize = request.PageSize 
        });

        var items = dbResults.Select(r => new TechSupportDto(
            r.Id,
            r.CoolerId,
            r.FaultType,
            r.Description,
            r.Status,
            r.ScheduledDate,
            r.CreatedAt,
            JsonSerializer.Deserialize<List<string>>(r.PhotoUrls ?? "[]") ?? new List<string>()
        )).ToList();

        return new PagedResultDto<TechSupportDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
