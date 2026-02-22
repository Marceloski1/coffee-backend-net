using Coffee.Application.DTOs;
using Coffee.Domain.Entities;

namespace Coffee.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Category?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<(IEnumerable<Category> Items, int TotalCount)> GetByFilterWithCountAsync(CategoryQueryDto query, CancellationToken ct = default);
    Task<Category> CreateAsync(Category category, CancellationToken ct = default);
    Task<Category> UpdateAsync(Category category, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
