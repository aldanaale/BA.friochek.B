
using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BA.Backend.Application.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ISessionService _sessionService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IUserSessionRepository userSessionRepository,
        ISessionService sessionService,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _userSessionRepository = userSessionRepository;
        _sessionService = sessionService;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant is null)
        {
            throw new System.Security.Authentication.InvalidCredentialException("Credenciales inválidas");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, tenant.Id, cancellationToken);
        if (user is null)
        {
            throw new System.Security.Authentication.InvalidCredentialException("Credenciales inválidas");
        }

        if (!user.IsActive)
        {
            throw new System.Security.Authentication.InvalidCredentialException("La cuenta no está disponible");
        }

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            throw new System.Security.Authentication.InvalidCredentialException("Credenciales inválidas");
        }

        var activeSessions = await _userSessionRepository.GetActiveSessionsByUserAsync(user.Id);
        var existingDeviceSession = activeSessions.FirstOrDefault(s => s.DeviceId == request.DeviceFingerprint);
        
        bool sessionReplaced = false;
        if (existingDeviceSession is null && activeSessions.Any())
        {
            var previousSession = activeSessions.First();
            await _userSessionRepository.InvalidateSessionAsync(previousSession.Id, "Se inició una nueva sesión en otro dispositivo");
            sessionReplaced = true;
        }

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
            JwtToken = token,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userSessionRepository.CreateSessionAsync(userSession);
        await _sessionService.RegisterSessionAsync(sessionId, user.Id, expiresAt, cancellationToken);

        var redirectTo = GetRedirectUrl(user.Role.ToString());

        ClienteDataDto? clienteData = null;

        if (user.Role.ToString() == "Cliente")
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");

            using IDbConnection connection = new SqlConnection(connectionString);

            StoreDataDto? store = null;
            if (user.StoreId.HasValue)
            {
                const string storeSql = @"
                    SELECT Id, Name, Address, ContactName, ContactPhone
                    FROM dbo.Stores
                    WHERE Id = @StoreId AND TenantId = @TenantId AND IsActive = 1";
                
                store = await connection.QueryFirstOrDefaultAsync<StoreDataDto>(storeSql, 
                    new { StoreId = user.StoreId.Value, TenantId = tenant.Id });

                if (store != null)
                {
                    const string coolersSql = @"
                        SELECT Id, SerialNumber, Model, Capacity, Status, CreatedAt AS InstalledAt, LastMaintenanceAt AS LastRevisionAt
                        FROM dbo.Coolers
                        WHERE StoreId = @StoreId";
                    
                    var coolers = await connection.QueryAsync<CoolerDataDto>(coolersSql, 
                        new { StoreId = store.Id });
                    store = store with { Coolers = coolers.ToList() };
                }
            }

            const string ticketsSql = @"
                SELECT Id, CoolerId, FaultType, Description, Status, ScheduledDate, CreatedAt
                FROM dbo.TechSupportRequests
                WHERE UserId = @UserId AND TenantId = @TenantId
                ORDER BY CreatedAt DESC";
                
            var tickets = new List<TicketDataDto>();
            try 
            {
                var queryTickets = await connection.QueryAsync<TicketDataDto>(ticketsSql, new { UserId = user.Id, TenantId = tenant.Id });
                tickets = queryTickets.ToList();
            }
            catch (SqlException) { /* Ignorar si la tabla aún no existe */ }

            const string ordersSql = @"
                SELECT Id, CoolerId, NfcTagId, Status, Total, DispatchDate, CreatedAt
                FROM dbo.Orders
                WHERE UserId = @UserId AND TenantId = @TenantId AND Status != 'Pagado'
                ORDER BY CreatedAt DESC";

            var activeOrders = new List<OrderDataDto>();
            try 
            {
                var queryOrders = await connection.QueryAsync<OrderDataDto>(ordersSql, new { UserId = user.Id, TenantId = tenant.Id });
                activeOrders = queryOrders.ToList();
            }
            catch (SqlException) { /* Ignorar si la tabla no existe */ }

            clienteData = new ClienteDataDto(store, tickets, activeOrders);
        }

        return new LoginResponseDto(
            AccessToken: token,
            ExpiresAt: expiresAt,
            UserFullName: user.FullName,
            Role: user.Role.ToString(),
            UserId: user.Id,
            TenantId: tenant.Id,
            SessionReplaced: sessionReplaced,
            RedirectTo: redirectTo,
            ClienteData: clienteData
        );
    }

    private static string GetRedirectUrl(string role) => role switch
    {
        "Admin" => "/admin/dashboard",
        "Cliente" => "/cliente/dashboard",
        "Transportista" => "/transportista/panel",
        "Tecnico" => "/tecnico/panel",
        _ => "/dashboard"
    };
}
