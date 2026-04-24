using BA.Backend.Application;
using BA.Backend.Infrastructure;
using BA.Backend.WebAPI.Middleware;
using BA.Backend.WebAPI.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.RateLimiting;
using BA.Backend.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console;
using BA.Backend.Infrastructure.Services;

// Configuración inicial de Serilog (Bootstrap)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

try
{
    // Verificación proactiva de puerto (Evita stack traces gigantes de Kestrel)
    try 
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, 5003);
        listener.Start();
        listener.Stop();
    }
    catch (System.Net.Sockets.SocketException)
    {
        AnsiConsole.Write(new Panel("[bold red]ERROR DE ARRANQUE: Puerto 5003 ocupado[/]\n[grey]Por favor, cierra la otra instancia de la API antes de iniciar una nueva.[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red));
        return; // Salida limpia
    }

    AnsiConsole.Write(new Rule("[bold white]CORE BACKEND SERVICES[/]").RuleStyle("grey30").LeftJustified());
    AnsiConsole.WriteLine();

    Log.Information("Iniciando host de BA.FrioCheck API...");

    var builder = WebApplication.CreateBuilder(args);

    // Tema de Colores Personalizado - Alta Intensidad y Contraste
    var customTheme = new SystemConsoleTheme(
        new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
        {
            [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
            [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Magenta },
            [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
            [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
            [ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
            [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
            [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
            [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Green },
            [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
            [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Red },
            [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Red },
            [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
        });

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            theme: customTheme));

    builder.WebHost.UseUrls("http://0.0.0.0:5003");

    // Logger para uso durante la configuración
    var logger = Log.ForContext<Program>();
    
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                   ?? builder.Configuration["Jwt:SecretKey"];

    if (string.IsNullOrWhiteSpace(secretKey) && builder.Environment.IsDevelopment())
    {
        // Solo en desarrollo permitimos una llave por defecto si no hay nada configurado
        secretKey = "BA.FrioCheck.Development.Secret.Key.32.Chars.Min";
        Log.Information("JWT_SECRET_KEY no configurada. Usando clave de desarrollo (Modo Seguro/Dev).");
    }

    if (string.IsNullOrWhiteSpace(secretKey))
    {
        Log.Fatal("JWT_SECRET_KEY is not set. Application cannot start. Please set the environment variable or 'Jwt:SecretKey' in configuration.");
        throw new InvalidOperationException("JWT_SECRET_KEY is required");
    }

    if (secretKey.Length < 32)
    {
        Log.Fatal("JWT_SECRET_KEY must be at least 32 characters long for security");
        throw new InvalidOperationException("JWT_SECRET_KEY must be at least 32 characters");
    }

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    if (allowedOrigins.Length == 0)
    {
        Log.Information("No CORS origins configured in settings. Using inclusive default policy (AllowAll) for connectivity.");
    }

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = ApiResponse<object>.FailureResponse(errors);
            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BA.FrioCheck API",
        Version = "v1",
        Description = "API de gestión de cadena de frío — BA.FrioCheck (Antigravity) · Puerto 5003"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    var appXmlFile = "BA.Backend.Application.xml";
    var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
    if (File.Exists(appXmlPath))
    {
        options.IncludeXmlComments(appXmlPath);
    }

    // Filtros de operación
    options.OperationFilter<RoleOperationFilter>();

    // Enriquece schemas de enums con descripción legible de valores
    options.SchemaFilter<EnumSchemaFilter>();

    // Orden y descripción de secciones (sin esto Swagger ordena alfabéticamente)
    options.DocumentFilter<TagOrderDocumentFilter>();

    // Tags con descripción de cada área de dominio
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = """
            **Cómo autenticarse:**

            1. Ejecuta `POST /api/v1/auth/login` con tu `email`, `password` y `tenantSlug`
            2. Copia el valor de `data.accessToken` de la respuesta
            3. Pégalo aquí tal cual — Swagger agrega el prefijo `Bearer ` automáticamente

            El token expira en 60 minutos. Si recibes **401 Unauthorized**, repite el proceso.
            """
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });


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
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if (allowedOrigins is { Length: > 0 })
        {
            policy.SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                var host = uri.Host;
                // Permitir localhost siempre
                if (host == "localhost" || host == "127.0.0.1")
                    return true;
                // Permitir cualquier IP en la red local 192.168.100.*
                if (host.StartsWith("192.168.100."))
                    return true;
                // Verificar contra la lista configurada en appsettings
                return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

            Log.Information("CORS: FrontendPolicy activa — localhost + red 192.168.100.* + {Count} origins fijos", allowedOrigins.Length);
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            
            Log.Warning("CORS: No origins configured. Falling back to 'AllowAll' because environment is Development.");
        }
        else
        {
            // In Production, if no origins are set, we restrict
            Log.Fatal("CORS: No origins configured in Production. Frontend connectivity will be RESTRICTED.");
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // Si la petición es para el Hub, extraemos el token del Query String
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && 
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.FailureResponse("No autorizado o token inválido.");
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                }));
            }
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// [0] HUB DE INTEGRACIÓN UNIVERSAL (STOCK & ORDERS)
builder.Services.AddScoped<BA.Backend.Application.Common.Interfaces.IIntegrationFactory, BA.Backend.Infrastructure.Services.IntegrationFactory>();
builder.Services.AddScoped<BA.Backend.Infrastructure.Services.Integrations.MockIntegrationAdapter>();
builder.Services.AddScoped<BA.Backend.Infrastructure.Services.Integrations.SavoryIntegrationAdapter>();
builder.Services.AddScoped<BA.Backend.Application.Common.Interfaces.ICatalogSyncService, BA.Backend.Infrastructure.Services.CatalogSyncService>();
builder.Services.AddHostedService<BA.Backend.Infrastructure.Services.CatalogSyncWorker>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();

// PlatformAdmin bypasses all [Authorize(Roles = "...")] checks globally
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    BA.Backend.WebAPI.Authorization.PlatformAdminAuthorizationHandler>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Política estricta para Login
    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5; // Más estricto: 5 intentos por minuto
        opt.QueueLimit = 0;
    });

    // Política general para el resto de la API
    options.AddFixedWindowLimiter("GlobalPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100; // 100 requests por minuto por IP (aproximado)
        opt.QueueLimit = 20;
    });
});

// Silenciar los avisos de validación de modelos de EF Core (Query Filter Warnings)
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.Error);

var app = builder.Build();

    var useRealDb = builder.Configuration.GetValue<bool>("ConnectionStrings:UseRealDatabase");
    if (useRealDb)
    {
        try 
        {
            // Ejecutamos el sembrado sin el spinner de Spectre para evitar conflictos 
            // con los logs detallados que genera DbInitializer
            using (var scope = app.Services.CreateScope())
            {
                await BA.Backend.Infrastructure.Data.DbInitializer.SeedAsync(app.Services);
            }
            
            AnsiConsole.MarkupLine("[bold green]✓ Base de Datos:[/] Estructura y sembrado verificado correctamente.");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[bold red]⚠ ADVERTENCIA:[/] No se pudo sincronizar la base de datos. La aplicación continuará pero algunas funciones pueden fallar.");
            Log.Warning(ex, "Fallo en la sincronización de base de datos durante el arranque.");
        }
    }
    else
    {
        Log.Information("Saltando inicialización de base de datos (Modo Simulación / UseRealDatabase=false).");
    }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BA.FrioCheck API v1");
        c.RoutePrefix = "swagger";
    });
}

// [0.5] COMPATIBILIDAD DE RUTAS: Habilitar prefijo /api/v1/ de forma retrocompatible
app.UsePathBase("/api/v1");

// [1] SEGURIDAD: Siempre inyectar cabeceras de seguridad para pasar [SEC06]
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline'; connect-src 'self' ws: wss:;");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    await next();
});

app.UseStaticFiles();

// [2] CORS
app.UseCors("FrontendPolicy");

// [3] RATE LIMITING
app.UseRateLimiter();

// [0] COMPATIBILIDAD EXTERNA (Flat JSON)
app.UseMiddleware<BA.Backend.WebAPI.Middleware.FlatResponseMiddleware>();

// [3] EXCEPCIONES GLOBAL
app.UseMiddleware<GlobalExceptionHandler>();

// [4] AUTH & AUTHORIZATION
app.UseAuthentication();
app.UseMiddleware<LogContextMiddleware>();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds + "ms"
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
});

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var port = 5003;
        var ip = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())
            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

        // Tabla de Conectividad Premium
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[u]TIPO[/]").Centered())
            .AddColumn(new TableColumn("[u]DIRECCIÓN / INSTANCIA[/]").LeftAligned());

        table.AddRow("[green]STATUS[/]", "[bold white]API BA.FrioCheck — ¡LISTA Y CORRIENDO![/]");
        table.AddRow("Localhost", $"[blue]http://localhost:{port}[/]");
        table.AddRow("Network IP", $"[blue]http://{ip}:{port}[/]");
        table.AddRow("Swagger UI", $"[yellow]http://localhost:{port}/swagger[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    });

    app.Run();
}
catch (Exception ex)
{
    // Manejo elegante para errores comunes de desarrollo (puerto ocupado)
    if (ex is System.IO.IOException && ex.Message.Contains("address already in use", StringComparison.OrdinalIgnoreCase))
    {
        AnsiConsole.Write(new Panel("[bold red]ERROR: El puerto 5003 ya está en uso.[/]\n[grey]Por favor, cierra la otra instancia de la API antes de iniciar una nueva.[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red));
    }
    else if (ex.InnerException is System.Net.Sockets.SocketException socketEx && socketEx.NativeErrorCode == 10048)
    {
        AnsiConsole.Write(new Panel("[bold red]ERROR: Conflicto de socket (Puerto 5003).[/]\n[grey]La dirección ya está siendo utilizada por otro proceso.[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red));
    }
    else
    {
        // Para errores desconocidos, mantenemos el reporte detallado pero con estilo Spectre
        AnsiConsole.Write(new Rule("[red]ERROR CRÍTICO[/]").RuleStyle("red"));
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    }
}
finally
{
    Log.CloseAndFlush();
}
