using BA.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class NfcTagConfiguration : IEntityTypeConfiguration<NfcTag>
{
    public void Configure(EntityTypeBuilder<NfcTag> builder)
    {
        builder.HasKey(e => e.TagId);
        builder.Property(e => e.SecurityHash).IsRequired().HasMaxLength(256);

        // FIX: Cooler.NfcTag es navegación singular (NfcTag?), no colección
        // Usar WithOne() en lugar de WithMany() elimina el shadow property CoolerId1
        builder.HasOne(e => e.Cooler)
            .WithOne(c => c.NfcTag)
            .HasForeignKey<NfcTag>(e => e.CoolerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(e => e.CoolerId);
    }
}
