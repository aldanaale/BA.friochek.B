using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace BA.Backend.WebAPI.Authorization;

/// <summary>
/// Grants PlatformAdmin users access to any role-protected endpoint automatically.
/// Registered as a singleton so it intercepts every RolesAuthorizationRequirement
/// before ASP.NET Core evaluates the [Authorize(Roles = "...")] attribute.
/// </summary>
public class PlatformAdminAuthorizationHandler
    : AuthorizationHandler<RolesAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RolesAuthorizationRequirement requirement)
    {
        if (context.User.IsInRole("PlatformAdmin"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
