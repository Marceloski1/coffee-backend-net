using Coffee.Application.DTOs;
using Coffee.Domain.Common;

namespace Coffee.Application.Interfaces;

public interface IIngredientService
{
    Task<Result<PagedResult<IngredientDto>>> GetAllAsync(IngredientQueryDto query, CancellationToken ct = default);
    Task<Result<IngredientDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IngredientDto>> CreateAsync(CreateIngredientDto request, CancellationToken ct = default);
    Task<Result<IngredientDto>> UpdateAsync(Guid id, UpdateIngredientDto request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
