using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Transportista.DTOs;

/// <summary>
/// DTO especializado para la hoja de ruta del transportista.
/// </summary>
public record TransportistaRouteDto(
    Guid RouteStopId,
    Guid OrderId,
    string StoreName,
    string StoreAddress,
    string StoreCity,
    int OrderTotal,
    int ItemsCount,
    string Status, 
    DateTime OrderDate
);
