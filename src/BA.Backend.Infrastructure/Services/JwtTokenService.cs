using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Infrastructure.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AppTokenValidationResult = BA.Backend.Application.Common.Interfaces.TokenValidationResult;

namespace BA.Backend.Infrastructure.Services;

public class JwtTokenService(JwtSettings settings) : IJwtTokenService
{
    public (string Token, DateTime ExpiresAt) GenerateToken(User user, Guid tenantId, string sessionId)
    {
        Console.WriteLine("Generando token JWT para el usuario: " + user.Email);
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("session_id", sessionId), // Guardamos el ID de sesion para la sesion unica
            new Claim("tenant_id", tenantId.ToString()), // Muy importante para el multi-tenant
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        Console.WriteLine("Token generado con exito!");
        
        return (tokenString, expiresAt);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(Guid userId, Guid tenantId, string sessionId)
    {
        Console.WriteLine("Generando Refresh Token para el usuario ID: " + userId);
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
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
            issuer: settings.Issuer,
            audience: settings.Audience,
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
            Console.WriteLine("Validando token recibido...");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                Console.WriteLine("El token no es un JWT valido");
                return null;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            var sessionIdClaim = principal.FindFirst("session_id");
            var tenantIdClaim = principal.FindFirst("tenant_id");
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            var roleClaim = principal.FindFirst("role");

            if (userIdClaim == null || sessionIdClaim == null || tenantIdClaim == null)
            {
                Console.WriteLine("Faltan datos criticos adentro del token");
                return null;
            }

            Console.WriteLine("Token validado correctamente para el usuario ID: " + userIdClaim.Value);
            
            return new AppTokenValidationResult
            {
                UserId = Guid.Parse(userIdClaim.Value),
                SessionId = sessionIdClaim.Value,
                TenantId = Guid.Parse(tenantIdClaim.Value),
                Email = emailClaim?.Value,
                Role = roleClaim?.Value ?? "Cliente",
                IssuedAt = jwtToken.IssuedAt,
                ExpiresAt = jwtToken.ValidTo
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al validar el token: " + ex.Message);
            return null;
        }
    }

    public (string Token, DateTime ExpiresAt) GenerateNfcScanToken(string tagId, Guid coolerId, Guid storeId, Guid tenantId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // JWT corto de 10 minutos
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
            issuer: settings.Issuer,
            audience: settings.Audience,
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
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
        catch
        {
            return null;
        }
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
