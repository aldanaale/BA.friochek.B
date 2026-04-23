using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BA.Backend.WebAPI.Swagger;

public class RoleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;
        var controllerType = methodInfo.DeclaringType;

        var allowAnonymous = (controllerType?.GetCustomAttributes<AllowAnonymousAttribute>(true).Any() ?? false)
            || methodInfo.GetCustomAttributes<AllowAnonymousAttribute>(true).Any();

        var authorizeAttributes = new List<AuthorizeAttribute>();
        if (controllerType != null)
        {
            authorizeAttributes.AddRange(controllerType.GetCustomAttributes<AuthorizeAttribute>(true));
        }

        authorizeAttributes.AddRange(methodInfo.GetCustomAttributes<AuthorizeAttribute>(true));

        var roleText = "Público / No requiere autenticación";
        if (!allowAnonymous && authorizeAttributes.Any())
        {
            var roles = authorizeAttributes
                .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
                .Select(a => a.Roles!)
                .Distinct()
                .ToList();

            roleText = roles.Any()
                ? string.Join(", ", roles)
                : "Cualquier usuario autenticado";
        }

        var purpose = operation.Summary?.Trim();
        if (string.IsNullOrEmpty(purpose))
        {
            purpose = context.ApiDescription.RelativePath is not null
                ? $"Endpoint `{context.ApiDescription.RelativePath}`"
                : "Acción del API";
        }

        var note = $"**Rol:** {roleText}\n\n**Para:** {purpose}";

        if (string.IsNullOrEmpty(operation.Description))
        {
            operation.Description = note;
        }
        else if (!operation.Description.Contains("**Rol:**"))
        {
            operation.Description = $"{operation.Description}\n\n{note}";
        }
    }
}
