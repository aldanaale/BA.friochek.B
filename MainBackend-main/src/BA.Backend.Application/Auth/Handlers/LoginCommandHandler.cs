using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Application.Exceptions;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;

namespace BA.Backend.Application.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ISessionService _sessionService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IAuthRepository authRepository,
        IUserSessionRepository userSessionRepository,
        ISessionService sessionService,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        ILogger<LoginCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _authRepository = authRepository;
        _userSessionRepository = userSessionRepository;
        _sessionService = sessionService;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Determinar el Tenant Slug (Opcional) ─────────────────────
        var slugToUse = request.TenantSlug;

        if (string.IsNullOrWhiteSpace(slugToUse))
        {
            _logger.LogInformation("Login sin slug: Se buscará la empresa automáticamente por email ({Email})", request.Email);
        }

        // ── 2. CARGA ULTRA-CONSOLIDADA (Single Round-Trip vía AuthRepository) ───────
        var loginData = await _authRepository.GetLoginDataAsync(request.Email, slugToUse, cancellationToken);

        var user = loginData.User;
        var tenant = loginData.Tenant;
        
        if (user is null || tenant is null)
        {
            _logger.LogWarning("Login fallido: Credenciales inválidas para {Email} (Empresa: {Slug})", request.Email, slugToUse ?? "Auto-detect");
            throw new InvalidCredentialsException("Credenciales inválidas o empresa no encontrada");
        }

        var activeSessions = loginData.ActiveSessions;

        // ── 3. Validar Seguridad (Contraseña y Estado) ────────────────────────────
        if (!user.IsActive)
            throw new InvalidCredentialsException("La cuenta no está disponible");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login fallido para {Email}: contraseña incorrecta", request.Email);
            throw new InvalidCredentialsException("Contraseña incorrecta");
        }

        // ── 4. Manejo de sesión única ────────────────────────────────────────
        var existingDevice = activeSessions.FirstOrDefault(s => s.DeviceId == request.DeviceFingerprint);
        bool sessionReplaced = false;

        if (existingDevice is null && activeSessions.Any())
        {
            var previousSession = activeSessions.First();
            await _userSessionRepository.InvalidateSessionAsync(previousSession.Id, "Nueva sesión en otro dispositivo");
            sessionReplaced = true;
        }

        // ── 5. Generar token y Gestionar Sesión (Paralelo) ────────────────────
        var sessionId = Guid.NewGuid().ToString();
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, tenant.Id, sessionId);

        var userSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = tenant.Id,
            DeviceId = request.DeviceFingerprint,
            DeviceFingerprint = request.DeviceFingerprint,
            AccessToken = token,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Ejecución en paralelo
        var dbTask = _userSessionRepository.CreateSessionAsync(userSession);
        var serviceTask = _sessionService.RegisterSessionAsync(sessionId, user.Id, expiresAt, cancellationToken);
        await Task.WhenAll(dbTask, serviceTask);

        // ── 6. Carga de datos de Cliente (solo datos básicos, historial se carga en Dashboard) ──────
        ClienteDataDto? clienteData = null;
        if (user.Role == UserRole.Cliente && loginData.Store != null)
        {
            var store = new StoreDataDto
            {
                Id = loginData.Store.Id,
                Name = loginData.Store.Name,
                Address = loginData.Store.Address,
                ContactName = loginData.Store.ContactName,
                ContactPhone = loginData.Store.ContactPhone,
                Coolers = loginData.Coolers.Select(c => new CoolerDataDto
                {
                    Id = c.Id,
                    SerialNumber = c.SerialNumber,
                    Model = c.Model,
                    Capacity = c.Capacity,
                    Status = c.Status,
                    InstalledAt = c.CreatedAt,
                    LastRevisionAt = c.LastMaintenanceAt
                }).ToList()
            };

            clienteData = new ClienteDataDto { Store = store, Tickets = new() };
        }

        _logger.LogInformation("Login ULTRA-OPTIMIZADO exitoso para {Email}", user.Email);

        return new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            UserFullName = user.FullName,
            Role = user.Role.ToString(),
            UserId = user.Id,
            TenantId = tenant.Id,
            SessionReplaced = sessionReplaced,
            RedirectTo = GetRedirectUrl(user.Role.ToString()),
            ClienteData = clienteData
        };
    }
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GetRedirectUrl(string role) => role switch
    {
        "Admin" => "/admin/dashboard",
        "PlatformAdmin" => "/admin/dashboard",
        "Cliente" => "/cliente/dashboard",
        "Transportista" => "/transportista/dashboard",
        "Tecnico" => "/tecnico/dashboard",
        "Supervisor" => "/supervisor/dashboard",
        "EjecutivoComercial" => "/ejecutivo/dashboard",
        _ => "/dashboard"
    };
}
