namespace BA.Backend.Application.Auth.DTOs;

public record LoginResponseDto
{
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string AccessToken { get; init; } = null!;

    /// <example>2026-04-10T15:30:00Z</example>
    public DateTime ExpiresAt { get; init; }

    /// <example>Roberto Admin</example>
    public string UserFullName { get; init; } = null!;

    /// <example>Admin</example>
    public string Role { get; init; } = null!;

    /// <example>f47ac10b-58cc-4372-a567-0e02b2c3d479</example>
    public Guid UserId { get; init; }

    /// <example>99999999-9999-9999-9999-999999999991</example>
    public Guid TenantId { get; init; }

    /// <example>false</example>
    public bool SessionReplaced { get; init; }

    /// <example>/admin/dashboard</example>
    public string RedirectTo { get; init; } = null!;

    public ClienteDataDto? ClienteData { get; init; }
}

public record ClienteDataDto
{
    public StoreDataDto? Store { get; init; }
    public List<TicketDataDto> Tickets { get; init; } = new();
}

public record StoreDataDto
{
    /// <example>0c7044d3-90b2-4988-bd11-c36f85167b5e</example>
    public Guid Id { get; init; }

    /// <example>Tienda Savory Central</example>
    public string Name { get; init; } = null!;

    /// <example>Av. Providencia 1234, Santiago</example>
    public string Address { get; init; } = null!;

    /// <example>Diego Tecnico</example>
    public string? ContactName { get; init; }

    /// <example>+56912345678</example>
    public string? ContactPhone { get; init; }

    public List<CoolerDataDto> Coolers { get; init; } = new();
}

public record CoolerDataDto
{
    /// <example>a2b3c4d5-e6f7-4a5b-9c8d-7e6f5a4b3c2d</example>
    public Guid Id { get; init; }

    /// <example>SN-SAV-001</example>
    public string SerialNumber { get; init; } = null!;

    /// <example>Vista-2000</example>
    public string Model { get; init; } = null!;

    /// <example>500</example>
    public int Capacity { get; init; }

    /// <example>Activo</example>
    public string Status { get; init; } = null!;

    /// <example>2025-01-15T10:00:00Z</example>
    public DateTime? InstalledAt { get; init; }

    /// <example>2026-03-01T14:30:00Z</example>
    public DateTime? LastRevisionAt { get; init; }
}

public record TicketDataDto
{
    /// <example>e1f2g3h4-i5j6-4k7l-8m9n-0o1p2q3r4s5t</example>
    public Guid Id { get; init; }

    /// <example>a2b3c4d5-e6f7-4a5b-9c8d-7e6f5a4b3c2d</example>
    public Guid CoolerId { get; init; }

    /// <example>Mantenimiento Preventivo</example>
    public string FaultType { get; init; } = null!;

    /// <example>Revisión trimestral de compresor</example>
    public string Description { get; init; } = null!;

    /// <example>Programado</example>
    public string Status { get; init; } = null!;

    /// <example>2026-04-15T09:00:00Z</example>
    public DateTime ScheduledDate { get; init; }

    /// <example>2026-04-01T08:00:00Z</example>
    public DateTime CreatedAt { get; init; }
}

