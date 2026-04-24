using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Routes.DTOs;

public record RouteDto(
    Guid Id,
    Guid TenantId,
    Guid TransportistaId,
    string TransportistaName,
    DateTime Date,
    string Status,
    DateTime CreatedAt,
    List<RouteStopDto> Stops
);

public record RouteStopDto(
    Guid Id,
    Guid StoreId,
    string StoreName,
    int StopOrder,
    string Status,
    DateTime? ArrivalAt,
    string? Notes
);

public record CreateRouteDto(
    Guid TransportistaId,
    DateTime Date,
    List<CreateRouteStopDto> Stops
);

public record CreateRouteStopDto(
    Guid StoreId,
    int StopOrder
);

public record UpdateRouteStatusDto(string Status);