using BA.Backend.Application.Common.DTOs;

namespace BA.Backend.Application.Cliente.DTOs;

public class ClientHomeDto
{
    /// <summary>Usar UserSummaryDto de Common.DTOs en lugar de la clase local UserDto.</summary>
    public UserSummaryDto User { get; set; } = new();
    public TiendaDto Tienda { get; set; } = new();

    public string UserFullName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;

    public int TotalCoolers { get; set; }
    public int OperationalCoolers { get; set; }
    public int FaultyCoolers { get; set; }

    public List<CoolerDto> Coolers { get; set; } = new();
    public List<HomeOrderDto> ActiveOrders { get; set; } = new();
    public List<HomeOrderDto> Orders { get; set; } = new();
    public int CurrentOrdersCount { get; set; }
    public int OpenAssistanceCount { get; set; }
    public List<TechRequestDto> TechRequests { get; set; } = new();

    public string SupportPhone { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
}

// UserDto eliminada — usar BA.Backend.Application.Common.DTOs.UserSummaryDto

public class TiendaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

public class CoolerDto
{
    public Guid CoolerId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastRevisionAt { get; set; }
}

public class HomeOrderDto
{
    public Guid OrderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DispatchDate { get; set; }
    public bool IsInProgress { get; set; }
}

public class TechRequestDto
{
    public Guid RequestId { get; set; }
    public string FaultType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendiente";
    public DateTime ScheduledDate { get; set; }
}
