using Coffee.Application.DTOs;
using Coffee.Domain.Entities;
using CoffeeEntity = Coffee.Domain.Entities.Coffee;

namespace Coffee.Application.Interfaces;

/// <summary>
/// Repository interface for coffee operations
/// </summary>
public interface ICoffeeRepository
{
    Task<IEnumerable<CoffeeEntity>> GetAllAsync(CancellationToken ct = default);
    Task<CoffeeEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CoffeeEntity?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<(IEnumerable<CoffeeEntity> Items, int TotalCount)> GetByFilterWithCountAsync(CoffeeQueryDto query, CancellationToken ct = default);
    Task<CoffeeEntity> CreateAsync(CoffeeEntity coffee, CancellationToken ct = default);
    Task<CoffeeEntity> UpdateAsync(CoffeeEntity coffee, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
