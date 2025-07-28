using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class ApiGatewayService : IApiGatewayService
{
    private readonly IRateLimitingService _rateLimitingService;
    private readonly IApiVersioningService _versioningService;
    private readonly IRequestTransformationService _transformationService;
    private readonly ILogger<ApiGatewayService> _logger;
    private readonly IConfiguration _configuration;

    public ApiGatewayService(
        IRateLimitingService rateLimitingService,
        IApiVersioningService versioningService,
        IRequestTransformationService transformationService,
        ILogger<ApiGatewayService> logger,
        IConfiguration configuration)
    {
        _rateLimitingService = rateLimitingService;
        _versioningService = versioningService;
        _transformationService = transformationService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> ValidateRateLimit(string clientId, string endpoint)
    {
        try
        {
            var isAllowed = await _rateLimitingService.IsRequestAllowed(clientId, endpoint);
            
            if (!isAllowed)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", 
                    clientId, endpoint);
            }

            return isAllowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating rate limit for client {ClientId}", clientId);
            return false;
        }
    }

    public async Task<bool> ValidateApiVersion(string version, string endpoint)
    {
        try
        {
            var isSupported = _versioningService.IsVersionSupported(version, endpoint);
            
            if (!isSupported)
            {
                _logger.LogWarning("Unsupported API version {Version} requested for endpoint {Endpoint}", 
                    version, endpoint);
            }

            return isSupported;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API version {Version} for endpoint {Endpoint}", 
                version, endpoint);
            return false;
        }
    }

    public async Task<object?> TransformRequest(object request, string targetFormat)
    {
        try
        {
            return await _transformationService.TransformRequest<object>(request, "json", targetFormat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming request to format {Format}", targetFormat);
            return null;
        }
    }

    public async Task<object?> TransformResponse(object response, string targetFormat)
    {
        try
        {
            return await _transformationService.TransformResponse(response, "json", targetFormat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming response to format {Format}", targetFormat);
            return null;
        }
    }

    public async Task LogApiCall(string clientId, string endpoint, string method, int statusCode, long responseTime)
    {
        try
        {
            var logEntry = new
            {
                ClientId = clientId,
                Endpoint = endpoint,
                Method = method,
                StatusCode = statusCode,
                ResponseTime = responseTime,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("API Call: {LogEntry}", JsonSerializer.Serialize(logEntry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging API call for client {ClientId}", clientId);
        }
    }

    public async Task<bool> IsEndpointAllowed(string endpoint, string clientId)
    {
        try
        {
            // TODO: Implement endpoint access control based on client permissions
            // For now, allow all endpoints
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking endpoint access for client {ClientId}", clientId);
            return false;
        }
    }
}

public class RateLimitingService : IRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateLimitingService> _logger;
    private readonly int _defaultRateLimit;
    private readonly int _timeWindowSeconds;

    public RateLimitingService(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
        _defaultRateLimit = configuration.GetValue<int>("ApiGateway:RateLimitPerMinute", 1000);
        _timeWindowSeconds = 60; // 1 minute window
    }

    public async Task<bool> IsRequestAllowed(string clientId, string endpoint)
    {
        try
        {
            var rateLimitInfo = await GetRateLimitInfo(clientId, endpoint);
            return rateLimitInfo.IsAllowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for client {ClientId}", clientId);
            return true; // Allow on error to prevent blocking
        }
    }

    public async Task<RateLimitInfo> GetRateLimitInfo(string clientId, string endpoint)
    {
        try
        {
            var key = $"rate_limit:{clientId}:{endpoint}";
            var currentCount = await GetCurrentRequestCount(key);
            var resetTime = DateTime.UtcNow.AddSeconds(_timeWindowSeconds);

            var rateLimitInfo = new RateLimitInfo
            {
                Limit = _defaultRateLimit,
                Remaining = Math.Max(0, _defaultRateLimit - currentCount),
                ResetTime = resetTime,
                IsAllowed = currentCount < _defaultRateLimit
            };

            return rateLimitInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rate limit info for client {ClientId}", clientId);
            return new RateLimitInfo { IsAllowed = true, Limit = _defaultRateLimit, Remaining = _defaultRateLimit };
        }
    }

    public async Task IncrementRequestCount(string clientId, string endpoint)
    {
        try
        {
            var key = $"rate_limit:{clientId}:{endpoint}";
            var currentCountStr = await _cache.GetStringAsync(key);
            var currentCount = int.TryParse(currentCountStr, out var count) ? count : 0;
            
            var newCount = currentCount + 1;
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_timeWindowSeconds)
            };
            
            await _cache.SetStringAsync(key, newCount.ToString(), options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing request count for client {ClientId}", clientId);
        }
    }

    public async Task ResetRateLimit(string clientId, string endpoint)
    {
        try
        {
            var key = $"rate_limit:{clientId}:{endpoint}";
            await _cache.RemoveAsync(key);
            _logger.LogInformation("Reset rate limit for client {ClientId} on endpoint {Endpoint}", 
                clientId, endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit for client {ClientId}", clientId);
        }
    }

    private async Task<int> GetCurrentRequestCount(string key)
    {
        var countStr = await _cache.GetStringAsync(key);
        return int.TryParse(countStr, out var count) ? count : 0;
    }
}

public class ApiVersioningService : IApiVersioningService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiVersioningService> _logger;
    private readonly string _defaultVersion;
    private readonly string _versionHeader;

    public ApiVersioningService(IConfiguration configuration, ILogger<ApiVersioningService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _defaultVersion = configuration.GetValue<string>("ApiGateway:DefaultApiVersion", "1.0")!;
        _versionHeader = configuration.GetValue<string>("ApiGateway:ApiVersionHeader", "X-Api-Version")!;
    }

    public string GetRequestedVersion(IDictionary<string, object> requestHeaders)
    {
        // Check header first
        if (requestHeaders.TryGetValue(_versionHeader, out var headerValue))
        {
            return headerValue?.ToString() ?? _defaultVersion;
        }

        // Check version query parameter if passed in headers dictionary
        if (requestHeaders.TryGetValue("version", out var queryValue))
        {
            return queryValue?.ToString() ?? _defaultVersion;
        }

        // Check path-based version if passed in headers dictionary
        if (requestHeaders.TryGetValue("path", out var pathValue))
        {
            var pathSegments = pathValue?.ToString()?.Split('/') ?? Array.Empty<string>();
            var versionSegment = pathSegments.FirstOrDefault(s => s.StartsWith("v", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(versionSegment))
            {
                return versionSegment.Substring(1); // Remove 'v' prefix
            }
        }

        return _defaultVersion;
    }

    public bool IsVersionSupported(string version, string endpoint)
    {
        // TODO: Implement version compatibility matrix based on endpoint
        // For now, support versions 1.0 and 2.0
        var supportedVersions = new[] { "1.0", "2.0" };
        return supportedVersions.Contains(version);
    }

    public string GetDefaultVersion()
    {
        return _defaultVersion;
    }

    public async Task<IEnumerable<string>> GetSupportedVersions(string endpoint)
    {
        // TODO: Implement endpoint-specific version support
        return new[] { "1.0", "2.0" };
    }
}

public class RequestTransformationService : IRequestTransformationService
{
    private readonly ILogger<RequestTransformationService> _logger;

    public RequestTransformationService(ILogger<RequestTransformationService> logger)
    {
        _logger = logger;
    }

    public async Task<T?> TransformRequest<T>(object request, string sourceFormat, string targetFormat)
    {
        try
        {
            if (sourceFormat.Equals(targetFormat, StringComparison.OrdinalIgnoreCase))
            {
                // No transformation needed - directly deserialize the request object
                if (request is string stringContent)
                {
                    return JsonSerializer.Deserialize<T>(stringContent);
                }
                else if (request is T directObject)
                {
                    return directObject;
                }
            }

            // TODO: Implement format transformation logic (JSON to XML, etc.)
            _logger.LogWarning("Format transformation from {Source} to {Target} not implemented", 
                sourceFormat, targetFormat);
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming request from {Source} to {Target}", 
                sourceFormat, targetFormat);
            return default(T);
        }
    }

    public async Task<object?> TransformResponse(object response, string sourceFormat, string targetFormat)
    {
        try
        {
            if (sourceFormat.Equals(targetFormat, StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }

            // TODO: Implement response transformation logic
            _logger.LogWarning("Response transformation from {Source} to {Target} not implemented", 
                sourceFormat, targetFormat);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming response from {Source} to {Target}", 
                sourceFormat, targetFormat);
            return response;
        }
    }

    public async Task<bool> ValidateRequestFormat(object request, string expectedFormat)
    {
        try
        {
            // For generic object validation, we'll check the request type
            return expectedFormat.ToLowerInvariant() switch
            {
                "json" => request is string || request.GetType().IsClass,
                "xml" => request is string,
                _ => true // Allow unknown formats for now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating request format {Format}", expectedFormat);
            return false;
        }
    }
} 