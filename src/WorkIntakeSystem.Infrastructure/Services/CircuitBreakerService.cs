using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class CircuitBreakerService : ICircuitBreakerService
{
    private readonly ILogger<CircuitBreakerService> _logger;
    private readonly Dictionary<string, CircuitBreakerState> _circuitBreakers;
    private readonly Dictionary<string, CircuitBreakerMetrics> _metrics;
    private readonly object _circuitBreakersLock = new();
    private readonly object _metricsLock = new();

    public CircuitBreakerService(ILogger<CircuitBreakerService> logger)
    {
        _logger = logger;
        _circuitBreakers = new Dictionary<string, CircuitBreakerState>();
        _metrics = new Dictionary<string, CircuitBreakerMetrics>();
    }

    public async Task<T> ExecuteWithCircuitBreakerAsync<T>(string serviceName, Func<Task<T>> operation)
    {
        var circuitBreaker = GetOrCreateCircuitBreaker(serviceName);
        var metrics = GetOrCreateMetrics(serviceName);

        // Check if circuit breaker is open
        if (circuitBreaker.State == "Open")
        {
            if (DateTime.UtcNow < circuitBreaker.NextAttemptTime)
            {
                _logger.LogWarning("Circuit breaker is open for service {ServiceName}, request rejected", serviceName);
                UpdateMetrics(metrics, false, 0, true);
                throw new CircuitBreakerOpenException($"Circuit breaker is open for service {serviceName}");
            }
            else
            {
                // Time to try half-open
                circuitBreaker.State = "HalfOpen";
                _logger.LogInformation("Circuit breaker transitioning to half-open for service {ServiceName}", serviceName);
            }
        }

        var startTime = DateTime.UtcNow;
        var success = false;

        try
        {
            var result = await operation();
            success = true;
            var duration = DateTime.UtcNow - startTime;

            // Update circuit breaker state
            if (circuitBreaker.State == "HalfOpen")
            {
                // Success in half-open state, close the circuit breaker
                circuitBreaker.State = "Closed";
                circuitBreaker.FailureCount = 0;
                _logger.LogInformation("Circuit breaker closed for service {ServiceName} after successful half-open test", serviceName);
            }

            UpdateMetrics(metrics, success, duration.TotalMilliseconds, false);
            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(metrics, success, duration.TotalMilliseconds, false);
            
            // Record failure
            circuitBreaker.FailureCount++;
            circuitBreaker.LastFailureTime = DateTime.UtcNow;

            _logger.LogWarning(ex, "Operation failed for service {ServiceName}, failure count: {FailureCount}", 
                serviceName, circuitBreaker.FailureCount);

            // Check if we should open the circuit breaker
            if (circuitBreaker.FailureCount >= circuitBreaker.Threshold)
            {
                circuitBreaker.State = "Open";
                circuitBreaker.NextAttemptTime = DateTime.UtcNow.Add(circuitBreaker.Timeout);
                _logger.LogError("Circuit breaker opened for service {ServiceName} after {FailureCount} failures", 
                    serviceName, circuitBreaker.FailureCount);
            }

            throw;
        }
    }

    public async Task<bool> IsServiceAvailableAsync(string serviceName)
    {
        var status = await GetCircuitBreakerStatusAsync(serviceName);
        return status.State == "Closed";
    }

    public async Task<CircuitBreakerStatus> GetCircuitBreakerStatusAsync(string serviceName)
    {
        var circuitBreaker = GetOrCreateCircuitBreaker(serviceName);
        
        return new CircuitBreakerStatus
        {
            ServiceName = serviceName,
            State = circuitBreaker.State,
            FailureCount = circuitBreaker.FailureCount,
            LastFailureTime = circuitBreaker.LastFailureTime,
            NextAttemptTime = circuitBreaker.NextAttemptTime,
            Threshold = circuitBreaker.Threshold,
            Timeout = circuitBreaker.Timeout
        };
    }

    public async Task<bool> ResetCircuitBreakerAsync(string serviceName)
    {
        try
        {
            lock (_circuitBreakersLock)
            {
                if (_circuitBreakers.TryGetValue(serviceName, out var circuitBreaker))
                {
                    circuitBreaker.State = "Closed";
                    circuitBreaker.FailureCount = 0;
                    circuitBreaker.LastFailureTime = DateTime.UtcNow;
                    circuitBreaker.NextAttemptTime = DateTime.UtcNow;
                }
            }

            _logger.LogInformation("Circuit breaker reset for service {ServiceName}", serviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset circuit breaker for service {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<bool> SetCircuitBreakerThresholdAsync(string serviceName, int threshold)
    {
        try
        {
            var circuitBreaker = GetOrCreateCircuitBreaker(serviceName);
            circuitBreaker.Threshold = threshold;
            
            _logger.LogInformation("Circuit breaker threshold updated for service {ServiceName}: {Threshold}", 
                serviceName, threshold);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set circuit breaker threshold for service {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<bool> SetCircuitBreakerTimeoutAsync(string serviceName, TimeSpan timeout)
    {
        try
        {
            var circuitBreaker = GetOrCreateCircuitBreaker(serviceName);
            circuitBreaker.Timeout = timeout;
            
            _logger.LogInformation("Circuit breaker timeout updated for service {ServiceName}: {Timeout}", 
                serviceName, timeout);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set circuit breaker timeout for service {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<IEnumerable<CircuitBreakerStatus>> GetAllCircuitBreakerStatusesAsync()
    {
        var statuses = new List<CircuitBreakerStatus>();
        
        lock (_circuitBreakersLock)
        {
            foreach (var kvp in _circuitBreakers)
            {
                var circuitBreaker = kvp.Value;
                statuses.Add(new CircuitBreakerStatus
                {
                    ServiceName = kvp.Key,
                    State = circuitBreaker.State,
                    FailureCount = circuitBreaker.FailureCount,
                    LastFailureTime = circuitBreaker.LastFailureTime,
                    NextAttemptTime = circuitBreaker.NextAttemptTime,
                    Threshold = circuitBreaker.Threshold,
                    Timeout = circuitBreaker.Timeout
                });
            }
        }

        return statuses;
    }

    public async Task<CircuitBreakerMetrics> GetCircuitBreakerMetricsAsync(string serviceName)
    {
        var metrics = GetOrCreateMetrics(serviceName);
        
        lock (_metricsLock)
        {
            return new CircuitBreakerMetrics
            {
                ServiceName = serviceName,
                TotalRequests = metrics.TotalRequests,
                SuccessfulRequests = metrics.SuccessfulRequests,
                FailedRequests = metrics.FailedRequests,
                CircuitOpenCount = metrics.CircuitOpenCount,
                AverageResponseTime = metrics.AverageResponseTime,
                LastReset = metrics.LastReset
            };
        }
    }

    private CircuitBreakerState GetOrCreateCircuitBreaker(string serviceName)
    {
        lock (_circuitBreakersLock)
        {
            if (!_circuitBreakers.TryGetValue(serviceName, out var circuitBreaker))
            {
                circuitBreaker = new CircuitBreakerState
                {
                    State = "Closed",
                    FailureCount = 0,
                    LastFailureTime = DateTime.UtcNow,
                    NextAttemptTime = DateTime.UtcNow,
                    Threshold = 5,
                    Timeout = TimeSpan.FromMinutes(1)
                };
                _circuitBreakers[serviceName] = circuitBreaker;
            }
            return circuitBreaker;
        }
    }

    private CircuitBreakerMetrics GetOrCreateMetrics(string serviceName)
    {
        lock (_metricsLock)
        {
            if (!_metrics.TryGetValue(serviceName, out var metrics))
            {
                metrics = new CircuitBreakerMetrics
                {
                    ServiceName = serviceName,
                    TotalRequests = 0,
                    SuccessfulRequests = 0,
                    FailedRequests = 0,
                    CircuitOpenCount = 0,
                    AverageResponseTime = 0,
                    LastReset = DateTime.UtcNow
                };
                _metrics[serviceName] = metrics;
            }
            return metrics;
        }
    }

    private void UpdateMetrics(CircuitBreakerMetrics metrics, bool success, double responseTime, bool circuitOpen)
    {
        lock (_metricsLock)
        {
            metrics.TotalRequests++;
            
            if (success)
            {
                metrics.SuccessfulRequests++;
            }
            else
            {
                metrics.FailedRequests++;
            }

            if (circuitOpen)
            {
                metrics.CircuitOpenCount++;
            }

            // Update average response time
            if (metrics.SuccessfulRequests > 0)
            {
                var totalTime = metrics.AverageResponseTime * (metrics.SuccessfulRequests - 1) + responseTime;
                metrics.AverageResponseTime = totalTime / metrics.SuccessfulRequests;
            }
        }
    }

    private class CircuitBreakerState
    {
        public string State { get; set; } = "Closed";
        public int FailureCount { get; set; } = 0;
        public DateTime LastFailureTime { get; set; } = DateTime.UtcNow;
        public DateTime NextAttemptTime { get; set; } = DateTime.UtcNow;
        public int Threshold { get; set; } = 5;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
    }
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message)
    {
    }
} 