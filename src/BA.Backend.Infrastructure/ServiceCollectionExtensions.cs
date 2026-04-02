using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using BA.Backend.Infrastructure.Repositories;
using BA.Backend.Infrastructure.Services;
using BA.Backend.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Application.Tecnico.Interfaces;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BA.Backend.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection("Jwt").Bind(jwtSettings);
        services.AddSingleton(jwtSettings);

        var databaseSettings = new DatabaseSettings();
        configuration.GetSection("ConnectionStrings").Bind(databaseSettings);
        services.AddSingleton(databaseSettings);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(databaseSettings.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        if (databaseSettings.UseRealDatabase)
        {
            Console.WriteLine("MODO: Base de Datos Real activada.");
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ITechSupportRepository, TechSupportRepository>();
            services.AddScoped<ITransportistaRepository, TransportistaRepository>(); 
            services.AddScoped<ITecnicoRepository, TecnicoRepository>();
        }
        else
        {
            Console.WriteLine("MODO: Simulación (Base de Datos desactivada).");
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ITechSupportRepository, TechSupportRepository>();
            
            services.AddScoped<ITransportistaRepository, TransportistaRepository>();
            services.AddScoped<ITecnicoRepository, TecnicoRepository>();
        }
        
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ICoolerRepository, CoolerRepository>();
        services.AddScoped<INfcTagRepository, NfcTagRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IMermaRepository, MermaRepository>();
        services.AddScoped<INfcValidationService, NfcValidationService>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
