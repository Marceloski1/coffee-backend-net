using Coffee.Application.DTOs;
using Coffee.Domain.Entities;

namespace Coffee.Application.Interfaces;

public interface IIngredientRepository
{
    Task<IEnumerable<Ingredient>> GetAllAsync(CancellationToken ct = default);
    Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Ingredient?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IEnumerable<Ingredient>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<(IEnumerable<Ingredient> Items, int TotalCount)> GetByFilterWithCountAsync(IngredientQueryDto query, CancellationToken ct = default);
    Task<Ingredient> CreateAsync(Ingredient ingredient, CancellationToken ct = default);
    Task<Ingredient> UpdateAsync(Ingredient ingredient, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
