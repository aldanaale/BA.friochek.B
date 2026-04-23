using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Application.Auth.Handlers;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var slugToUse = request.TenantSlug;
        Domain.Entities.User? user = null;
        Domain.Entities.Tenant? tenant = null;

        if (string.IsNullOrWhiteSpace(slugToUse))
        {
            _logger.LogInformation("ForgotPassword sin slug: Buscando usuario globalmente por email ({Email})", request.Email);
            user = await _userRepository.GetGlobalByEmailAsync(request.Email, cancellationToken);
            
            if (user != null)
            {
                tenant = await _tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);
            }
        }
        else
        {
            // Búsqueda tradicional por Slug + Email
            tenant = await _tenantRepository.GetBySlugAsync(slugToUse, cancellationToken);
            if (tenant != null && tenant.IsActive)
            {
                user = await _userRepository.GetByEmailAsync(request.Email, tenant.Id, cancellationToken);
            }
        }

        if (tenant is null || !tenant.IsActive || user is null)
        {
            _logger.LogWarning("ForgotPassword: No se encontró usuario activo {Email} (Empresa: {Slug})", 
                request.Email, slugToUse ?? "Auto-detect");
            return new ForgotPasswordResponseDto();
        }

        var resetToken = Guid.NewGuid().ToString("N");
        var tokenHash = _passwordHasher.Hash(resetToken);

        var passwordResetToken = new Domain.Entities.PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _tokenRepository.AddAsync(passwordResetToken, cancellationToken);

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/auth/reset-password?token={resetToken}&email={user.Email}&slug={tenant.Slug}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                resetLink,
                user.FullName,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email de recuperacion a {Email}", user.Email);
        }

        return new ForgotPasswordResponseDto();
    }
}
