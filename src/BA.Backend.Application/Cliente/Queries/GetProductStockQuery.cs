using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace BA.Backend.Application.Cliente.Queries;

public record GetProductStockQuery(Guid ProductId, Guid TenantId) : IRequest<int>;

public class GetProductStockQueryHandler : IRequestHandler<GetProductStockQuery, int>
{
    private readonly IIntegrationFactory _integrationFactory;
    private readonly IProductRepository _productRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMemoryCache _cache;

    public GetProductStockQueryHandler(
        IIntegrationFactory integrationFactory,
        ITenantRepository tenantRepository,
        IProductRepository productRepository,
        IMemoryCache cache)
    {
        _integrationFactory = integrationFactory;
        _tenantRepository = tenantRepository;
        _productRepository = productRepository;
        _cache = cache;
    }

    public async Task<int> Handle(GetProductStockQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"stock_{request.TenantId}_{request.ProductId}";
        
        if (_cache.TryGetValue(cacheKey, out int cachedStock))
        {
            return cachedStock;
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null || string.IsNullOrEmpty(product.ExternalSku))
        {
            return 0;
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null) return 0;

        var adapter = _integrationFactory.Create(tenant);
        var stock = await adapter.GetStockAsync(request.TenantId, product.ExternalSku, cancellationToken);

        // Cachear por 5 minutos para evitar spam de llamadas
        _cache.Set(cacheKey, stock, TimeSpan.FromMinutes(5));

        return stock;
    }
}
