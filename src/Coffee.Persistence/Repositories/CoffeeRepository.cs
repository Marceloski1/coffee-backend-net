using Microsoft.EntityFrameworkCore;
using Coffee.Application.Interfaces;
using Coffee.Application.DTOs;
using Coffee.Persistence.Data;
using CoffeeEntity = Coffee.Domain.Entities.Coffee;

namespace Coffee.Persistence.Repositories;

public class CoffeeRepository : ICoffeeRepository
{
    private readonly CoffeeDbContext _context;

    public CoffeeRepository(CoffeeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<CoffeeEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Coffees
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<CoffeeEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Coffees
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<CoffeeEntity?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Coffees
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<(IEnumerable<CoffeeEntity> Items, int TotalCount)> GetByFilterWithCountAsync(
        CoffeeQueryDto query, 
        CancellationToken ct = default)
    {
        var queryable = _context.Coffees.AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.ToLower();
            queryable = queryable.Where(c => c.Name.ToLower().Contains(searchTerm));
        }

        // Get total count before pagination
        var totalCount = await queryable.CountAsync(ct);

        // Apply sorting
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

        // Apply pagination
        var items = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<CoffeeEntity> CreateAsync(CoffeeEntity coffee, CancellationToken ct = default)
    {
        _context.Coffees.Add(coffee);
        await _context.SaveChangesAsync(ct);
        return coffee;
    }

    public async Task<CoffeeEntity> UpdateAsync(CoffeeEntity coffee, CancellationToken ct = default)
    {
        _context.Coffees.Update(coffee);
        await _context.SaveChangesAsync(ct);
        return coffee;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var coffee = await _context.Coffees.FindAsync(new object[] { id }, ct);
        if (coffee != null)
        {
            _context.Coffees.Remove(coffee);
            await _context.SaveChangesAsync(ct);
        }
    }
}
