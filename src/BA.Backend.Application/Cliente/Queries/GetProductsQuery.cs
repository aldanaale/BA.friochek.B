using BA.Backend.Domain.Entities;
using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Cliente.Queries;

public record GetProductsQuery(Guid TenantId) : IRequest<IEnumerable<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IIntegrationFactory _integrationFactory;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ITenantRepository tenantRepository,
        IIntegrationFactory integrationFactory,
        ILogger<GetProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _tenantRepository = tenantRepository;
        _integrationFactory = integrationFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        // 1. Obtener datos base del catálogo local
        var products = await _productRepository.GetAllAsync(request.TenantId, ct);
        
        var tenant = await _tenantRepository.GetActiveByIdAsync(request.TenantId, ct);
        if (tenant == null) return Enumerable.Empty<ProductDto>();

        var integration = _integrationFactory.Create(tenant);

        // 3. Consultar Stock Externo
        try 
        {
            var productTasks = products.Select(async p => 
            {
                var sku = p.ExternalSku ?? "N/A";
                var stock = await integration.GetStockAsync(request.TenantId, sku, ct);
                
                return new ProductDto(
                    p.Id,
                    p.Name,
                    "", // Description placeholder
                    p.Type,
                    p.Price,
                    p.IsActive,
                    p.ExternalSku,
                    stock
                );
            });

            return await Task.WhenAll(productTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener catálogo con stock externo para Tenant {TenantId}", request.TenantId);
            throw;
        }
    }
}
