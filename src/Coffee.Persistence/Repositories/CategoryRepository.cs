using Microsoft.EntityFrameworkCore;
using Coffee.Application.Interfaces;
using Coffee.Application.DTOs;
using Coffee.Persistence.Data;
using CategoryEntity = Coffee.Domain.Entities.Category;

namespace Coffee.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly CoffeeDbContext _context;

    public CategoryRepository(CoffeeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<CategoryEntity?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<(IEnumerable<CategoryEntity> Items, int TotalCount)> GetByFilterWithCountAsync(
        CategoryQueryDto query, 
        CancellationToken ct = default)
    {
        var queryable = _context.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.ToLower();
            queryable = queryable.Where(c => c.Name.ToLower().Contains(searchTerm));
        }

        var totalCount = await queryable.CountAsync(ct);

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            queryable = query.SortBy.ToLower() switch
            {
                "name" => query.SortDescending 
                    ? queryable.OrderByDescending(c => c.Name) 
                    : queryable.OrderBy(c => c.Name),
                "createdat" => query.SortDescending 
                    ? queryable.OrderByDescending(c => c.CreatedAt) 
                    : queryable.OrderBy(c => c.CreatedAt),
                "updatedat" => query.SortDescending 
                    ? queryable.OrderByDescending(c => c.UpdatedAt) 
                    : queryable.OrderBy(c => c.UpdatedAt),
                _ => queryable.OrderBy(c => c.Name)
            };
        }
        else
        {
            queryable = queryable.OrderBy(c => c.Name);
        }

        var items = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<CategoryEntity> CreateAsync(CategoryEntity category, CancellationToken ct = default)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(ct);
        return category;
    }

    public async Task<CategoryEntity> UpdateAsync(CategoryEntity category, CancellationToken ct = default)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(ct);
        return category;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { id }, ct);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(ct);
        }
    }
}
