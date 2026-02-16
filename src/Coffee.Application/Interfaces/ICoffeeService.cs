using Coffee.Domain.Common;
using Coffee.Application.DTOs;

namespace Coffee.Application.Interfaces;

/// <summary>
/// Service interface for coffee operations following Clean Architecture patterns
/// </summary>
public interface ICoffeeService
{
    Task<Result<PagedResult<CoffeeDto>>> GetAllAsync(CoffeeQueryDto query, CancellationToken ct = default);
    Task<Result<CoffeeDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CoffeeDto>> CreateAsync(CreateCoffeeDto request, CancellationToken ct = default);
    Task<Result<CoffeeDto>> UpdateAsync(Guid id, UpdateCoffeeDto request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
