using AutoMapper;
using Coffee.Application.DTOs;
using Coffee.Application.Interfaces;
using Coffee.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using IngredientEntity = Coffee.Domain.Entities.Ingredient;

namespace Coffee.Application.Services;

public class IngredientService : IIngredientService
{
    private readonly IIngredientRepository _repository;
    private readonly ICacheService _cache;
    private readonly IValidator<CreateIngredientDto> _createValidator;
    private readonly IValidator<UpdateIngredientDto> _updateValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<IngredientService> _logger;

    public IngredientService(
        IIngredientRepository repository,
        ICacheService cache,
        IValidator<CreateIngredientDto> createValidator,
        IValidator<UpdateIngredientDto> updateValidator,
        IMapper mapper,
        ILogger<IngredientService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PagedResult<IngredientDto>>> GetAllAsync(IngredientQueryDto query, CancellationToken ct = default)
    {
        try
        {
            var cacheKey = $"ingredients:page{query.Page}:size{query.PageSize}:search{query.Search ?? "null"}:active{query.IsActive?.ToString() ?? "null"}:sort{query.SortBy ?? "null"}:desc{query.SortDescending}";

            var cached = await _cache.GetAsync<PagedResult<IngredientDto>>(cacheKey, ct);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for ingredient list with key {CacheKey}", cacheKey);
                return Result<PagedResult<IngredientDto>>.Success(cached);
            }

            var (ingredients, totalCount) = await _repository.GetByFilterWithCountAsync(query, ct);
            var ingredientDtos = _mapper.Map<IEnumerable<IngredientDto>>(ingredients);

            var result = new PagedResult<IngredientDto>
            {
                Items = ingredientDtos.ToList(),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);

            return Result<PagedResult<IngredientDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ingredient list with query {@Query}", query);
            return Result<PagedResult<IngredientDto>>.Failure(
                "An error occurred while retrieving the ingredient list",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<IngredientDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result<IngredientDto>.Failure("Invalid ingredient ID", "INVALID_ID");

        try
        {
            var cacheKey = GetCacheKey(id);

            var cached = await _cache.GetAsync<IngredientDto>(cacheKey, ct);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for ingredient {IngredientId}", id);
                return Result<IngredientDto>.Success(cached);
            }

            var ingredient = await _repository.GetByIdAsync(id, ct);
            if (ingredient == null)
            {
                _logger.LogWarning("Ingredient not found: {IngredientId}", id);
                return Result<IngredientDto>.Failure($"Ingredient '{id}' not found", "NOT_FOUND");
            }

            var ingredientDto = _mapper.Map<IngredientDto>(ingredient);

            await _cache.SetAsync(cacheKey, ingredientDto, TimeSpan.FromMinutes(15), ct);

            return Result<IngredientDto>.Success(ingredientDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ingredient {IngredientId}", id);
            return Result<IngredientDto>.Failure(
                "An error occurred while retrieving the ingredient",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<IngredientDto>> CreateAsync(CreateIngredientDto request, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<IngredientDto>.Failure(errors, "VALIDATION_ERROR");
        }

        try
        {
            var existing = await _repository.GetByNameAsync(request.Name, ct);
            if (existing != null)
                return Result<IngredientDto>.Failure($"Ingredient with name '{request.Name}' already exists", "DUPLICATE_NAME");

            var ingredient = _mapper.Map<IngredientEntity>(request);
            ingredient.Id = Guid.NewGuid();
            ingredient.CreatedAt = DateTime.UtcNow;
            ingredient.UpdatedAt = DateTime.UtcNow;

            var created = await _repository.CreateAsync(ingredient, ct);
            var ingredientDto = _mapper.Map<IngredientDto>(created);

            _logger.LogInformation("Created ingredient {IngredientId} with name {Name}", created.Id, created.Name);

            await InvalidateListCaches(ct);

            return Result<IngredientDto>.Success(ingredientDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ingredient with name {Name}", request.Name);
            return Result<IngredientDto>.Failure(
                "An error occurred while creating the ingredient",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<IngredientDto>> UpdateAsync(Guid id, UpdateIngredientDto request, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result<IngredientDto>.Failure("Invalid ingredient ID", "INVALID_ID");

        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<IngredientDto>.Failure(errors, "VALIDATION_ERROR");
        }

        try
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return Result<IngredientDto>.Failure($"Ingredient '{id}' not found", "NOT_FOUND");

            var duplicate = await _repository.GetByNameAsync(request.Name, ct);
            if (duplicate != null && duplicate.Id != id)
                return Result<IngredientDto>.Failure($"Another ingredient with name '{request.Name}' already exists", "DUPLICATE_NAME");

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(existing, ct);
            var ingredientDto = _mapper.Map<IngredientDto>(updated);

            await _cache.RemoveAsync(GetCacheKey(id), ct);
            await InvalidateListCaches(ct);

            _logger.LogInformation("Updated ingredient {IngredientId}", id);

            return Result<IngredientDto>.Success(ingredientDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ingredient {IngredientId}", id);
            return Result<IngredientDto>.Failure(
                "An error occurred while updating the ingredient",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result.Failure("Invalid ingredient ID", "INVALID_ID");

        try
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return Result.Failure($"Ingredient '{id}' not found", "NOT_FOUND");

            await _repository.DeleteAsync(id, ct);

            await _cache.RemoveAsync(GetCacheKey(id), ct);
            await InvalidateListCaches(ct);

            _logger.LogInformation("Deleted ingredient {IngredientId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ingredient {IngredientId}", id);
            return Result.Failure(
                "An error occurred while deleting the ingredient",
                "INTERNAL_ERROR");
        }
    }

    private static string GetCacheKey(Guid id) => $"ingredient:{id}";

    private async Task InvalidateListCaches(CancellationToken ct = default)
    {
        _logger.LogDebug("Invalidating ingredient list caches");
        await Task.CompletedTask;
    }
}
