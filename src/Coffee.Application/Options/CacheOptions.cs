namespace Coffee.Application.Options;

/// <summary>
/// Configuration options for caching
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Cache";

    public bool UseRedis { get; set; } = false;
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan SlidingExpiry { get; set; } = TimeSpan.FromMinutes(5);
    public string RedisConnection { get; set; } = "localhost:6379";
    public string RedisInstanceName { get; set; } = "Coffee_";
}
