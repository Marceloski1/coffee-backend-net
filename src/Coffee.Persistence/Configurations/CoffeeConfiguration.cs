using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoffeeEntity = Coffee.Domain.Entities.Coffee;

namespace Coffee.Persistence.Configurations;

/// <summary>
/// Entity configuration for Coffee entity
/// </summary>
public class CoffeeConfiguration : IEntityTypeConfiguration<CoffeeEntity>
{
    public void Configure(EntityTypeBuilder<CoffeeEntity> builder)
    {
        builder.ToTable("Coffees");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Add index on Name for faster lookups
        builder.HasIndex(e => e.Name)
            .IsUnique();

        // Add index on CreatedAt for sorting
        builder.HasIndex(e => e.CreatedAt);

        // Add index on UpdatedAt for sorting
        builder.HasIndex(e => e.UpdatedAt);
    }
}
