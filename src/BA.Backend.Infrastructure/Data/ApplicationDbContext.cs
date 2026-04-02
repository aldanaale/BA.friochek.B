using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Cooler> Coolers { get; set; } = null!;
    public DbSet<NfcTag> NfcTags { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<TechSupportRequest> TechSupportRequests { get; set; } = null!;

    public DbSet<Route> Routes { get; set; } = null!;
    public DbSet<RouteStop> RouteStops { get; set; } = null!;
    public DbSet<Merma> Mermas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(d => d.Tenant)
                .WithMany()
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cooler>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.SerialNumber).IsUnique();
        });

        modelBuilder.Entity<NfcTag>(entity =>
        {
            entity.HasKey(e => e.TagId);
            entity.HasOne(d => d.Cooler)
                .WithOne(p => p.NfcTag)
                .HasForeignKey<NfcTag>(d => d.CoolerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TechSupportRequest>(entity =>
        {
            entity.ToTable("TechSupportRequests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FaultType).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });
    }
}
