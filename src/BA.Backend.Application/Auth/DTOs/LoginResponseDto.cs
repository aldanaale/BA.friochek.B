namespace BA.Backend.Application.Auth.DTOs;

public record LoginResponseDto
{
    public string AccessToken { get; init; } = null!;

    public DateTime ExpiresAt { get; init; }

    public string UserFullName { get; init; } = null!;

    public string Role { get; init; } = null!;

    public Guid UserId { get; init; }

    public Guid TenantId { get; init; }

    public bool SessionReplaced { get; init; }

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
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Address { get; init; } = null!;

    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }

    public List<CoolerDataDto> Coolers { get; init; } = new();
}

public record CoolerDataDto
{
    public Guid Id { get; init; }

    public string SerialNumber { get; init; } = null!;

    public string Model { get; init; } = null!;

    public int Capacity { get; init; }

    public string Status { get; init; } = null!;

    public DateTime? InstalledAt { get; init; }
    public DateTime? LastRevisionAt { get; init; }
}

public record TicketDataDto
{
    public Guid Id { get; init; }

    public Guid CoolerId { get; init; }

    public string FaultType { get; init; } = null!;

    public string Description { get; init; } = null!;

    public string Status { get; init; } = null!;

    public DateTime ScheduledDate { get; init; }

    public DateTime CreatedAt { get; init; }
}

