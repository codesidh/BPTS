using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class IISOutputCacheService : IIISOutputCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<IISOutputCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _isEnabled;
    private readonly TimeSpan _defaultExpiration;

    public IISOutputCacheService(
        IDistributedCache distributedCache,
        ILogger<IISOutputCacheService> logger,
        IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _configuration = configuration;
        _isEnabled = configuration.GetValue<bool>("Caching:IISOutputCacheEnabled", true);
        _defaultExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Caching:IISOutputCacheMinutes", 30));
    }

    public async Task<string?> GetCachedOutputAsync(string cacheKey)
    {
        if (!_isEnabled)
        {
            return null;
        }

        try
        {
            var cachedContent = await _distributedCache.GetStringAsync($"output:{cacheKey}");
            if (!string.IsNullOrEmpty(cachedContent))
            {
                _logger.LogDebug("Output cache hit for key: {CacheKey}", cacheKey);
                return cachedContent;
            }

            _logger.LogDebug("Output cache miss for key: {CacheKey}", cacheKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached output for key: {CacheKey}", cacheKey);
            return null;
        }
    }

    public async Task SetCachedOutputAsync(string cacheKey, string content, TimeSpan expiration, string[]? varyByHeaders = null)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _distributedCache.SetStringAsync($"output:{cacheKey}", content, options);
            
            // Store vary-by headers information
            if (varyByHeaders?.Length > 0)
            {
                var varyByInfo = string.Join(",", varyByHeaders);
                await _distributedCache.SetStringAsync($"varyby:{cacheKey}", varyByInfo, options);
            }

            _logger.LogDebug("Set output cache for key: {CacheKey} with expiration: {Expiration}", 
                cacheKey, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached output for key: {CacheKey}", cacheKey);
        }
    }

    public async Task InvalidateOutputCacheAsync(string pattern)
    {
        try
        {
            // TODO: Implement pattern-based cache invalidation
            // This would require maintaining a key registry or using Redis pattern operations
            _logger.LogInformation("Output cache invalidation requested for pattern: {Pattern}", pattern);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating output cache for pattern: {Pattern}", pattern);
        }
    }

    public async Task InvalidateAllOutputCacheAsync()
    {
        try
        {
            // TODO: Implement full output cache clearing
            _logger.LogInformation("All output cache invalidation requested");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all output cache");
        }
    }

    public string GenerateOutputCacheKey(string path, string queryString, string[]? varyByHeaders = null)
    {
        try
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(path.ToLowerInvariant());
            
            if (!string.IsNullOrEmpty(queryString))
            {
                keyBuilder.Append("?").Append(queryString.ToLowerInvariant());
            }

            if (varyByHeaders?.Length > 0)
            {
                keyBuilder.Append("|vary:");
                foreach (var header in varyByHeaders.OrderBy(h => h))
                {
                    keyBuilder.Append(header.ToLowerInvariant()).Append(",");
                }
            }

            var keyString = keyBuilder.ToString();
            
            // Generate a hash to keep cache keys reasonable length
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
            var hashString = Convert.ToBase64String(hashBytes);
            
            // Remove characters that might cause issues
            var cleanHash = hashString.Replace("+", "-").Replace("/", "_").Replace("=", "");
            
            return $"page_{cleanHash}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating output cache key for path: {Path}", path);
            return $"page_{DateTime.UtcNow.Ticks}";
        }
    }
} 