using BA.Backend.Application.Common.Interfaces;
using MediatR;

namespace BA.Backend.Application.Admin.Commands;

public record SyncCatalogCommand(Guid TenantId) : IRequest<int>;

public class SyncCatalogCommandHandler : IRequestHandler<SyncCatalogCommand, int>
{
    private readonly ICatalogSyncService _syncService;

    public SyncCatalogCommandHandler(ICatalogSyncService syncService)
    {
        _syncService = syncService;
    }

    public async Task<int> Handle(SyncCatalogCommand request, CancellationToken cancellationToken)
    {
        return await _syncService.SyncTenantCatalogAsync(request.TenantId, cancellationToken);
    }
}
