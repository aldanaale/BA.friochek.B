using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PingController : ControllerBase
{
    /// <summary>
    /// Verifica que el backend está vivo y responde correctamente.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Online",
            Message = "Backend FCHEK está respondiendo correctamente",
            Timestamp = DateTime.UtcNow,
            MachineName = Environment.MachineName
        });
    }
}