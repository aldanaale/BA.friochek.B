using System;
using System.Collections.Generic;

namespace BA.Backend.Application.Supervisor.DTOs;

public class SupervisorDashboardDto
{
    public int ActiveTechnicians { get; set; }
    public int PendingTickets { get; set; }
    public int RepairsToday { get; set; }
    public List<RecentAlertDto> RecentAlerts { get; set; } = new();
    public List<TechnicianStatusDto> TechnicianStatus { get; set; } = new();
}

public class RecentAlertDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., "Critical Failure", "Delay"
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Severity { get; set; } = "Medium"; // Low, Medium, High
}

public class TechnicianStatusDto
{
    public Guid TechnicianId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty; // "Online", "In Route", "Working", "Offline"
    public int AssignedTickets { get; set; }
    public int CompletedToday { get; set; }
}
