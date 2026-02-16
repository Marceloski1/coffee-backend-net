using AutoMapper;
using Coffee.Application.DTOs;
using Coffee.Application.Interfaces;
using Coffee.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CoffeeEntity = Coffee.Domain.Entities.Coffee;

namespace Coffee.Application.Services;

/// <summary>
/// Service implementation for coffee operations with caching and validation
/// </summary>
public class CoffeeService : ICoffeeService
{
    private readonly ICoffeeRepository _repository;
    private readonly ICacheService _cache;
    private readonly IValidator<CreateCoffeeDto> _createValidator;
    private readonly IValidator<UpdateCoffeeDto> _updateValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<CoffeeService> _logger;

    public CoffeeService(
        ICoffeeRepository repository,
        ICacheService cache,
        IValidator<CreateCoffeeDto> createValidator,
        IValidator<UpdateCoffeeDto> updateValidator,
        IMapper mapper,
        ILogger<CoffeeService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PagedResult<CoffeeDto>>> GetAllAsync(CoffeeQueryDto query, CancellationToken ct = default)
    {
        try
        {
            // Generate cache key based on query parameters
            var cacheKey = $"coffees:page{query.Page}:size{query.PageSize}:search{query.Search ?? "null"}:sort{query.SortBy ?? "null"}:desc{query.SortDescending}";

            // Try to get from cache
            var cached = await _cache.GetAsync<PagedResult<CoffeeDto>>(cacheKey, ct);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for coffee list with key {CacheKey}", cacheKey);
                return Result<PagedResult<CoffeeDto>>.Success(cached);
            }

            // Fetch from repository
            var (coffees, totalCount) = await _repository.GetByFilterWithCountAsync(query, ct);
            var coffeeDtos = _mapper.Map<IEnumerable<CoffeeDto>>(coffees);

            var result = new PagedResult<CoffeeDto>
            {
                Items = coffeeDtos.ToList(),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

            // Cache the result
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);

            return Result<PagedResult<CoffeeDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving coffee list with query {@Query}", query);
            return Result<PagedResult<CoffeeDto>>.Failure(
                "An error occurred while retrieving the coffee list",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<CoffeeDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result<CoffeeDto>.Failure("Invalid coffee ID", "INVALID_ID");

        try
        {
            var cacheKey = GetCacheKey(id);

            // Try cache first
            var cached = await _cache.GetAsync<CoffeeDto>(cacheKey, ct);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for coffee {CoffeeId}", id);
                return Result<CoffeeDto>.Success(cached);
            }

            // Fetch from repository
            var coffee = await _repository.GetByIdAsync(id, ct);
            if (coffee == null)
            {
                _logger.LogWarning("Coffee not found: {CoffeeId}", id);
                return Result<CoffeeDto>.Failure($"Coffee '{id}' not found", "NOT_FOUND");
            }

            var coffeeDto = _mapper.Map<CoffeeDto>(coffee);

            // Populate cache
            await _cache.SetAsync(cacheKey, coffeeDto, TimeSpan.FromMinutes(15), ct);

            return Result<CoffeeDto>.Success(coffeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving coffee {CoffeeId}", id);
            return Result<CoffeeDto>.Failure(
                "An error occurred while retrieving the coffee",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<CoffeeDto>> CreateAsync(CreateCoffeeDto request, CancellationToken ct = default)
    {
        // Validate
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<CoffeeDto>.Failure(errors, "VALIDATION_ERROR");
        }

        try
        {
            // Check for duplicates
            var existing = await _repository.GetByNameAsync(request.Name, ct);
            if (existing != null)
                return Result<CoffeeDto>.Failure($"Coffee with name '{request.Name}' already exists", "DUPLICATE_NAME");

            // Create entity
            var coffee = _mapper.Map<CoffeeEntity>(request);
            coffee.Id = Guid.NewGuid();
            coffee.CreatedAt = DateTime.UtcNow;
            coffee.UpdatedAt = DateTime.UtcNow;

            // Persist
            var created = await _repository.CreateAsync(coffee, ct);
            var coffeeDto = _mapper.Map<CoffeeDto>(created);

            _logger.LogInformation("Created coffee {CoffeeId} with name {Name}", created.Id, created.Name);

            // Invalidate list caches
            await InvalidateListCaches(ct);

            return Result<CoffeeDto>.Success(coffeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coffee with name {Name}", request.Name);
            return Result<CoffeeDto>.Failure(
                "An error occurred while creating the coffee",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result<CoffeeDto>> UpdateAsync(Guid id, UpdateCoffeeDto request, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result<CoffeeDto>.Failure("Invalid coffee ID", "INVALID_ID");

        // Validate
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<CoffeeDto>.Failure(errors, "VALIDATION_ERROR");
        }

        try
        {
            // Fetch existing
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return Result<CoffeeDto>.Failure($"Coffee '{id}' not found", "NOT_FOUND");

            // Check for name conflicts
            var duplicate = await _repository.GetByNameAsync(request.Name, ct);
            if (duplicate != null && duplicate.Id != id)
                return Result<CoffeeDto>.Failure($"Another coffee with name '{request.Name}' already exists", "DUPLICATE_NAME");

            // Apply updates
            existing.Name = request.Name;
            existing.UpdatedAt = DateTime.UtcNow;

            // Persist
            var updated = await _repository.UpdateAsync(existing, ct);
            var coffeeDto = _mapper.Map<CoffeeDto>(updated);

            // Invalidate caches
            await _cache.RemoveAsync(GetCacheKey(id), ct);
            await InvalidateListCaches(ct);

            _logger.LogInformation("Updated coffee {CoffeeId}", id);

            return Result<CoffeeDto>.Success(coffeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coffee {CoffeeId}", id);
            return Result<CoffeeDto>.Failure(
                "An error occurred while updating the coffee",
                "INTERNAL_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return Result.Failure("Invalid coffee ID", "INVALID_ID");

        try
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return Result.Failure($"Coffee '{id}' not found", "NOT_FOUND");

            // Delete
            await _repository.DeleteAsync(id, ct);

            // Invalidate caches
            await _cache.RemoveAsync(GetCacheKey(id), ct);
            await InvalidateListCaches(ct);

            _logger.LogInformation("Deleted coffee {CoffeeId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting coffee {CoffeeId}", id);
            return Result.Failure(
                "An error occurred while deleting the coffee",
                "INTERNAL_ERROR");
        }
    }

    private static string GetCacheKey(Guid id) => $"coffee:{id}";

    private async Task InvalidateListCaches(CancellationToken ct = default)
    {
        // Note: In a real application, you might want to use a more sophisticated
        // cache invalidation strategy, such as cache tags or patterns
        _logger.LogDebug("Invalidating coffee list caches");
        await Task.CompletedTask; // Placeholder for cache invalidation logic
    }
}
