using System.Linq;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BA.Backend.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("┌──────────────────────────────────────────────────────────┐");
        logger.LogInformation("│   VERIFICANDO INTEGRIDAD DE LA BASE DE DATOS...          │");
        logger.LogInformation("└──────────────────────────────────────────────────────────┘");

        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("[UPDATE] Aplicando {Count} migraciones críticas para estabilizar el esquema...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("[READY] Sincronización de esquema completada exitosamente.");
            }
            else
            {
                logger.LogInformation("[OK] Esquema SQL ya se encuentra en su versión más reciente.");
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2714)
        {
            logger.LogInformation("La estructura ya existe en SQL Server. Sincronización completa.");
        }
        catch (Exception ex) when (ex.Message.Contains("already an object named"))
        {
            logger.LogInformation("La estructura base ya está presente. Omitiendo migración inicial.");
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            logger.LogCritical("Error crítico de conexión SQL: {Message}", sqlEx.Message);
            if (sqlEx.Number == -1 || sqlEx.Number == 2 || sqlEx.Number == 53)
                logger.LogError("Sugerencia: Verifica que el servidor SQL esté encendido y acepte conexiones remotas.");
            else if (sqlEx.Number == 18456)
                logger.LogError("Sugerencia: Error de inicio de sesión. Verifica usuario y contraseña en el ConnectionString.");

            throw; // Re-lanzar para detener el inicio si la DB es vital
        }
        catch (Exception ex)
        {
            logger.LogCritical("Error inesperado al inicializar la base de datos: {Message}", ex.Message);
            throw;
        }

        // Tabla ActiveSessions (manejada por Dapper)
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id=OBJECT_ID(N'[dbo].[ActiveSessions]') AND type='U')
                BEGIN
                    CREATE TABLE [dbo].[ActiveSessions] (
                        [SessionId] NVARCHAR(64) NOT NULL PRIMARY KEY,
                        [UserId] UNIQUEIDENTIFIER NOT NULL,
                        [IsRevoked] BIT NOT NULL DEFAULT 0,
                        [ExpiresAt] DATETIME2 NOT NULL,
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [FK_ActiveSessions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
                    );
                END");
        }
        catch (Exception ex)
        {
            logger.LogWarning("No se pudo verificar/crear ActiveSessions: {Message}", ex.Message);
        }

        // 1. Tenants
        var tSavory = await GetOrCreateTenant(context, "Savory Chile", "savory-chile");
        var tBresler = await GetOrCreateTenant(context, "Bresler Chile", "bresler-chile");
        var tCoppelia = await GetOrCreateTenant(context, "Coppelia Chile", "coppelia-chile");

        // 2. Usuarios de prueba con diferentes Tenants
        var testUsers = new[]
        {
            // Admins
            (tSavory.Id,   "admin@savory.cl",         "Frio2026!", "Roberto Admin", UserRole.Admin),
            (tBresler.Id,  "ignacio.romo@bresler.cl",  "Frio2026!", "Ignacio Admin", UserRole.Admin),
            (tCoppelia.Id, "gabriela.paz@coppelia.cl", "Frio2026!", "Gabriela Admin", UserRole.Admin),
            
            // Tecnicos
            (tSavory.Id,   "tec.frio@savory.cl",    "Frio2026!", "Carlos Tecnico", UserRole.Tecnico),
            (tBresler.Id,  "tec.elec@bresler.cl",   "Frio2026!", "Jorge Tecnico", UserRole.Tecnico),
            
            // Transportistas
            (tSavory.Id,   "trans1@savory.cl",  "Frio2026!", "Sebastian Trans", UserRole.Transportista),
            (tBresler.Id,  "trans1@bresler.cl", "Frio2026!", "Marcelo Trans", UserRole.Transportista),

            // Clientes
            (tSavory.Id,   "cliente1@savory.cl",      "Frio2026!", "Maria Cliente", UserRole.Cliente),
            (tBresler.Id,  "cliente.local@bresler.cl", "Frio2026!", "Daniela Cliente", UserRole.Cliente),
            (tCoppelia.Id, "arturo.vidal@coppelia.cl", "Frio2026!", "Arturo Cliente", UserRole.Cliente),

            // Supervisor y Ejecutivo (Nuevos)
            (tSavory.Id,   "supervisor@savory.cl", "Frio2026!", "Andres Supervisor", UserRole.Supervisor),
            (tSavory.Id,   "vendedor@savory.cl",   "Frio2026!", "Luna Ejecutiva", UserRole.EjecutivoComercial),
        };

        // Hardcoded hash for 'Frio2026!' generating it once to be extremely safe
        string hardcodedHash = hasher.Hash("Frio2026!");

        foreach (var (tId, email, pwd, fullName, role) in testUsers)
        {
            var existingUser = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser == null)
            {
                var nameParts = fullName.Split(' ');
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    TenantId = tId,
                    Email = email,
                    PasswordHash = hardcodedHash,
                    Name = firstName,
                    LastName = lastName,
                    Role = role,
                    IsActive = true,
                    IsLocked = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingUser.PasswordHash = hardcodedHash;
                existingUser.IsActive = true;
                existingUser.IsLocked = false;
                context.Users.Update(existingUser);
            }
        }
        await context.SaveChangesAsync();

        // Si ya existen rutas o pedidos, saltamos el resto del sembrado para preservar integridad operacional
        if (await context.Routes.AnyAsync())
        {
            logger.LogInformation("Datos operativos detectados. Saltando sembrado de activos/tiendas para preservar integridad.");
            return;
        }

        // 3. Stores, Coolers y NfcTags para cumplir los tests
        var tenants = new[] { tSavory, tBresler, tCoppelia };
        int storeCounter = 1;

        foreach (var t in tenants)
        {
            for (int i = 0; i < 3; i++)
            {
                var storeName = $"Tienda {t.Name} #{i + 1}";
                var s = await context.Stores.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == storeName && x.TenantId == t.Id);
                if (s == null)
                {
                    s = new Store
                    {
                        Id = Guid.NewGuid(),
                        TenantId = t.Id,
                        Name = storeName,
                        Address = $"Calle Falsa {storeCounter++}",
                        City = "Santiago",
                        District = "Centro",
                        ContactName = "Encargado",
                        ContactPhone = "+569 1111 2222",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Stores.Add(s);
                    await context.SaveChangesAsync();
                }

                // Agregar 2 coolers por tienda
                for (int j = 0; j < 2; j++)
                {
                    var sn = $"SN-{t.Slug[..3]}-{storeCounter}-{j}";
                    var cooler = await context.Coolers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.SerialNumber == sn);
                    if (cooler == null)
                    {
                        cooler = new Cooler
                        {
                            Id = Guid.NewGuid(),
                            TenantId = t.Id,
                            StoreId = s.Id,
                            SerialNumber = sn,
                            Name = $"Cooler {sn}",
                            Model = "B-Standard",
                            Capacity = 150,
                            Status = "Activo",
                            CreatedAt = DateTime.UtcNow
                        };
                        context.Coolers.Add(cooler);
                        await context.SaveChangesAsync();

                        // Agregar un Tag NFC para cada cooler (solo si no existe)
                        var tagId = $"TAG-{sn}";
                        var existingTag = await context.NfcTags.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TagId == tagId);
                        if (existingTag == null)
                        {
                            var tag = new NfcTag
                            {
                                TagId = tagId,
                                CoolerId = cooler.Id,
                                IsEnrolled = true,
                                Status = "Activo",
                                CreatedAt = DateTime.UtcNow,
                                SecurityHash = "HASH_" + sn
                            };
                            context.NfcTags.Add(tag);
                        }
                    }
                }
            }
        }
        await context.SaveChangesAsync();

        // 4. Asignar store al cliente de Bresler (específico para tests previos)
        var breslerStore = await context.Stores.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.TenantId == tBresler.Id);
        var cliente = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == "cliente.local@bresler.cl" && u.TenantId == tBresler.Id);
        if (cliente != null && !cliente.StoreId.HasValue && breslerStore != null)
        {
            cliente.StoreId = breslerStore.Id;
            context.Users.Update(cliente);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Seed completado correctamente.");
    }

    private static async Task<Tenant> GetOrCreateTenant(ApplicationDbContext context, string name, string slug)
    {
        var tenant = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Slug == slug);
        if (tenant == null)
        {
            tenant = new Tenant { Id = Guid.NewGuid(), Name = name, Slug = slug, IsActive = true, CreatedAt = DateTime.UtcNow };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }
        return tenant;
    }
}
