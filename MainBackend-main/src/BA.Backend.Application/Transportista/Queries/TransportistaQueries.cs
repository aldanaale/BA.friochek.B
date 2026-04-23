using MediatR;
using BA.Backend.Application.Transportista.DTOs;

namespace BA.Backend.Application.Transportista.Queries;

public record GetDailyRouteQuery(
    Guid TransportistaId,
    DateTime RouteDate,
    Guid TenantId) : IRequest<List<TransportistaRouteStopDto>>;

public record GetCoolerByNfcTagQuery(
    string NfcTagId,
    Guid TransportistaId) : IRequest<CoolerDetailDto>;

public record GetPendingDeliveriesByRouteQuery(
    Guid TransportistaId,
    DateTime RouteDate,
    Guid TenantId) : IRequest<List<TransportistaRouteStopDto>>;

public record GetCoolerMovementHistoryQuery(
    Guid CoolerId,
    DateTime? FromDate,
    DateTime? ToDate,
    MovementType? MovementType,
    int PageNumber,
    int PageSize) : IRequest<List<MovementSummaryDto>>;

public record GetPendingTicketsByRouteQuery(
    Guid TransportistaId,
    DateTime RouteDate,
    Guid TenantId) : IRequest<List<SupportTicketResultDto>>;
