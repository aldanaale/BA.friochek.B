using MediatR;
using BA.Backend.Application.Transportista.Queries;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Exceptions;

namespace BA.Backend.Application.Transportista.Handlers;

internal sealed class GetDailyRouteQueryHandler : IRequestHandler<GetDailyRouteQuery, List<TransportistaRouteStopDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetDailyRouteQueryHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TransportistaRouteStopDto>> Handle(GetDailyRouteQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetDailyRouteAsync(request.TransportistaId, request.RouteDate, request.TenantId);
    }
}

internal sealed class GetCoolerByNfcTagQueryHandler : IRequestHandler<GetCoolerByNfcTagQuery, CoolerDetailDto>
{
    private readonly ITransportistaRepository _repository;
    private readonly INfcValidationService _nfcService;

    public GetCoolerByNfcTagQueryHandler(ITransportistaRepository repository, INfcValidationService nfcService)
    {
        _repository = repository;
        _nfcService = nfcService;
    }

    public async Task<CoolerDetailDto> Handle(GetCoolerByNfcTagQuery request, CancellationToken cancellationToken)
    {
        bool isRegistered = await _nfcService.IsTagRegisteredAsync(request.NfcTagId);
        if (!isRegistered)
        {
            throw new UserNotFoundException($"Tag NFC {request.NfcTagId} no registrado.");
        }

        return await _repository.GetCoolerByNfcTagAsync(request.NfcTagId);
    }
}

internal sealed class GetPendingDeliveriesByRouteQueryHandler : IRequestHandler<GetPendingDeliveriesByRouteQuery, List<TransportistaRouteStopDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetPendingDeliveriesByRouteQueryHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TransportistaRouteStopDto>> Handle(GetPendingDeliveriesByRouteQuery request, CancellationToken cancellationToken)
    {
        var route = await _repository.GetDailyRouteAsync(request.TransportistaId, request.RouteDate, request.TenantId);
        return route.Where(r => r.HasPendingDelivery).ToList();
    }
}

internal sealed class GetCoolerMovementHistoryQueryHandler : IRequestHandler<GetCoolerMovementHistoryQuery, List<MovementSummaryDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetCoolerMovementHistoryQueryHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MovementSummaryDto>> Handle(GetCoolerMovementHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetCoolerHistoryAsync(
            request.CoolerId,
            request.FromDate,
            request.ToDate,
            request.MovementType,
            request.PageNumber <= 0 ? 1 : request.PageNumber,
            request.PageSize <= 0 ? 10 : request.PageSize);

    }
}

internal sealed class GetPendingTicketsByRouteQueryHandler : IRequestHandler<GetPendingTicketsByRouteQuery, List<SupportTicketResultDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetPendingTicketsByRouteQueryHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SupportTicketResultDto>> Handle(GetPendingTicketsByRouteQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetPendingTicketsByRouteAsync(request.TransportistaId, request.RouteDate);
    }
}
