using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BA.Backend.Infrastructure.Data;

/// <summary>
/// DbContext principal de la aplicación.
///
/// FIX F7.1: EF Core solo permite UN HasQueryFilter por entidad.
/// El código original llamaba HasQueryFilter DOS veces (IsDeleted y TenantId),
/// lo que provocaba que el segundo sobrescribiera al primero.
/// Resultado: entidades multi-tenant perdían el soft-delete filter.
///
/// Solución: un solo filtro combinado por tipo de entidad:
///   - IBaseEntity + ITenantEntity → !IsDeleted AND TenantId == current
///   - Solo IBaseEntity            → !IsDeleted
///   - Solo ITenantEntity          → TenantId == current
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService currentTenantService) : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    // ── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Cooler> Coolers { get; set; } = null!;
    public DbSet<NfcTag> NfcTags { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<TechSupportRequest> TechSupportRequests { get; set; } = null!;
    public DbSet<Route> Routes { get; set; } = null!;
    public DbSet<RouteStop> RouteStops { get; set; } = null!;
    public DbSet<Merma> Mermas { get; set; } = null!;
    public DbSet<Transportista> Transportistas { get; set; } = null!;
    public DbSet<Supervisor> Supervisores { get; set; } = null!;
    public DbSet<EjecutivoComercial> EjecutivosComerciales { get; set; } = null!;
    public DbSet<ClientNote> ClientNotes { get; set; } = null!;
    public DbSet<IntegrationLog> IntegrationLogs { get; set; } = null!;
    public DbSet<Tecnico> Tecnicos { get; set; } = null!;
    public DbSet<OperationCertificate> OperationCertificates { get; set; } = null!;

    // ── Auditoría automática ─────────────────────────────────────────────────
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IBaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentTenantService.UserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentTenantService.UserId;
                    break;

                case EntityState.Deleted:
                    // Soft-delete: marcar como eliminado en vez de borrar la fila
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = _currentTenantService.TenantId ?? Guid.Empty;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    // ── Configuración del modelo ─────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        var method = typeof(ApplicationDbContext).GetMethod(nameof(ConfigureGlobalFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                method.MakeGenericMethod(entityType.ClrType).Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void ConfigureGlobalFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
    {
        var isTenant = typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity));
        var isBase = typeof(IBaseEntity).IsAssignableFrom(typeof(TEntity));

        if (isBase && isTenant)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                !((IBaseEntity)e).IsDeleted &&
                (_currentTenantService.IsPlatformAdmin || ((ITenantEntity)e).TenantId == _currentTenantService.TenantId));
        }
        else if (isBase)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !((IBaseEntity)e).IsDeleted);
        }
        else if (isTenant)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => 
                _currentTenantService.IsPlatformAdmin || ((ITenantEntity)e).TenantId == _currentTenantService.TenantId);
        }
    }
}
