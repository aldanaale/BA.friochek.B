using Microsoft.AspNetCore.Http;

namespace BA.Backend.WebAPI.DTOs.Cliente;

public record CreateTechSupportRequest(
    string NfcAccessToken,
    string FaultType,
    string Description,
    DateTime ScheduledDate,
    IFormFileCollection? Photos = null
);

public record ReportDamagedTagRequest(Guid CoolerId, string Description);
