using Microsoft.EntityFrameworkCore;
using Coffee.Application.Interfaces;
using Coffee.Application.DTOs;
using Coffee.Persistence.Data;
using IngredientEntity = Coffee.Domain.Entities.Ingredient;

namespace Coffee.Persistence.Repositories;

public class IngredientRepository : IIngredientRepository
{
    private readonly CoffeeDbContext _context;

    public IngredientRepository(CoffeeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<IngredientEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Ingredients
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .ToListAsync(ct);
    }

    public async Task<IngredientEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Ingredients
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<IngredientEntity?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Ingredients
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<IEnumerable<IngredientEntity>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await _context.Ingredients
            .AsNoTracking()
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<IngredientEntity> Items, int TotalCount)> GetByFilterWithCountAsync(
        IngredientQueryDto query, 
        CancellationToken ct = default)
    {
        var queryable = _context.Ingredients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.ToLower();
            queryable = queryable.Where(i => i.Name.ToLower().Contains(searchTerm));
        }

        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(i => i.IsActive == query.IsActive.Value);
        }

        var totalCount = await queryable.CountAsync(ct);

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            queryable = query.SortBy.ToLower() switch
            {
                "name" => query.SortDescending 
                    ? queryable.OrderByDescending(i => i.Name) 
                    : queryable.OrderBy(i => i.Name),
                "createdat" => query.SortDescending 
                    ? queryable.OrderByDescending(i => i.CreatedAt) 
                    : queryable.OrderBy(i => i.CreatedAt),
                "updatedat" => query.SortDescending 
                    ? queryable.OrderByDescending(i => i.UpdatedAt) 
                    : queryable.OrderBy(i => i.UpdatedAt),
                "isactive" => query.SortDescending 
                    ? queryable.OrderByDescending(i => i.IsActive) 
                    : queryable.OrderBy(i => i.IsActive),
                _ => queryable.OrderBy(i => i.Name)
            };
        }
        else
        {
            queryable = queryable.OrderBy(i => i.Name);
        }

        var items = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IngredientEntity> CreateAsync(IngredientEntity ingredient, CancellationToken ct = default)
    {
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync(ct);
        return ingredient;
    }

    public async Task<IngredientEntity> UpdateAsync(IngredientEntity ingredient, CancellationToken ct = default)
    {
        _context.Ingredients.Update(ingredient);
        await _context.SaveChangesAsync(ct);
        return ingredient;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ingredient = await _context.Ingredients.FindAsync(new object[] { id }, ct);
        if (ingredient != null)
        {
            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync(ct);
        }
    }
}
