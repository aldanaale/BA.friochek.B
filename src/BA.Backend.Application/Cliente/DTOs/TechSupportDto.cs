using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Cliente.DTOs;

public record TechSupportDto(
    Guid Id,
    Guid CoolerId,
    string FaultType,
    string Description,
    string Status,
    DateTime ScheduledDate,
    DateTime CreatedAt,
    List<string> PhotoUrls
);
