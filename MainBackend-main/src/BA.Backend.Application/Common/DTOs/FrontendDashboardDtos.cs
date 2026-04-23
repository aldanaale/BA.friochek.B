using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Common.DTOs;

// --- AUTH ---
public record FrontendAuthResponse(
    string Id,
    string AccessToken,
    string Role, // retailer | technician | admin | delivery
    string TenantId,
    string RedirectTo,
    string ExpiresAt,
    string UserId,
    string UserFullName
);

// --- SHARED USER ---
public record FrontendUserDto(
    string Id,
    string Role,
    string TenantId,
    string Nombre,
    string Apellido,
    string Email,
    string Phone,
    string? Store = null
);

// --- RETAILER (CLIENTE) ---
public record RetailerHomeResponse(
    FrontendUserDto User,
    TiendaDto Tienda,
    List<CoolerFrontendDto> Coolers,
    List<TechRequestFrontendDto> TechRequests
);

public record TiendaDto(string Nombre, string Direccion);

public record CoolerFrontendDto(
    string CoolerId,
    string Model,
    string Status, // operativo | falla | etc
    string LastRevisionAt, // DD-MM-YYYY
    int Capacity,
    string LocationDescription
);

public record TechRequestFrontendDto(
    string RequestId,
    string FaultType,
    string ScheduledDate, // DD-MM-YYYY
    string Status // pending | in_progress | completed
);

// --- TECHNICIAN ---
public record TechnicianHomeResponse(
    FrontendUserDto User,
    List<TaskFrontendDto> Tasks
);

public record TaskFrontendDto(
    string Id,
    string Store,
    string Address,
    string Commune,
    string AssistanceType,
    string Status, // pending | completed
    double Lat,
    double Lng
);

// --- ADMIN ---
public record AdminHomeResponse(
    FrontendUserDto User
);

// --- DELIVERY (TRANSPORTISTA) ---
public record DeliveryHomeResponse(
    FrontendUserDto User
);
