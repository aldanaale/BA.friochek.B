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

public record GetProductsQuery(Guid TenantId) : IRequest<IEnumerable<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly string _connectionString;

    public GetProductsQueryHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT 
                Id, 
                Name, 
                Type, 
                Price, 
                Stock 
            FROM dbo.Products 
            WHERE TenantId = @TenantId AND IsActive = 1
            ORDER BY Name";

        var products = await connection.QueryAsync<ProductDto>(sql, new { request.TenantId });
        return products;
    }
}
