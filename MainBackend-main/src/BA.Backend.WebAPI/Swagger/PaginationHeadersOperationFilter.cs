using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BA.Backend.WebAPI.Swagger;

public class PaginationHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiPath = context.ApiDescription.RelativePath?.TrimEnd('/').ToLowerInvariant();
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();

        if (apiPath == "api/v1/users" && httpMethod == "GET")
        {
            if (operation.Responses.TryGetValue("200", out var response))
            {
                if (response.Headers == null)
                {
                    response.Headers = new Dictionary<string, OpenApiHeader>();
                }

                response.Headers["X-Total-Count"] = new OpenApiHeader
                {
                    Description = "Número total de usuarios disponibles en el tenant",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                };
                response.Headers["X-Page-Number"] = new OpenApiHeader
                {
                    Description = "Número de página solicitada",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                };
                response.Headers["X-Page-Size"] = new OpenApiHeader
                {
                    Description = "Cantidad de elementos devueltos por página",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                };
                response.Headers["X-Total-Pages"] = new OpenApiHeader
                {
                    Description = "Número total de páginas disponibles",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                };
            }
        }
    }
}
