using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class MultiTierCachingService : IMultiTierCachingService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<MultiTierCachingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _defaultMemoryExpiration;
    private readonly TimeSpan _defaultDistributedExpiration;

    public MultiTierCachingService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<MultiTierCachingService> logger,
        IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _configuration = configuration;
        
        _defaultMemoryExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Caching:DefaultMemoryExpirationMinutes", 15));
        _defaultDistributedExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Caching:DefaultDistributedExpirationMinutes", 60));
    }

    public async Task<T?> GetFromMemoryAsync<T>(string key)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var value))
            {
                _logger.LogDebug("Cache hit (Memory) for key: {Key}", key);
                return (T?)value;
            }

            _logger.LogDebug("Cache miss (Memory) for key: {Key}", key);
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting from memory cache for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetInMemoryAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultMemoryExpiration,
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set(key, value, options);
            _logger.LogDebug("Set memory cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting memory cache for key: {Key}", key);
        }
    }

    public async Task RemoveFromMemoryAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Removed from memory cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from memory cache for key: {Key}", key);
        }
    }

    public async Task<T?> GetFromDistributedAsync<T>(string key)
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache hit (Distributed) for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue);
            }

            _logger.LogDebug("Cache miss (Distributed) for key: {Key}", key);
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting from distributed cache for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetInDistributedAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultDistributedExpiration
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options);
            _logger.LogDebug("Set distributed cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting distributed cache for key: {Key}", key);
        }
    }

    public async Task RemoveFromDistributedAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Removed from distributed cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from distributed cache for key: {Key}", key);
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try 
        {
            // Try L1 cache (Memory) first
            var memoryResult = await GetFromMemoryAsync<T>(key);
            if (memoryResult != null)
            {
                return memoryResult;
            }

            // Try L2 cache (Distributed/Redis)
            var distributedResult = await GetFromDistributedAsync<T>(key);
            if (distributedResult != null)
            {
                // Promote to L1 cache
                await SetInMemoryAsync(key, distributedResult);
                return distributedResult;
            }

            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multi-tier cache get for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? memoryExpiration = null, TimeSpan? distributedExpiration = null)
    {
        try
        {
            // Set in both tiers
            var memoryTask = SetInMemoryAsync(key, value, memoryExpiration);
            var distributedTask = SetInDistributedAsync(key, value, distributedExpiration);
        
            await Task.WhenAll(memoryTask, distributedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multi-tier cache set for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var memoryTask = RemoveFromMemoryAsync(key);
            var distributedTask = RemoveFromDistributedAsync(key);
            
            await Task.WhenAll(memoryTask, distributedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multi-tier cache remove for key: {Key}", key);
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            // Clear memory cache
            if (_memoryCache is MemoryCache memCache)
            {
                memCache.Clear();
            }

            // For distributed cache, we'd need to track keys or use a different approach
            _logger.LogWarning("Distributed cache clear all not implemented - requires key tracking");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all caches");
        }
    }
}

public class DatabaseQueryCacheService : IDatabaseQueryCacheService
{
    private readonly IMultiTierCachingService _cachingService;
    private readonly ILogger<DatabaseQueryCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _defaultQueryCacheExpiration;

    public DatabaseQueryCacheService(
        IMultiTierCachingService cachingService,
        ILogger<DatabaseQueryCacheService> logger,
        IConfiguration configuration)
    {
        _cachingService = cachingService;
        _logger = logger;
        _configuration = configuration;
        _defaultQueryCacheExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Caching:DatabaseQueryCacheMinutes", 15));
    }

    public async Task<T?> GetQueryResultAsync<T>(string queryKey)
    {
        try
        {
            var cacheKey = $"query:{queryKey}";
            return await _cachingService.GetAsync<T>(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query result from cache: {QueryKey}", queryKey);
            return default(T);
        }
    }

    public async Task SetQueryResultAsync<T>(string queryKey, T result, TimeSpan? expiration = null)
    {
        try
        {
            var cacheKey = $"query:{queryKey}";
            var exp = expiration ?? _defaultQueryCacheExpiration;
            await _cachingService.SetAsync(cacheKey, result, exp, exp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting query result in cache: {QueryKey}", queryKey);
        }
    }

    public async Task InvalidateQueryCacheAsync(string pattern)
    {
        try
        {
            // TODO: Implement pattern-based cache invalidation
            _logger.LogInformation("Query cache invalidation for pattern: {Pattern}", pattern);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating query cache for pattern: {Pattern}", pattern);
        }
    }

    public async Task InvalidateTableCacheAsync(string tableName)
    {
        try
        {
            // Invalidate all queries related to a specific table
            await InvalidateQueryCacheAsync($"*{tableName}*");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating table cache: {TableName}", tableName);
        }
    }

    public string GenerateQueryKey(string query, object? parameters = null)
    {
        try
        {
            var parametersHash = parameters != null ? 
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters))) : 
                string.Empty;
            
            var queryHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(query));
            return $"{queryHash}:{parametersHash}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query key");
            return $"query_{DateTime.UtcNow.Ticks}";
        }
    }
}

public class ConfigurationCacheService : IConfigurationCacheService
{
    private readonly IMultiTierCachingService _cachingService;
    private readonly ILogger<ConfigurationCacheService> _logger;
    private readonly TimeSpan _configurationCacheExpiration;

    public ConfigurationCacheService(
        IMultiTierCachingService cachingService,
        ILogger<ConfigurationCacheService> logger,
        IConfiguration configuration)
    {
        _cachingService = cachingService;
        _logger = logger;
        _configurationCacheExpiration = TimeSpan.FromHours(
            configuration.GetValue<int>("Caching:ConfigurationCacheHours", 2));
    }

    public async Task<T?> GetConfigurationAsync<T>(string configKey, int? businessVerticalId = null)
    {
        try
        {
            var cacheKey = GenerateConfigCacheKey(configKey, businessVerticalId);
            return await _cachingService.GetAsync<T>(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration from cache: {ConfigKey}", configKey);
            return default(T);
        }
    }

    public async Task SetConfigurationAsync<T>(string configKey, T value, int? businessVerticalId = null, TimeSpan? expiration = null)
    {
        try
        {
            var cacheKey = GenerateConfigCacheKey(configKey, businessVerticalId);
            var exp = expiration ?? _configurationCacheExpiration;
            await _cachingService.SetAsync(cacheKey, value, exp, exp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting configuration in cache: {ConfigKey}", configKey);
        }
    }

    public async Task InvalidateConfigurationAsync(string configKey, int? businessVerticalId = null)
    {
        try
        {
            var cacheKey = GenerateConfigCacheKey(configKey, businessVerticalId);
            await _cachingService.RemoveAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating configuration cache: {ConfigKey}", configKey);
        }
    }

    public async Task InvalidateAllConfigurationsAsync()
    {
        try
        {
            // TODO: Implement configuration cache clearing
            _logger.LogInformation("Invalidating all configuration cache");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all configuration cache");
        }
    }

    public async Task RefreshConfigurationAsync(string configKey, int? businessVerticalId = null)
    {
        try
        {
            await InvalidateConfigurationAsync(configKey, businessVerticalId);
            _logger.LogInformation("Refreshed configuration cache: {ConfigKey}", configKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing configuration cache: {ConfigKey}", configKey);
        }
    }

    private static string GenerateConfigCacheKey(string configKey, int? businessVerticalId)
    {
        return businessVerticalId.HasValue 
            ? $"config:{configKey}:bv_{businessVerticalId}" 
            : $"config:{configKey}:global";
    }
} 