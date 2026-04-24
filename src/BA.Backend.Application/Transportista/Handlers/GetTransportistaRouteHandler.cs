using MediatR;
using BA.Backend.Application.Transportista.Queries;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Transportista.Handlers;

public class GetRouteHandler : IRequestHandler<GetRouteQuery, List<TransportistaRouteDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetRouteHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TransportistaRouteDto>> Handle(GetRouteQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetPendingRouteStopsAsync(request.TransportistaId, request.TenantId);
    }
}

