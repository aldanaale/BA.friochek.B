using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

/// <summary>
/// Controlador base que centraliza helpers comunes:
/// GetTenantId(), GetUserId() y OkResponse() para evitar
/// duplicacion en cada controller.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly ICurrentTenantService CurrentTenant;

    protected BaseApiController(IMediator mediator, ICurrentTenantService currentTenant)
    {
        Mediator = mediator;
        CurrentTenant = currentTenant;
    }

    /// <summary>Retorna el TenantId del JWT. Lanza UnauthorizedAccessException si no existe.</summary>
    protected Guid GetTenantId()
    {
        if (CurrentTenant.IsPlatformAdmin)
            return Guid.Empty;
        return CurrentTenant.TenantId
               ?? throw new UnauthorizedAccessException("Tenant no identificado en el token");
    }

    /// <summary>Retorna el UserId del JWT. Lanza UnauthorizedAccessException si no existe.</summary>
    protected Guid GetUserId()
        => Guid.Parse(CurrentTenant.UserId
           ?? throw new UnauthorizedAccessException("Usuario no autenticado"));

    /// <summary>Envia un command/query por MediatR y envuelve el resultado en ApiResponse.</summary>
    protected async Task<ActionResult<ApiResponse<T>>> Send<T>(
        IRequest<T> request, CancellationToken ct)
    {
        var result = await Mediator.Send(request, ct);
        return Ok(ApiResponse<T>.SuccessResponse(result));
    }
}
