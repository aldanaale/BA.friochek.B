using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BA.Backend.Application.Transportista.Queries;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Cliente.Commands;
using BA.Backend.Application.Cliente.DTOs;
using BA.Backend.WebAPI.DTOs.Cliente;
using BA.Backend.Application.Common.DTOs;
using BA.Backend.Application.Common.Queries;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("delivery")]
[Authorize(Roles = "Transportista")]
[Tags("Transporte")]
public class TransportistaController(
    IMediator mediator,
    ILogger<TransportistaController> logger,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{

    /// <summary>
    /// Dashboard Home - Delivery
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryHomeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeliveryHomeResponse>>> GetHome(CancellationToken ct)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetDeliveryDashboardQuery(userId, tenantId);
        var result = await mediator.Send(query, ct);

        return Ok(ApiResponse<DeliveryHomeResponse>.SuccessResponse(result));
    }


    /// <summary>
    /// Obtiene la hoja de ruta asignada al transportista autenticado.
    /// </summary>
    [HttpGet("route")]
    [ProducesResponseType(typeof(ApiResponse<List<TransportistaRouteDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TransportistaRouteDto>>>> GetMyRoute()
    {
        var transportistaId = GetUserId();
        var tenantId = GetTenantId();

        logger.LogInformation("Obteniendo hoja de ruta para Transportista {TransportistaId}", transportistaId);

        var query = new GetRouteQuery(transportistaId, tenantId);
        var result = await mediator.Send(query);

        return Ok(ApiResponse<List<TransportistaRouteDto>>.SuccessResponse(result));
    }

    [HttpPost("delivery")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> RecordDelivery([FromBody] RecordDeliveryRequest req)
    {
        logger.LogInformation("Registrando entrega para Parada {RouteStopId}", req.RouteStopId);
 
        var command = new DeliveryCommand(
            req.RouteStopId, 
            req.NfcAccessToken, 
            req.Latitude, 
            req.Longitude, 
            req.SignatureBase64);
            
        await mediator.Send(command);

        return Ok(ApiResponse<object>.SuccessResponse(null, "Entrega registrada exitosamente."));
    }

    /// <summary>
    /// Reporta una merma de producto o cooler durante la entrega.
    /// </summary>
    [HttpPost("merma")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Guid>>> RecordMerma([FromForm] RecordMermaRequest req)
    {
        var tenantId = GetTenantId();
        var transportistaId = GetUserId();

        logger.LogInformation("Reportando merma en Cooler {CoolerId} por Transportista {TransportistaId}", req.CoolerId, transportistaId);

        var command = new MermaCommand(
            tenantId,
            transportistaId,
            req.CoolerId,
            req.ProductId,
            req.ProductName,
            req.Quantity,
            req.Reason,
            req.Description ?? string.Empty,
            req.Photo,
            req.NfcAccessToken,
            req.Latitude,
            req.Longitude
        );

        var id = await mediator.Send(command);
        return Ok(ApiResponse<Guid>.SuccessResponse(id, "Merma reportada con éxito."));
    }

    /// <summary>
    /// Crea un ticket de soporte técnico (ej. cooler descompuesto) durante la ruta.
    /// </summary>
    [HttpPost("tech-support")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTechSupport([FromForm] CreateTechSupportRequest req)
    {
        var tenantId = GetTenantId();
        var transportistaId = GetUserId();

        logger.LogInformation("Creando Ticket de Soporte por Transportista {TransportistaId}", transportistaId);

        var command = new CreateTechSupportCommand(
            req.NfcAccessToken,
            req.FaultType,
            req.Description,
            req.ScheduledDate,
            req.Photos,
            transportistaId,
            tenantId
        );

        var id = await mediator.Send(command);
        return Ok(ApiResponse<Guid>.SuccessResponse(id, "Ticket de soporte creado con éxito."));
    }

    /// <summary>
    /// Descarga el certificado de entrega legal en formato PDF.
    /// </summary>
    [HttpGet("delivery/{id:guid}/certificate.pdf")]
    [AllowAnonymous] // Permitimos descarga para clientes con el enlace, o se puede proteger según requerimiento.
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadDeliveryCertificatePdf(Guid id, CancellationToken ct)
    {
        var query = new GetDeliveryCertificatePdfQuery(id);
        var pdfBytes = await mediator.Send(query, ct);
        
        return File(pdfBytes, "application/pdf", $"Certificado_Entrega_{id}.pdf");
    }
}
