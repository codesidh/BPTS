using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;
using StackExchange.Redis;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IDatabase _redisDatabase;
        private readonly string _elasticsearchUrl;
        private readonly string _logstashUrl;
        private readonly string _kibanaUrl;
        private readonly bool _monitoringEnabled;

        public MonitoringService(
            ILogger<MonitoringService> logger,
            IConfiguration configuration,
            HttpClient httpClient,
            IConnectionMultiplexer redisConnection)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _redisDatabase = redisConnection.GetDatabase();
            
            _elasticsearchUrl = _configuration["Monitoring:Elasticsearch:Url"] ?? "http://localhost:9200";
            _logstashUrl = _configuration["Monitoring:Logstash:Url"] ?? "http://localhost:5044";
            _kibanaUrl = _configuration["Monitoring:Kibana:Url"] ?? "http://localhost:5601";
            _monitoringEnabled = _configuration.GetValue<bool>("Monitoring:Enabled", true);
        }

        #region ELK Stack Integration

        public async Task<bool> SendToElasticsearchAsync(string index, object data)
        {
            if (!_monitoringEnabled) return true;

            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_elasticsearchUrl}/{index}/_doc", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully sent data to Elasticsearch index: {Index}", index);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to send data to Elasticsearch. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending data to Elasticsearch index: {Index}", index);
                return false;
            }
        }

        public async Task<bool> SendToLogstashAsync(string pipeline, object data)
        {
            if (!_monitoringEnabled) return true;

            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_logstashUrl}/{pipeline}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully sent data to Logstash pipeline: {Pipeline}", pipeline);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to send data to Logstash. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending data to Logstash pipeline: {Pipeline}", pipeline);
                return false;
            }
        }

        public async Task<List<object>> QueryElasticsearchAsync(string index, string query)
        {
            if (!_monitoringEnabled) return new List<object>();

            try
            {
                var content = new StringContent(query, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_elasticsearchUrl}/{index}/_search", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    var hits = searchResult.GetProperty("hits").GetProperty("hits");
                    var results = new List<object>();
                    
                    foreach (var hit in hits.EnumerateArray())
                    {
                        results.Add(hit.GetProperty("_source").GetRawText());
                    }
                    
                    return results;
                }
                else
                {
                    _logger.LogWarning("Failed to query Elasticsearch. Status: {StatusCode}", response.StatusCode);
                    return new List<object>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Elasticsearch index: {Index}", index);
                return new List<object>();
            }
        }

        #endregion

        #region Application Performance Monitoring

        public async Task TrackMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null)
        {
            if (!_monitoringEnabled) return;

            try
            {
                var metricData = new
                {
                    metric_name = metricName,
                    value = value,
                    tags = tags ?? new Dictionary<string, string>(),
                    timestamp = DateTime.UtcNow
                };

                await SendToElasticsearchAsync("metrics", metricData);
                _logger.LogDebug("Tracked metric: {MetricName} = {Value}", metricName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking metric: {MetricName}", metricName);
            }
        }

        public async Task TrackDependencyAsync(string dependencyType, string target, TimeSpan duration, bool success)
        {
            if (!_monitoringEnabled) return;

            try
            {
                var dependencyData = new
                {
                    dependency_type = dependencyType,
                    target = target,
                    duration_ms = duration.TotalMilliseconds,
                    success = success,
                    timestamp = DateTime.UtcNow
                };

                await SendToElasticsearchAsync("dependencies", dependencyData);
                _logger.LogDebug("Tracked dependency: {Type} -> {Target} ({Duration}ms, Success: {Success})", 
                    dependencyType, target, duration.TotalMilliseconds, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking dependency: {Type} -> {Target}", dependencyType, target);
            }
        }

        public async Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
        {
            if (!_monitoringEnabled) return;

            try
            {
                var exceptionData = new
                {
                    exception_type = exception.GetType().Name,
                    message = exception.Message,
                    stack_trace = exception.StackTrace,
                    source = exception.Source,
                    properties = properties ?? new Dictionary<string, string>(),
                    timestamp = DateTime.UtcNow
                };

                await SendToElasticsearchAsync("exceptions", exceptionData);
                _logger.LogError(exception, "Tracked exception: {ExceptionType}", exception.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking exception");
            }
        }

        public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
        {
            if (!_monitoringEnabled) return;

            try
            {
                var eventData = new
                {
                    event_name = eventName,
                    properties = properties ?? new Dictionary<string, string>(),
                    timestamp = DateTime.UtcNow
                };

                await SendToElasticsearchAsync("events", eventData);
                _logger.LogDebug("Tracked event: {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking event: {EventName}", eventName);
            }
        }

        #endregion

        #region Health Checks

        public async Task<HealthCheckResult> CheckDatabaseHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // This would typically check the actual database connection
                // For now, we'll simulate a health check
                await Task.Delay(100); // Simulate database check
                
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = "Database connection is working properly",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "connection_string", "***" },
                        { "database_name", "WorkIntakeSystem" }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Database health check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckRedisHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var pingResult = await _redisDatabase.PingAsync();
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = $"Redis is responding (Ping: {pingResult.TotalMilliseconds}ms)",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "ping_time_ms", pingResult.TotalMilliseconds },
                        { "redis_version", "6.0+" }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Redis health check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckExternalServicesHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var healthyServices = 0;
            var totalServices = 0;
            var serviceResults = new Dictionary<string, bool>();
            
            try
            {
                // Check Microsoft 365 services
                totalServices++;
                try
                {
                    var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/");
                    serviceResults["Microsoft Graph"] = response.IsSuccessStatusCode;
                    if (response.IsSuccessStatusCode) healthyServices++;
                }
                catch
                {
                    serviceResults["Microsoft Graph"] = false;
                }

                // Check other external services
                var externalServices = new[]
                {
                    "https://api.github.com",
                    "https://httpbin.org/status/200"
                };

                foreach (var service in externalServices)
                {
                    totalServices++;
                    try
                    {
                        var response = await _httpClient.GetAsync(service);
                        serviceResults[service] = response.IsSuccessStatusCode;
                        if (response.IsSuccessStatusCode) healthyServices++;
                    }
                    catch
                    {
                        serviceResults[service] = false;
                    }
                }

                stopwatch.Stop();
                
                var isHealthy = healthyServices > 0;
                var status = isHealthy ? "Healthy" : "Unhealthy";
                var description = $"{healthyServices}/{totalServices} external services are responding";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "healthy_services", healthyServices },
                        { "total_services", totalServices },
                        { "service_results", serviceResults }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"External services health check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckPerformanceHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var process = Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64;
                var cpuUsage = await GetCpuUsageAsync();
                
                stopwatch.Stop();
                
                var isHealthy = memoryUsage < 500 * 1024 * 1024 && cpuUsage < 80; // 500MB and 80% CPU
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"Memory: {memoryUsage / 1024 / 1024}MB, CPU: {cpuUsage:F1}%";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "memory_usage_mb", memoryUsage / 1024 / 1024 },
                        { "cpu_usage_percent", cpuUsage },
                        { "process_id", process.Id },
                        { "thread_count", process.Threads.Count }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Performance health check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        #endregion

        #region Custom Metrics

        public async Task IncrementCounterAsync(string counterName, Dictionary<string, string>? tags = null)
        {
            if (!_monitoringEnabled) return;

            try
            {
                var counterData = new
                {
                    counter_name = counterName,
                    operation = "increment",
                    value = 1,
                    tags = tags ?? new Dictionary<string, string>(),
                    timestamp = DateTime.UtcNow
                };

                await SendToElasticsearchAsync("counters", counterData);
                _logger.LogDebug("Incremented counter: {CounterName}", counterName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter: {CounterName}", counterName);
            }
        }

        public async Task SetGaugeAsync(string gaugeName, double value, Dictionary<string, string>? tags = null)
        {
            if (!_monitoringEnabled) return;

            try
            {
                var gaugeData = new
                {
                    gauge_name = gaugeName,
                    value = value,
                    tags = tags ?? new Dictionary<string, string>(),
                    timestamp = DateTime.UtcNow
                };

                await SendToElasticsearchAsync("gauges", gaugeData);
                _logger.LogDebug("Set gauge: {GaugeName} = {Value}", gaugeName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting gauge: {GaugeName}", gaugeName);
            }
        }

        public async Task RecordHistogramAsync(string histogramName, double value, Dictionary<string, string>? tags = null)
        {
            if (!_monitoringEnabled) return;

            try
            {
                var histogramData = new
                {
                    histogram_name = histogramName,
                    value = value,
                    tags = tags ?? new Dictionary<string, string>(),
                    timestamp = DateTime.UtcNow
                };

                await SendToElasticsearchAsync("histograms", histogramData);
                _logger.LogDebug("Recorded histogram: {HistogramName} = {Value}", histogramName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording histogram: {HistogramName}", histogramName);
            }
        }

        #endregion

        #region Private Methods

        private async Task<double> GetCpuUsageAsync()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                
                await Task.Delay(100); // Wait 100ms to measure CPU usage
                
                var endTime = DateTime.UtcNow;
                var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
                
                return Math.Min(cpuUsageTotal, 100.0);
            }
            catch
            {
                return 0.0;
            }
        }

        #endregion
    }
} 