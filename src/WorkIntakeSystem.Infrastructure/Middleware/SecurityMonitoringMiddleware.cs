using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Infrastructure.Middleware;

public class SecurityMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityMonitoringMiddleware> _logger;
    private readonly ISecurityMonitoringService _securityMonitoringService;

    public SecurityMonitoringMiddleware(
        RequestDelegate next,
        ILogger<SecurityMonitoringMiddleware> logger,
        ISecurityMonitoringService securityMonitoringService)
    {
        _next = next;
        _logger = logger;
        _securityMonitoringService = securityMonitoringService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString();
        
        // Add correlation ID to response headers
        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        try
        {
            // Extract security context
            var securityEvent = await ExtractSecurityEventAsync(context, correlationId);
            
            // Process the request
            await _next(context);
            
            // Update security event with response information
            securityEvent.Details = $"Request completed with status {context.Response.StatusCode}";
            
            // Parse existing metadata and add response information
            var metadata = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(securityEvent.Metadata))
            {
                try
                {
                    metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(securityEvent.Metadata) ?? new Dictionary<string, object>();
                }
                catch
                {
                    metadata = new Dictionary<string, object>();
                }
            }
            
            metadata["ResponseTime"] = stopwatch.ElapsedMilliseconds;
            metadata["StatusCode"] = context.Response.StatusCode;
            securityEvent.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
            
            // Log the security event
            await _securityMonitoringService.DetectSuspiciousActivityAsync(securityEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in security monitoring middleware");
            
            // Log security event for the exception
            var securityEvent = await ExtractSecurityEventAsync(context, correlationId);
            securityEvent.EventType = "SystemError";
            securityEvent.Severity = "High";
            securityEvent.Details = $"Exception occurred: {ex.Message}";
            
            // Parse existing metadata and add exception information
            var metadata = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(securityEvent.Metadata))
            {
                try
                {
                    metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(securityEvent.Metadata) ?? new Dictionary<string, object>();
                }
                catch
                {
                    metadata = new Dictionary<string, object>();
                }
            }
            
            metadata["ExceptionType"] = ex.GetType().Name;
            metadata["StackTrace"] = ex.StackTrace ?? string.Empty;
            securityEvent.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
            
            await _securityMonitoringService.DetectSuspiciousActivityAsync(securityEvent);
            
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private async Task<SecurityEvent> ExtractSecurityEventAsync(HttpContext context, string correlationId)
    {
        var userIdString = GetUserId(context);
        var userId = int.TryParse(userIdString, out var parsedUserId) ? parsedUserId : 0;
        var ipAddress = GetClientIPAddress(context);
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;
        var sessionId = context.Session?.Id ?? string.Empty;
        
        var securityEvent = new SecurityEvent
        {
            EventType = GetEventType(context),
            UserId = userId,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            SessionId = sessionId,
            Resource = $"{context.Request.Method} {context.Request.Path}",
            Action = GetAction(context),
            Details = GetRequestDetails(context),
            Timestamp = DateTime.UtcNow,
            Severity = GetSeverity(context),
            CorrelationId = correlationId,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "RequestId", context.TraceIdentifier },
                { "QueryString", context.Request.QueryString.ToString() },
                { "ContentType", context.Request.ContentType ?? string.Empty },
                { "ContentLength", context.Request.ContentLength ?? 0 },
                { "IsHttps", context.Request.IsHttps },
                { "Host", context.Request.Host.ToString() },
                { "UserRoles", string.Join(", ", GetUserRoles(context)) }
            })
        };

        return securityEvent;
    }

    private string GetUserId(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                   user.FindFirst("sub")?.Value ?? 
                   user.Identity.Name ?? "Unknown";
        }
        return "Anonymous";
    }

    private string GetClientIPAddress(HttpContext context)
    {
        // Check for forwarded IP first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIP))
        {
            return realIP;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetEventType(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        var method = context.Request.Method.ToUpper();

        // Authentication events
        if (path.Contains("/auth") || path.Contains("/login"))
        {
            return method == "POST" ? "Login" : "Authentication";
        }

        if (path.Contains("/logout"))
        {
            return "Logout";
        }

        // Data access events
        if (method == "GET" && (path.Contains("/api/") || path.Contains("/data")))
        {
            return "DataAccess";
        }

        // Data modification events
        if ((method == "POST" || method == "PUT" || method == "PATCH" || method == "DELETE") && path.Contains("/api/"))
        {
            return "DataModification";
        }

        // Permission changes
        if (path.Contains("/permissions") || path.Contains("/roles"))
        {
            return "PermissionChange";
        }

        // System access
        if (path.Contains("/admin") || path.Contains("/system"))
        {
            return "SystemAccess";
        }

        // Configuration changes
        if (path.Contains("/config") || path.Contains("/settings"))
        {
            return "ConfigurationChange";
        }

        return "SystemAccess";
    }

    private string GetAction(HttpContext context)
    {
        var method = context.Request.Method.ToUpper();
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        return method switch
        {
            "GET" => "Read",
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "PartialUpdate",
            "DELETE" => "Delete",
            _ => "Unknown"
        };
    }

    private string GetRequestDetails(HttpContext context)
    {
        var details = new List<string>
        {
            $"Method: {context.Request.Method}",
            $"Path: {context.Request.Path}",
            $"Query: {context.Request.QueryString}",
            $"Content-Type: {context.Request.ContentType}",
            $"Content-Length: {context.Request.ContentLength}"
        };

        // Add sensitive headers (without values)
        var sensitiveHeaders = new[] { "Authorization", "Cookie", "X-API-Key" };
        foreach (var header in sensitiveHeaders)
        {
            if (context.Request.Headers.ContainsKey(header))
            {
                details.Add($"{header}: [REDACTED]");
            }
        }

        return string.Join(", ", details);
    }

    private string GetSeverity(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        var method = context.Request.Method.ToUpper();

        // High severity operations
        if (method == "DELETE" || 
            path.Contains("/admin") || 
            path.Contains("/system") ||
            path.Contains("/security") ||
            path.Contains("/permissions"))
        {
            return "High";
        }

        // Medium severity operations
        if (method == "POST" || method == "PUT" || method == "PATCH" ||
            path.Contains("/config") || path.Contains("/settings"))
        {
            return "Medium";
        }

        // Low severity operations
        return "Low";
    }

    private List<string> GetUserRoles(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.FindAll(ClaimTypes.Role)
                      .Select(c => c.Value)
                      .ToList();
        }
        return new List<string>();
    }
}
