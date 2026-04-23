using BA.Backend.Application.Cliente.Commands;
using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.WebAPI.DTOs.Nfc;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("nfc")]
[Authorize]
[Tags("Nfc")]
public class NfcController(IMediator mediator, ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Valida un TAG NFC y devuelve la información del cooler asociado.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Request:
    /// { "nfcUid": "04:AB:CD:EF:12:34:56" }
    /// 
    /// Ejemplo de Respuesta 200:
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "coolerId": "a1b2c3d4-0020-0000-0000-000000000000",
    ///     "coolerModel": "Whirlpool WRT318FZDM",
    ///     "serialNumber": "WHR-2024-00123",
    ///     "status": "Activo",
    ///     "storeId": "a1b2c3d4-0010-0000-0000-000000000000",
    ///     "storeName": "Tienda Centro",
    ///     "nfcAccessToken": "nfc_tok_eyJhbGciOiJIUzI1NiJ9.example"
    ///   },
    ///   "message": null
    /// }
    /// </remarks>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<NfcValidationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NfcValidationResultDto>>> Validate([FromBody] ValidateNfcRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            var command = new ValidateNfcCommand(request.NfcUid, tenantId);
            var result = await mediator.Send(command);
            return Ok(ApiResponse<NfcValidationResultDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.FailureResponse("Tag NFC no encontrado", "NFC_NOT_FOUND"));
        }
    }

    /// <summary>
    /// Asocia un TAG NFC a un cooler (Enrolamiento).
    /// </summary>
    /// <remarks>
    /// Ejemplo de Request:
    /// {
    ///   "nfcUid": "04:AB:CD:EF:12:34:57",
    ///   "coolerId": "a1b2c3d4-0020-0000-0000-000000000000"
    /// }
    /// </remarks>
    [HttpPost("enroll")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<string>>> Enroll([FromBody] EnrollNfcRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            var command = new EnrollNfcCommand(request.NfcUid, request.CoolerId, tenantId);
            var tagId = await mediator.Send(command);
            return Ok(ApiResponse<string>.SuccessResponse(tagId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

}
