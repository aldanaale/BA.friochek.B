
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BA.Backend.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        Console.WriteLine("Iniciando el proceso de la base de datos...");

        try
        {
            Console.WriteLine("Corriendo migraciones...");
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error en migracion: " + ex.Message);
            Console.WriteLine("Intentando borrar y recrear la base de datos...");
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }

        Console.WriteLine("Creando tablas manuales para Dapper...");
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActiveSessions]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[ActiveSessions] (
                    [SessionId] NVARCHAR(64) NOT NULL PRIMARY KEY,
                    [UserId] UNIQUEIDENTIFIER NOT NULL,
                    [IsRevoked] BIT NOT NULL DEFAULT 0,
                    [ExpiresAt] DATETIME2 NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT [FK_ActiveSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
                );
            END
            ELSE
            BEGIN
                -- Si ya existe la tabla, solo checamos que la columna SessionId sea grande para el GUID
                IF (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ActiveSessions' AND COLUMN_NAME = 'SessionId') < 64
                BEGIN
                    ALTER TABLE [dbo].[ActiveSessions] ALTER COLUMN [SessionId] NVARCHAR(64) NOT NULL;
                END
            END
        ");

        
        Console.WriteLine("Buscando el tenant de admin...");
        var adminTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "admin");
        if (adminTenant == null)
        {
            Console.WriteLine("No existe el tenant, lo estamos creando...");
            adminTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Empresa Administradora",
                Slug = "admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(adminTenant);
            await context.SaveChangesAsync();
            Console.WriteLine("Tenant de admin creado con exito");
        }

        var adminEmail = "admin@test.com";
        Console.WriteLine("Buscando si ya existe el usuario: " + adminEmail);
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail && u.TenantId == adminTenant.Id);
        if (adminUser == null)
        {
            Console.WriteLine("El admin no existe, lo estamos creando ahora...");
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = adminTenant.Id,
                Email = adminEmail,
                PasswordHash = passwordHasher.Hash("Admin123!"), // La contraseña por defecto es Admin123!
                FullName = "Administrador del Sistema",
                Role = UserRole.Admin,
                IsActive = true,
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            Console.WriteLine("Usuario administrador creado!");
        }

        Console.WriteLine("Creando usuarios adicionales para pruebas...");

        var clienteEmail = "cliente@test.com";
        if (!await context.Users.AnyAsync(u => u.Email == clienteEmail))
        {
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                TenantId = adminTenant.Id,
                Email = clienteEmail,
                PasswordHash = passwordHasher.Hash("Cliente123!"),
                FullName = "Tienda Juanito Pérez",
                Role = UserRole.Cliente,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        var transEmail = "trans@test.com";
        if (!await context.Users.AnyAsync(u => u.Email == transEmail))
        {
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                TenantId = adminTenant.Id,
                Email = transEmail,
                PasswordHash = passwordHasher.Hash("Trans123!"),
                FullName = "Pedro Repartidor",
                Role = UserRole.Transportista,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        var tecnicoEmail = "tecnico@test.com";
        if (!await context.Users.AnyAsync(u => u.Email == tecnicoEmail))
        {
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                TenantId = adminTenant.Id,
                Email = tecnicoEmail,
                PasswordHash = passwordHasher.Hash("Tecnico123!"),
                FullName = "Marta Soporte Técnico",
                Role = UserRole.Tecnico,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        var storeName = "Tienda Cliente";
        var store = await context.Stores.FirstOrDefaultAsync(s => s.Name == storeName && s.TenantId == adminTenant.Id);
        if (store == null)
        {
            store = new Store
            {
                Id = Guid.NewGuid(),
                TenantId = adminTenant.Id,
                Name = storeName,
                Address = "Av. Siempre Viva 123",
                ContactName = "Juan Pérez",
                ContactPhone = "+56 9 1234 5678",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Stores.Add(store);
            await context.SaveChangesAsync();
        }

        var clienteUser = await context.Users.FirstOrDefaultAsync(u => u.Email == clienteEmail && u.TenantId == adminTenant.Id);
        if (clienteUser != null && !clienteUser.StoreId.HasValue)
        {
            clienteUser.StoreId = store.Id;
            context.Users.Update(clienteUser);
            await context.SaveChangesAsync();
        }

        if (!await context.Coolers.AnyAsync(c => c.StoreId == store.Id))
        {
            context.Coolers.Add(new Cooler
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                SerialNumber = "CL-0001",
                Model = "Cooler Cliente",
                Capacity = 100,
                Status = "Operativo",
                CreatedAt = DateTime.UtcNow,
                LastMaintenanceAt = DateTime.UtcNow.AddDays(-7)
            });
            await context.SaveChangesAsync();
        }

        if (clienteUser != null && !await context.Orders.AnyAsync(o => o.UserId == clienteUser.Id && o.TenantId == adminTenant.Id))
        {
            var cooler = await context.Coolers.FirstOrDefaultAsync(c => c.StoreId == store.Id);
            if (cooler != null)
            {
                var order = Order.Create(adminTenant.Id, clienteUser.Id, cooler.Id, Guid.NewGuid().ToString());
                context.Orders.Add(order);
                await context.SaveChangesAsync();
            }
        }

        if (clienteUser != null && !await context.TechSupportRequests.AnyAsync(r => r.UserId == clienteUser.Id && r.TenantId == adminTenant.Id))
        {
            var cooler = await context.Coolers.FirstOrDefaultAsync(c => c.StoreId == store.Id);
            if (cooler != null)
            {
                context.TechSupportRequests.Add(new TechSupportRequest
                {
                    Id = Guid.NewGuid(),
                    TenantId = adminTenant.Id,
                    UserId = clienteUser.Id,
                    CoolerId = cooler.Id,
                    FaultType = "Mantenimiento",
                    Description = "Solicitud de revisión inicial",
                    PhotoUrls = "[]",
                    ScheduledDate = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pendiente"
                });
                await context.SaveChangesAsync();
            }
        }

        Console.WriteLine("Usuarios de prueba creados con éxito!");

        Console.WriteLine("Proceso de base de datos terminado correctamente.");
    }
}
