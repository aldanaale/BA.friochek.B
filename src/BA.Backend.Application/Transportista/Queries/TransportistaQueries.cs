using MediatR;
using BA.Backend.Application.Transportista.DTOs;

namespace BA.Backend.Application.Transportista.Queries;

public record GetDailyRouteQuery(
    Guid TransportistId,
    DateTime RouteDate) : IRequest<List<RouteStopDto>>;

public record GetMachineByNfcTagQuery(
    string NfcTagId,
    Guid TransportistId) : IRequest<MachineDetailDto>;

public record GetPendingDeliveriesByRouteQuery(
    Guid TransportistId,
    DateTime RouteDate) : IRequest<List<RouteStopDto>>;

public record GetMachineMovementHistoryQuery(
    Guid MachineId,
    DateTime? FromDate,
    DateTime? ToDate,
    MovementType? MovementType,
    int PageNumber,
    int PageSize) : IRequest<List<MovementSummaryDto>>;

public record GetPendingTicketsByRouteQuery(
    Guid TransportistId,
    DateTime RouteDate) : IRequest<List<SupportTicketResultDto>>;
