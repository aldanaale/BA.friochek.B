using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BA.Backend.Domain.Entities;

namespace BA.Backend.Infrastructure.Data.Configurations;

public class MermaConfiguration : IEntityTypeConfiguration<Merma>
{
    public void Configure(EntityTypeBuilder<Merma> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ProductName)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(e => e.Reason)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(e => e.PhotoUrl)
            .IsRequired();
            
        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasOne(e => e.Cooler)
            .WithMany()
            .HasForeignKey(e => e.CoolerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
