using Microsoft.AspNetCore.Http;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IApiGatewayService
{
    Task<bool> ValidateRateLimit(string clientId, string endpoint);
    Task<bool> ValidateApiVersion(string version, string endpoint);
    Task<object?> TransformRequest(HttpRequest request, string targetFormat);
    Task<object?> TransformResponse(object response, string targetFormat);
    Task LogApiCall(string clientId, string endpoint, string method, int statusCode, long responseTime);
    Task<bool> IsEndpointAllowed(string endpoint, string clientId);
}

public interface IRateLimitingService
{
    Task<bool> IsRequestAllowed(string clientId, string endpoint);
    Task<RateLimitInfo> GetRateLimitInfo(string clientId, string endpoint);
    Task IncrementRequestCount(string clientId, string endpoint);
    Task ResetRateLimit(string clientId, string endpoint);
}

public interface IApiVersioningService
{
    string GetRequestedVersion(HttpRequest request);
    bool IsVersionSupported(string version, string endpoint);
    string GetDefaultVersion();
    Task<IEnumerable<string>> GetSupportedVersions(string endpoint);
}

public interface IRequestTransformationService
{
    Task<T?> TransformRequest<T>(HttpRequest request, string sourceFormat, string targetFormat);
    Task<object?> TransformResponse(object response, string sourceFormat, string targetFormat);
    Task<bool> ValidateRequestFormat(HttpRequest request, string expectedFormat);
}

public class RateLimitInfo
{
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetTime { get; set; }
    public bool IsAllowed { get; set; }
} 