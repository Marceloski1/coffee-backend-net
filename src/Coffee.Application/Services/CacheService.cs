using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using Coffee.Application.Options;

namespace Coffee.Application.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly CacheOptions _options;

    public CacheService(
        IMemoryCache cache,
        ILogger<CacheService> logger,
        IOptions<CacheOptions> options)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key {CacheKey}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache miss for key {CacheKey}", key);
        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? _options.DefaultExpiry,
            SlidingExpiration = _options.SlidingExpiry
        };

        _cache.Set(key, value, options);
        _logger.LogDebug("Cached value for key {CacheKey} with expiry {Expiry}", key, options.AbsoluteExpirationRelativeToNow);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        _logger.LogDebug("Removed cache entry for key {CacheKey}", key);
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
            _logger.LogInformation("Cleared all memory cache entries");
        }
        return Task.CompletedTask;
    }
}

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly CacheOptions _options;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger,
        IOptions<CacheOptions> options)
    {
        _cache = redis?.GetDatabase() ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await _cache.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache for key {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var cacheExpiry = expiry ?? _options.DefaultExpiry;
            
            await _cache.StringSetAsync(key, serializedValue, cacheExpiry);
            _logger.LogDebug("Cached value for key {CacheKey} with expiry {Expiry}", key, cacheExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cache.KeyDeleteAsync(key);
            _logger.LogDebug("Removed cache entry for key {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key {CacheKey}", key);
        }
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        try
        {
            var server = _cache.Multiplexer.GetServer(_cache.Multiplexer.GetEndPoints().First());
            await server.FlushAllDatabasesAsync();
            _logger.LogInformation("Cleared all Redis cache databases");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing Redis cache");
        }
    }
}
