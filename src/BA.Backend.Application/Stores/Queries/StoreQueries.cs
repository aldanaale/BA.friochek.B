using BA.Backend.Application.Stores.DTOs;
using MediatR;

namespace BA.Backend.Application.Stores.Queries;

public record GetAllStoresQuery(
    Guid TenantId
) : IRequest<IEnumerable<StoreDto>>;

public record GetStoreByIdQuery(
    Guid Id,
    Guid TenantId
) : IRequest<StoreDto?>;
