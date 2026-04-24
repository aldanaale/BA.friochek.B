using BA.Backend.Application.Users.Commands;
using BA.Backend.Application.Users.DTOs;
using BA.Backend.Application.Users.Queries;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("users")]
[Authorize]
[Tags("Users")]
public class UsersController(
    IMediator mediator,
    ILogger<UsersController> logger,
    ICurrentTenantService currentTenantService) : BaseApiController(mediator, currentTenantService)
{


    /// <summary>
    /// Obtiene todos los usuarios del tenant (paginado).
    /// </summary>
    /// <remarks>
    /// Ejemplo de Respuesta 200:
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "items": [
    ///       {
    ///         "id": "a1b2c3d4-0002-0000-0000-000000000000",
    ///         "fullName": "Carlos Administrador",
    ///         "email": "admin@friocheck.com",
    ///         "role": "Admin",
    ///         "isActive": true
    ///       }
    ///     ],
    ///     "totalCount": 15,
    ///     "totalPages": 2,
    ///     "pageNumber": 1
    ///   },
    ///   "message": null
    /// }
    /// </remarks>
    [HttpGet]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<UserDto>>>> GetAllUsers(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        logger.LogInformation("Cargando la lista de usuarios para el tenant: {TenantId}", tenantId);

        var query = new GetAllUsersQuery(tenantId, pageNumber, pageSize);
        var result = await mediator.Send(query, cancellationToken);

        return Ok(ApiResponse<PagedResultDto<UserDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Obtiene un usuario específico por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var query = new GetUserByIdQuery(id, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// Ejemplo de Request:
    /// {
    ///   "email": "nuevo.usuario@friocheck.com",
    ///   "fullName": "Pedro Gómez",
    ///   "password": "Password123!",
    ///   "role": 3
    /// }
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
        [FromBody] CreateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        logger.LogInformation("Creando un nuevo usuario: {Email}", dto.Email);

        try
        {
            var command = new CreateUserCommand(
                dto.Email,
                dto.FullName,
                dto.Password,
                (Domain.Enums.UserRole)dto.Role,
                tenantId
            );

            var result = await mediator.Send(command, cancellationToken);
            logger.LogInformation("Usuario creado correctamente con ID: {UserId}", result.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, ApiResponse<UserDto>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error al crear usuario: {Message}", ex.Message);
            return Conflict(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Actualiza la información de un usuario existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        try
        {
            var command = new UpdateUserCommand(
                id,
                dto.FullName,
                (Domain.Enums.UserRole)dto.Role,
                dto.IsActive,
                tenantId
            );

            var result = await mediator.Send(command, cancellationToken);
            return Ok(ApiResponse<UserDto>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Elimina de forma lógica (soft delete) un usuario del sistema.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        try
        {
            var command = new DeleteUserCommand(id, tenantId);
            await mediator.Send(command, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Usuario eliminado correctamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Bloquea un usuario, impidiendo su acceso al sistema.
    /// </summary>
    /// <remarks>
    /// URL ej: /api/v1/users/a1b2c3d4-0002-0000-0000-000000000000/lock
    /// </remarks>
    [HttpPost("{id:guid}/lock")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> LockUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        try
        {
            var command = new LockUserCommand(id, tenantId);
            await mediator.Send(command, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Usuario bloqueado correctamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Desbloquea un usuario previamente bloqueado.
    /// </summary>
    /// <remarks>
    /// URL ej: /api/v1/users/a1b2c3d4-0002-0000-0000-000000000000/unlock
    /// </remarks>
    [HttpPost("{id:guid}/unlock")]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UnlockUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        try
        {
            var command = new UnlockUserCommand(id, tenantId);
            await mediator.Send(command, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Usuario desbloqueado correctamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Genera y devuelve la credencial digital (código QR en base64) para el usuario autenticado.
    /// Válido por 15 minutos.
    /// </summary>
    [HttpGet("credential")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GetCredential(CancellationToken cancellationToken = default)
    {
        var query = new BA.Backend.Application.Common.Queries.GetMyCredentialQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<string>.SuccessResponse(result, "Credencial generada con éxito"));
    }
}
