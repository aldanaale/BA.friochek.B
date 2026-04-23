using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.FullName);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        // FIX: Role como byte (TINYINT en BD). Era HasConversion<string>() → conflicto de tipo
        builder.Property(e => e.Role).IsRequired().HasConversion<byte>();

        // Sub-tipos opcionales almacenados como TINYINT NULL
        builder.Property(e => e.ClientType).HasConversion<byte?>().IsRequired(false);
        builder.Property(e => e.TransportType).HasConversion<byte?>().IsRequired(false);

        builder.HasOne(e => e.Store).WithMany().HasForeignKey(e => e.StoreId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.Role });
        builder.HasIndex(e => e.StoreId);
    }
}
