using Microsoft.EntityFrameworkCore;
using CoffeeEntity = Coffee.Domain.Entities.Coffee;

namespace Coffee.Persistence.Data;

public class CoffeeDbContext : DbContext
{
    public CoffeeDbContext(DbContextOptions<CoffeeDbContext> options) : base(options)
    {
    }

    public DbSet<CoffeeEntity> Coffees { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditDates();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditDates();
        return base.SaveChanges();
    }

    private void UpdateAuditDates()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is CoffeeEntity)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is CoffeeEntity coffee)
                {
                    coffee.CreatedAt = DateTime.UtcNow;
                    coffee.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is CoffeeEntity coffee)
                {
                    coffee.UpdatedAt = DateTime.UtcNow;
                }
                
                entry.Property("CreatedAt").IsModified = false;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoffeeDbContext).Assembly);
    }
}
