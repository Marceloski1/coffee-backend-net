using Coffee.Application.DTOs;
using Coffee.Domain.Common;

namespace Coffee.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<PagedResult<CategoryDto>>> GetAllAsync(CategoryQueryDto query, CancellationToken ct = default);
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto request, CancellationToken ct = default);
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
