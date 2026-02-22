using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IngredientEntity = Coffee.Domain.Entities.Ingredient;

namespace Coffee.Persistence.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<IngredientEntity>
{
    public void Configure(EntityTypeBuilder<IngredientEntity> builder)
    {
        builder.ToTable("Ingredients");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(200);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.UpdatedAt);
    }
}
