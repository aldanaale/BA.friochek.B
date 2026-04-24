namespace BA.Backend.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    string? UserId { get; }
    string? Role { get; }
    bool IsPlatformAdmin { get; }
    string? IpAddress { get; }
    string? DeviceFingerprint { get; }
}
