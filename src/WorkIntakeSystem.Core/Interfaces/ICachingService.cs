namespace WorkIntakeSystem.Core.Interfaces;

public interface IMultiTierCachingService
{
    // L1 - Memory Cache (fastest)
    Task<T?> GetFromMemoryAsync<T>(string key);
    Task SetInMemoryAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveFromMemoryAsync(string key);

    // L2 - Redis Distributed Cache (shared across instances)
    Task<T?> GetFromDistributedAsync<T>(string key);
    Task SetInDistributedAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveFromDistributedAsync(string key);

    // Multi-tier operations
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? memoryExpiration = null, TimeSpan? distributedExpiration = null);
    Task RemoveAsync(string key);
    Task ClearAllAsync();
}

public interface IDatabaseQueryCacheService
{
    Task<T?> GetQueryResultAsync<T>(string queryKey);
    Task SetQueryResultAsync<T>(string queryKey, T result, TimeSpan? expiration = null);
    Task InvalidateQueryCacheAsync(string pattern);
    Task InvalidateTableCacheAsync(string tableName);
    string GenerateQueryKey(string query, object? parameters = null);
}

public interface IConfigurationCacheService
{
    Task<T?> GetConfigurationAsync<T>(string configKey, int? businessVerticalId = null);
    Task SetConfigurationAsync<T>(string configKey, T value, int? businessVerticalId = null, TimeSpan? expiration = null);
    Task InvalidateConfigurationAsync(string configKey, int? businessVerticalId = null);
    Task InvalidateAllConfigurationsAsync();
    Task RefreshConfigurationAsync(string configKey, int? businessVerticalId = null);
}

public interface IIISOutputCacheService
{
    Task<string?> GetCachedOutputAsync(string cacheKey);
    Task SetCachedOutputAsync(string cacheKey, string content, TimeSpan expiration, string[]? varyByHeaders = null);
    Task InvalidateOutputCacheAsync(string pattern);
    Task InvalidateAllOutputCacheAsync();
    string GenerateOutputCacheKey(string path, string queryString, string[]? varyByHeaders = null);
} 