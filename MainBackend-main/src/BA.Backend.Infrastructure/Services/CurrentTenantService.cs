using BA.Backend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BA.Backend.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDeviceFingerprintService _fingerprintService;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor, IDeviceFingerprintService fingerprintService)
    {
        _httpContextAccessor = httpContextAccessor;
        _fingerprintService = fingerprintService;
    }

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) 
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("role");

    public bool IsPlatformAdmin => Role == "PlatformAdmin" || Role == "5";

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? DeviceFingerprint
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var acceptLang = context.Request.Headers["Accept-Language"].ToString();
            
            return _fingerprintService.ComputeFingerprint(userAgent, acceptLang);
        }
    }
}
