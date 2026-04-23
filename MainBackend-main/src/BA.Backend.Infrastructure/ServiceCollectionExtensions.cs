using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using BA.Backend.Infrastructure.Repositories;
using BA.Backend.Infrastructure.Services;
using BA.Backend.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Application.Tecnico.Interfaces;

namespace BA.Backend.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection("Jwt").Bind(jwtSettings);

        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? configuration["Jwt:SecretKey"]
            ?? "BA.FrioCheck.Development.Secret.Key.32.Chars.Min";

        jwtSettings.SecretKey = secretKey;

        services.AddSingleton(jwtSettings);

        var databaseSettings = new DatabaseSettings();
        configuration.GetSection("ConnectionStrings").Bind(databaseSettings);
        services.AddSingleton(databaseSettings);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(databaseSettings.DefaultConnection);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITechSupportRepository, TechSupportRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<BA.Backend.Domain.Repositories.ITransportistaRepository, TransportistaRepository>();
        services.AddScoped<BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository, TransportistaRepository>();
        services.AddScoped<ITecnicoRepository, TecnicoRepository>();

        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ICoolerRepository, CoolerRepository>();
        services.AddScoped<INfcTagRepository, NfcTagRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IMermaRepository, MermaRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<INfcValidationService, NfcValidationService>();
        services.AddScoped<ISupervisorRepository, SupervisorRepository>();
        services.AddScoped<IEjecutivoComercialRepository, EjecutivoComercialRepository>();
        services.AddScoped<IClientNoteRepository, ClientNoteRepository>();
        services.AddScoped<IOperationCertificateRepository, OperationCertificateRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<INotificacionService, NotificacionService>();
        services.AddScoped<IGeoLocationService, GeoLocationService>();
        services.AddScoped<IDeviceFingerprintService, DeviceFingerprintService>();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddScoped<ICertificateSignerService, CertificateSignerService>();
        services.AddScoped<IQrGeneratorService, QrGeneratorService>();
        services.AddScoped<IPdfReportService, PdfReportService>();

        // IDateTimeProvider: abstrae DateTime.UtcNow para testabilidad
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
