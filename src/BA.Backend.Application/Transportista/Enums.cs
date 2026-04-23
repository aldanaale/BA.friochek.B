namespace BA.Backend.Application.Transportista;

public enum MachineStatus
{
    Active,
    Inactive,
    UnderMaintenance,
    Faulty
}

public enum MovementType
{
    Delivery,
    WastePickup,
    SupportTicket,
    Maintenance
}

public enum TicketCategory
{
    ElectricalFailure,
    AestheticDamage,
    TemperatureIssue,
    RefrigerantLeak,
    ScreenDamage,
    Other
}

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}
