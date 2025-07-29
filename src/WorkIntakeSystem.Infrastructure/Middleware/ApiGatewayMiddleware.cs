using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Middleware;

public class ApiGatewayMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiGatewayMiddleware> _logger;
    private readonly IApiGatewayService _apiGatewayService;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly IApiVersioningService _versioningService;

    public ApiGatewayMiddleware(
        RequestDelegate next,
        ILogger<ApiGatewayMiddleware> logger,
        IApiGatewayService apiGatewayService,
        IRateLimitingService rateLimitingService,
        IApiVersioningService versioningService)
    {
        _next = next;
        _logger = logger;
        _apiGatewayService = apiGatewayService;
        _rateLimitingService = rateLimitingService;
        _versioningService = versioningService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;
        var clientId = GetClientId(context);

        try
        {
            // Skip API Gateway processing for health checks and swagger
            if (ShouldSkipProcessing(endpoint))
            {
                await _next(context);
                return;
            }

            // 1. Validate API Version
            var requestHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => (object)h.Value.ToString());
            var requestedVersion = _versioningService.GetRequestedVersion(requestHeaders);
            if (!await _apiGatewayService.ValidateApiVersion(requestedVersion, endpoint))
            {
                await WriteErrorResponse(context, 400, "Unsupported API version", 
                    $"Version {requestedVersion} is not supported for endpoint {endpoint}");
                return;
            }

            // 2. Check Rate Limiting
            if (!await _apiGatewayService.ValidateRateLimit(clientId, endpoint))
            {
                var rateLimitInfo = await _rateLimitingService.GetRateLimitInfo(clientId, endpoint);
                
                // Add rate limit headers
                context.Response.Headers.Add("X-RateLimit-Limit", rateLimitInfo.Limit.ToString());
                context.Response.Headers.Add("X-RateLimit-Remaining", rateLimitInfo.Remaining.ToString());
                context.Response.Headers.Add("X-RateLimit-Reset", new DateTimeOffset(rateLimitInfo.ResetTime).ToUnixTimeSeconds().ToString());

                await WriteErrorResponse(context, 429, "Rate limit exceeded", 
                    "Too many requests. Please try again later.");
                return;
            }

            // 3. Check Endpoint Access
            if (!await _apiGatewayService.IsEndpointAllowed(endpoint, clientId))
            {
                await WriteErrorResponse(context, 403, "Access denied", 
                    "You don't have permission to access this endpoint");
                return;
            }

            // 4. Increment request count for rate limiting
            await _rateLimitingService.IncrementRequestCount(clientId, endpoint);

            // 5. Add API version header to response
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Add("X-Api-Version", requestedVersion);
                return Task.CompletedTask;
            });

            // 6. Process the request
            await _next(context);

            // 7. Log API call
            stopwatch.Stop();
            await _apiGatewayService.LogApiCall(clientId, endpoint, method, 
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in API Gateway middleware for endpoint {Endpoint}", endpoint);
            
            stopwatch.Stop();
            await _apiGatewayService.LogApiCall(clientId, endpoint, method, 500, stopwatch.ElapsedMilliseconds);
            
            if (!context.Response.HasStarted)
            {
                await WriteErrorResponse(context, 500, "Internal server error", 
                    "An error occurred while processing your request");
            }
        }
    }

    private static string GetClientId(HttpContext context)
    {
        // Try to get client ID from various sources
        
        // 1. From authentication claims
        var userIdentity = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userIdentity))
        {
            return userIdentity;
        }

        // 2. From custom header
        if (context.Request.Headers.TryGetValue("X-Client-Id", out var clientIdHeader))
        {
            return clientIdHeader.FirstOrDefault() ?? "anonymous";
        }

        // 3. From IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return $"ip_{ipAddress}";
        }

        return "anonymous";
    }

    private static bool ShouldSkipProcessing(string endpoint)
    {
        var skipPaths = new[]
        {
            "/health",
            "/swagger",
            "/api-docs",
            "/favicon.ico"
        };

        return skipPaths.Any(path => endpoint.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string error, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            error,
            message,
            statusCode,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(jsonResponse);
    }
} 