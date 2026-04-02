using MediatR;
using BA.Backend.Application.Transportista.Queries;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Application.Exceptions;

namespace BA.Backend.Application.Transportista.Handlers;

internal sealed class GetDailyRouteQueryHandler : IRequestHandler<GetDailyRouteQuery, List<RouteStopDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetDailyRouteQueryHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RouteStopDto>> Handle(GetDailyRouteQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetDailyRouteAsync(request.TransportistId, request.RouteDate);
    }
}

internal sealed class GetMachineByNfcTagQueryHandler : IRequestHandler<GetMachineByNfcTagQuery, MachineDetailDto>
{
    private readonly ITransportistaRepository _repository;
    private readonly INfcValidationService _nfcService;

    public GetMachineByNfcTagQueryHandler(ITransportistaRepository repository, INfcValidationService nfcService)
    {
        _repository = repository;
        _nfcService = nfcService;
    }

    public async Task<MachineDetailDto> Handle(GetMachineByNfcTagQuery request, CancellationToken cancellationToken)
    {
        bool isRegistered = await _nfcService.IsTagRegisteredAsync(request.NfcTagId);
        if (!isRegistered)
        {
            throw new UserNotFoundExeption($"Tag NFC {request.NfcTagId} no registrado.");
        }

        return await _repository.GetMachineByNfcTagAsync(request.NfcTagId);
    }
}

internal sealed class GetPendingDeliveriesByRouteQueryHandler : IRequestHandler<GetPendingDeliveriesByRouteQuery, List<RouteStopDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetPendingDeliveriesByRouteQueryHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RouteStopDto>> Handle(GetPendingDeliveriesByRouteQuery request, CancellationToken cancellationToken)
    {
        var route = await _repository.GetDailyRouteAsync(request.TransportistId, request.RouteDate);
        return route.Where(r => r.HasPendingDelivery).ToList();
    }
}

internal sealed class GetMachineMovementHistoryQueryHandler : IRequestHandler<GetMachineMovementHistoryQuery, List<MovementSummaryDto>>
{
    private readonly ITransportistaRepository _repository;

    public GetMachineMovementHistoryQueryHandler(ITransportistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MovementSummaryDto>> Handle(GetMachineMovementHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetMachineHistoryAsync(
            request.MachineId, 
            request.FromDate, 
            request.ToDate, 
            request.MovementType, 
            request.PageNumber, 
            request.PageSize);
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
        return await _repository.GetPendingTicketsByRouteAsync(request.TransportistId, request.RouteDate);
    }
}
