using AutoMapper;
using Coffee.Application.DTOs;
using Coffee.Application.Interfaces;
using Coffee.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CategoryEntity = Coffee.Domain.Entities.Category;

namespace Coffee.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ICacheService _cache;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository repository,
        ICacheService cache,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PagedResult<CategoryDto>>> GetAllAsync(CategoryQueryDto query, CancellationToken ct = default)
    {
        try
        {
            var cacheKey = $"categories:page{query.Page}:size{query.PageSize}:search{query.Search ?? "null"}:sort{query.SortBy ?? "null"}:desc{query.SortDescending}";

            var cached = await _cache.GetAsync<PagedResult<CategoryDto>>(cacheKey, ct);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for category list with key {CacheKey}", cacheKey);
                return Result<PagedResult<CategoryDto>>.Success(cached);
            }

            var (categories, totalCount) = await _repository.GetByFilterWithCountAsync(query, ct);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            var result = new PagedResult<CategoryDto>
            {
                Items = categoryDtos.ToList(),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);

            return Result<PagedResult<CategoryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category list with query {@Query}", query);
            return Result<PagedResult<CategoryDto>>.Failure(
                "An error occurred while retrieving the category list",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result<CategoryDto>.Failure("Invalid category ID", "INVALID_ID");

        try
        {
            var cacheKey = GetCacheKey(id);

            var cached = await _cache.GetAsync<CategoryDto>(cacheKey, ct);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for category {CategoryId}", id);
                return Result<CategoryDto>.Success(cached);
            }

            var category = await _repository.GetByIdAsync(id, ct);
            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", id);
                return Result<CategoryDto>.Failure($"Category '{id}' not found", "NOT_FOUND");
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);

            await _cache.SetAsync(cacheKey, categoryDto, TimeSpan.FromMinutes(15), ct);

            return Result<CategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return Result<CategoryDto>.Failure(
                "An error occurred while retrieving the category",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto request, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<CategoryDto>.Failure(errors, "VALIDATION_ERROR");
        }

        try
        {
            var existing = await _repository.GetByNameAsync(request.Name, ct);
            if (existing != null)
                return Result<CategoryDto>.Failure($"Category with name '{request.Name}' already exists", "DUPLICATE_NAME");

            var category = _mapper.Map<CategoryEntity>(request);
            category.Id = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            var created = await _repository.CreateAsync(category, ct);
            var categoryDto = _mapper.Map<CategoryDto>(created);

            _logger.LogInformation("Created category {CategoryId} with name {Name}", created.Id, created.Name);

            await InvalidateListCaches(ct);

            return Result<CategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category with name {Name}", request.Name);
            return Result<CategoryDto>.Failure(
                "An error occurred while creating the category",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto request, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result<CategoryDto>.Failure("Invalid category ID", "INVALID_ID");

        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<CategoryDto>.Failure(errors, "VALIDATION_ERROR");
        }

        try
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return Result<CategoryDto>.Failure($"Category '{id}' not found", "NOT_FOUND");

            var duplicate = await _repository.GetByNameAsync(request.Name, ct);
            if (duplicate != null && duplicate.Id != id)
                return Result<CategoryDto>.Failure($"Another category with name '{request.Name}' already exists", "DUPLICATE_NAME");

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(existing, ct);
            var categoryDto = _mapper.Map<CategoryDto>(updated);

            await _cache.RemoveAsync(GetCacheKey(id), ct);
            await InvalidateListCaches(ct);

            _logger.LogInformation("Updated category {CategoryId}", id);

            return Result<CategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return Result<CategoryDto>.Failure(
                "An error occurred while updating the category",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result.Failure("Invalid category ID", "INVALID_ID");

        try
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return Result.Failure($"Category '{id}' not found", "NOT_FOUND");

            await _repository.DeleteAsync(id, ct);

            await _cache.RemoveAsync(GetCacheKey(id), ct);
            await InvalidateListCaches(ct);

            _logger.LogInformation("Deleted category {CategoryId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return Result.Failure(
                "An error occurred while deleting the category",
                "INTERNAL_ERROR");
        }
    }

    private static string GetCacheKey(Guid id) => $"category:{id}";

    private async Task InvalidateListCaches(CancellationToken ct = default)
    {
        _logger.LogDebug("Invalidating category list caches");
        await Task.CompletedTask;
    }
}
