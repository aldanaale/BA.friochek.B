using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AppTokenValidationResult = BA.Backend.Application.Common.Interfaces.TokenValidationResult;

namespace BA.Backend.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(JwtSettings settings, ILogger<JwtTokenService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user, Guid tenantId, string sessionId)
    {
        _logger.LogDebug("Generating JWT token for user {UserId}", user.Id);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("session_id", sessionId),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogDebug("JWT token generated successfully for user {UserId}", user.Id);

        return (tokenString, expiresAt);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(Guid userId, Guid tenantId, string sessionId)
    {
        _logger.LogDebug("Generating refresh token for user {UserId}", userId);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddDays(7);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("session_id", sessionId),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("is_refresh_token", "true"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public AppTokenValidationResult? ValidateToken(string token)
    {
        try
        {
            _logger.LogDebug("Validating JWT token");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                _logger.LogWarning("Token validation failed: not a valid JWT");
                return null;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            var sessionIdClaim = principal.FindFirst("session_id");
            var tenantIdClaim = principal.FindFirst("tenant_id");
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            var roleClaim = principal.FindFirst("role");

            if (userIdClaim == null || sessionIdClaim == null || tenantIdClaim == null)
            {
                _logger.LogWarning("Token validation failed: missing required claims");
                return null;
            }

            _logger.LogDebug("Token validated successfully for user {UserId}", userIdClaim.Value);

            return new AppTokenValidationResult
            {
                UserId = Guid.Parse(userIdClaim.Value),
                SessionId = sessionIdClaim.Value,
                TenantId = Guid.Parse(tenantIdClaim.Value),
                Email = emailClaim?.Value,
                Role = roleClaim?.Value ?? string.Empty,
                IssuedAt = jwtToken.IssuedAt,
                ExpiresAt = jwtToken.ValidTo
            };
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
            return null;
        }
    }

    public (string Token, DateTime ExpiresAt) GenerateNfcScanToken(string tagId, Guid coolerId, Guid storeId, Guid tenantId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var claims = new[]
        {
            new Claim("nfc_tag", tagId),
            new Claim("cooler_id", coolerId.ToString()),
            new Claim("store_id", storeId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("is_nfc_token", "true"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public NfcTokenValidationResult? ValidateNfcToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken) return null;

            var isNfcToken = principal.FindFirst("is_nfc_token")?.Value;
            if (isNfcToken != "true") return null;

            return new NfcTokenValidationResult
            {
                TagId = principal.FindFirst("nfc_tag")?.Value ?? string.Empty,
                CoolerId = Guid.Parse(principal.FindFirst("cooler_id")?.Value ?? Guid.Empty.ToString()),
                StoreId = Guid.Parse(principal.FindFirst("store_id")?.Value ?? Guid.Empty.ToString()),
                TenantId = Guid.Parse(principal.FindFirst("tenant_id")?.Value ?? Guid.Empty.ToString()),
                ExpiresAt = jwtToken.ValidTo
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NFC token validation failed");
            return null;
        }
    }

    public (string Token, DateTime ExpiresAt) GenerateCredentialToken(string userId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role),
            new Claim("credential_type", "dynamic_qr"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public Guid? ExtractUserId(string token)
    {
        var result = ValidateToken(token);
        return result?.UserId;
    }

    public string? ExtractSessionId(string token)
    {
        var result = ValidateToken(token);
        return result?.SessionId;
    }
}
