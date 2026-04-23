using BA.Backend.Application.Cliente.Commands;
using BA.Backend.Application.Cliente.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/v1/nfc")]
[Authorize]
public class NfcController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Valida un TAG NFC y devuelve la información del cooler asociado
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(NfcValidationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NfcValidationResultDto>> Validate([FromBody] ValidateNfcRequest request)
    {
        try
        {
            var tenantId = GetTenantIdFromClaims();
            var command = new ValidateNfcCommand(request.NfcUid, tenantId);
            var result = await mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "NFC_NOT_FOUND" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "NFC_NOT_ACTIVE")
        {
            return BadRequest(new { code = "NFC_NOT_ACTIVE" });
        }
    }

    /// <summary>
    /// Asocia un TAG NFC a un cooler y marca el dispositivo como enrolado
    /// </summary>
    [HttpPost("enroll")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Enroll([FromBody] EnrollNfcRequest request)
    {
        try
        {
            var tenantId = GetTenantIdFromClaims();
            var command = new EnrollNfcCommand(request.NfcUid, request.CoolerId, tenantId);
            var tagId = await mediator.Send(command);
            return Ok(tagId);
        }
        catch (KeyNotFoundException ex) when (ex.Message == "COOLER_NOT_FOUND")
        {
            return NotFound(new { code = "COOLER_NOT_FOUND" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "NFC_ALREADY_ENROLLED")
        {
            return BadRequest(new { code = "NFC_ALREADY_ENROLLED" });
        }
    }

    private Guid GetTenantIdFromClaims()
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            throw new UnauthorizedAccessException("TenantId no encontrado o inválido en el JWT");

        return tenantId;
    }
}

public record ValidateNfcRequest(string NfcUid);
public record EnrollNfcRequest(string NfcUid, Guid CoolerId);
