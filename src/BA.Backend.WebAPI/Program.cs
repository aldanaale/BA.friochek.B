
using BA.Backend.Application;
using BA.Backend.Infrastructure;
using BA.Backend.WebAPI.Middleware;
using BA.Backend.WebAPI.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5003");

Console.WriteLine("Configurando la seguridad JWT...");
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    options.OperationFilter<RoleOperationFilter>();
    options.CustomSchemaIds(type =>
    {
        var schemaId = type.FullName ?? type.Name;
        return schemaId
            .Replace("[", "_")
            .Replace("]", "")
            .Replace("`", "_")
            .Replace(",", "")
            .Replace(" ", "")
            .Replace("+", ".");
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

Console.WriteLine("Registrando servicios de las capas del proyecto...");
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

var useRealDb = builder.Configuration.GetValue<bool>("ConnectionStrings:UseRealDatabase");
if (useRealDb)
{
    Console.WriteLine("Iniciando la base de datos y el sembrado de datos...");
    using (var scope = app.Services.CreateScope())
    {
        await BA.Backend.Infrastructure.Data.DbInitializer.SeedAsync(app.Services);
    }
}
else
{
    Console.WriteLine("Saltando inicialización de base de datos (Modo Simulación).");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionHandler>();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SessionValidationMiddleware>();

app.MapControllers();

Console.WriteLine("¡La API esta lista y corriendo!");
app.Run();

internal class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sessionId = context.User.FindFirst("session_id")?.Value;
            if (!string.IsNullOrEmpty(sessionId))
            {
                var sessionService = context.RequestServices.GetRequiredService<BA.Backend.Application.Common.Interfaces.ISessionService>();
                var isValid = await sessionService.IsSessionValidAsync(sessionId, context.RequestAborted);
                
                if (!isValid)
                {
                    Console.WriteLine("Sesion invalidada por el middleware (Sesion Unica)");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }
        }

        await _next(context);
    }
}
