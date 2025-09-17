using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class ApplicationPerformanceMonitoringService : IApplicationPerformanceMonitoringService
    {
        private readonly ILogger<ApplicationPerformanceMonitoringService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMonitoringService _monitoringService;
        private readonly bool _apmEnabled;
        private readonly Dictionary<string, object> _customProperties;

        public ApplicationPerformanceMonitoringService(
            ILogger<ApplicationPerformanceMonitoringService> logger,
            IConfiguration configuration,
            IMonitoringService monitoringService)
        {
            _logger = logger;
            _configuration = configuration;
            _monitoringService = monitoringService;
            _apmEnabled = configuration.GetValue<bool>("Monitoring:APM:Enabled", true);
            _customProperties = new Dictionary<string, object>();
        }

        #region Metric Tracking

        public async Task TrackMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.TrackMetricAsync(metricName, value, tags);
                _logger.LogDebug("Tracked APM metric: {MetricName} = {Value}", metricName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking APM metric: {MetricName}", metricName);
            }
        }

        public async Task IncrementCounterAsync(string counterName, Dictionary<string, string>? tags = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.IncrementCounterAsync(counterName, tags);
                _logger.LogDebug("Incremented APM counter: {CounterName}", counterName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing APM counter: {CounterName}", counterName);
            }
        }

        public async Task SetGaugeAsync(string gaugeName, double value, Dictionary<string, string>? tags = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.SetGaugeAsync(gaugeName, value, tags);
                _logger.LogDebug("Set APM gauge: {GaugeName} = {Value}", gaugeName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting APM gauge: {GaugeName}", gaugeName);
            }
        }

        public async Task RecordHistogramAsync(string histogramName, double value, Dictionary<string, string>? tags = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.RecordHistogramAsync(histogramName, value, tags);
                _logger.LogDebug("Recorded APM histogram: {HistogramName} = {Value}", histogramName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording APM histogram: {HistogramName}", histogramName);
            }
        }

        #endregion

        #region Dependency Tracking

        public async Task TrackDependencyAsync(string dependencyType, string target, TimeSpan duration, bool success, Dictionary<string, string>? properties = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.TrackDependencyAsync(dependencyType, target, duration, success);
                
                var eventProperties = properties ?? new Dictionary<string, string>();
                eventProperties["dependency_type"] = dependencyType;
                eventProperties["target"] = target;
                eventProperties["duration_ms"] = duration.TotalMilliseconds.ToString();
                eventProperties["success"] = success.ToString();
                
                await TrackEventAsync($"Dependency_{dependencyType}", eventProperties);
                
                _logger.LogDebug("Tracked APM dependency: {Type} -> {Target} ({Duration}ms, Success: {Success})", 
                    dependencyType, target, duration.TotalMilliseconds, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking APM dependency: {Type} -> {Target}", dependencyType, target);
            }
        }

        public async Task TrackHttpDependencyAsync(string method, string url, TimeSpan duration, int statusCode, bool success)
        {
            if (!_apmEnabled) return;

            try
            {
                var properties = new Dictionary<string, string>
                {
                    ["method"] = method,
                    ["url"] = url,
                    ["status_code"] = statusCode.ToString(),
                    ["success"] = success.ToString()
                };

                await TrackDependencyAsync("HTTP", url, duration, success, properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking HTTP dependency: {Method} {Url}", method, url);
            }
        }

        public async Task TrackDatabaseDependencyAsync(string databaseName, string command, TimeSpan duration, bool success)
        {
            if (!_apmEnabled) return;

            try
            {
                var properties = new Dictionary<string, string>
                {
                    ["database_name"] = databaseName,
                    ["command"] = command,
                    ["success"] = success.ToString()
                };

                await TrackDependencyAsync("Database", databaseName, duration, success, properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking database dependency: {Database} - {Command}", databaseName, command);
            }
        }

        public async Task TrackExternalServiceDependencyAsync(string serviceName, string operation, TimeSpan duration, bool success)
        {
            if (!_apmEnabled) return;

            try
            {
                var properties = new Dictionary<string, string>
                {
                    ["service_name"] = serviceName,
                    ["operation"] = operation,
                    ["success"] = success.ToString()
                };

                await TrackDependencyAsync("ExternalService", serviceName, duration, success, properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking external service dependency: {Service} - {Operation}", serviceName, operation);
            }
        }

        #endregion

        #region Exception Tracking

        public async Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.TrackExceptionAsync(exception, properties);
                
                var eventProperties = properties ?? new Dictionary<string, string>();
                eventProperties["exception_type"] = exception.GetType().Name;
                eventProperties["exception_message"] = exception.Message;
                eventProperties["exception_source"] = exception.Source ?? "Unknown";
                
                await TrackEventAsync("Exception", eventProperties);
                
                _logger.LogError(exception, "Tracked APM exception: {ExceptionType}", exception.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking APM exception");
            }
        }

        public async Task TrackExceptionAsync(Exception exception, string context, Dictionary<string, string>? properties = null)
        {
            if (!_apmEnabled) return;

            try
            {
                var eventProperties = properties ?? new Dictionary<string, string>();
                eventProperties["context"] = context;
                
                await TrackExceptionAsync(exception, eventProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking APM exception with context: {Context}", context);
            }
        }

        #endregion

        #region Event Tracking

        public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.TrackEventAsync(eventName, properties);
                _logger.LogDebug("Tracked APM event: {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking APM event: {EventName}", eventName);
            }
        }

        public async Task TrackUserActionAsync(string userId, string action, Dictionary<string, string>? properties = null)
        {
            if (!_apmEnabled) return;

            try
            {
                var eventProperties = properties ?? new Dictionary<string, string>();
                eventProperties["user_id"] = userId;
                eventProperties["action"] = action;
                
                await TrackEventAsync("UserAction", eventProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user action: {UserId} - {Action}", userId, action);
            }
        }

        public async Task TrackBusinessEventAsync(string eventName, Dictionary<string, object>? properties = null)
        {
            if (!_apmEnabled) return;

            try
            {
                var eventProperties = new Dictionary<string, string>();
                if (properties != null)
                {
                    foreach (var kvp in properties)
                    {
                        eventProperties[kvp.Key] = kvp.Value?.ToString() ?? "null";
                    }
                }
                
                await TrackEventAsync($"Business_{eventName}", eventProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking business event: {EventName}", eventName);
            }
        }

        #endregion

        #region Performance Tracking

        public async Task<IDisposable> TrackOperationAsync(string operationName, Dictionary<string, string>? properties = null)
        {
            if (!_apmEnabled) return new NoOpTracker();

            try
            {
                var tracker = new OperationTracker(this, operationName, properties);
                await TrackEventAsync($"Operation_Start_{operationName}", properties);
                return tracker;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting operation tracking: {OperationName}", operationName);
                return new NoOpTracker();
            }
        }

        public async Task TrackPerformanceAsync(string operationName, TimeSpan duration, Dictionary<string, string>? properties = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await TrackMetricAsync($"performance_{operationName}_duration", duration.TotalMilliseconds, properties);
                await TrackEventAsync($"Performance_{operationName}", properties);
                
                _logger.LogDebug("Tracked performance: {OperationName} took {Duration}ms", 
                    operationName, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking performance: {OperationName}", operationName);
            }
        }

        public async Task TrackMemoryUsageAsync(long memoryUsage, Dictionary<string, string>? tags = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await TrackMetricAsync("memory_usage_bytes", memoryUsage, tags);
                await TrackMetricAsync("memory_usage_mb", memoryUsage / 1024.0 / 1024.0, tags);
                
                _logger.LogDebug("Tracked memory usage: {MemoryUsage} bytes", memoryUsage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking memory usage");
            }
        }

        public async Task TrackCpuUsageAsync(double cpuUsage, Dictionary<string, string>? tags = null)
        {
            if (!_apmEnabled) return;

            try
            {
                await TrackMetricAsync("cpu_usage_percent", cpuUsage, tags);
                
                _logger.LogDebug("Tracked CPU usage: {CpuUsage}%", cpuUsage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking CPU usage");
            }
        }

        #endregion

        #region Custom Context

        public async Task SetCustomPropertyAsync(string key, string value)
        {
            if (!_apmEnabled) return;

            try
            {
                _customProperties[key] = value;
                await TrackEventAsync("CustomProperty_Set", new Dictionary<string, string> { [key] = value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting custom property: {Key}", key);
            }
        }

        public async Task SetCustomPropertyAsync(string key, object value)
        {
            if (!_apmEnabled) return;

            try
            {
                _customProperties[key] = value;
                await TrackEventAsync("CustomProperty_Set", new Dictionary<string, string> { [key] = value?.ToString() ?? "null" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting custom property: {Key}", key);
            }
        }

        public async Task ClearCustomPropertiesAsync()
        {
            if (!_apmEnabled) return;

            try
            {
                _customProperties.Clear();
                await TrackEventAsync("CustomProperty_Clear", new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing custom properties");
            }
        }

        #endregion

        #region Flush and Configuration

        public async Task FlushAsync()
        {
            if (!_apmEnabled) return;

            try
            {
                await _monitoringService.SendToElasticsearchAsync("apm_flush", new { timestamp = DateTime.UtcNow });
                _logger.LogDebug("APM data flushed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing APM data");
            }
        }

        public async Task<bool> IsEnabledAsync()
        {
            return _apmEnabled;
        }

        public async Task SetEnabledAsync(bool enabled)
        {
            // This would typically update configuration
            _logger.LogInformation("APM enabled status changed to: {Enabled}", enabled);
        }

        #endregion

        #region Private Classes

        private class OperationTracker : IOperationTracker
        {
            private readonly ApplicationPerformanceMonitoringService _apmService;
            private readonly string _operationName;
            private readonly Dictionary<string, string> _properties;
            private readonly Stopwatch _stopwatch;
            private bool _success = true;
            private string _result = "Unknown";

            public OperationTracker(ApplicationPerformanceMonitoringService apmService, string operationName, Dictionary<string, string> properties)
            {
                _apmService = apmService;
                _operationName = operationName;
                _properties = properties ?? new Dictionary<string, string>();
                _stopwatch = Stopwatch.StartNew();
            }

            public void SetSuccess(bool success)
            {
                _success = success;
            }

            public void SetResult(string result)
            {
                _result = result;
            }

            public void AddProperty(string key, string value)
            {
                _properties[key] = value;
            }

            public void AddProperty(string key, object value)
            {
                _properties[key] = value?.ToString() ?? "null";
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                
                try
                {
                    _properties["duration_ms"] = _stopwatch.Elapsed.TotalMilliseconds.ToString();
                    _properties["success"] = _success.ToString();
                    _properties["result"] = _result;
                    
                    _apmService.TrackPerformanceAsync(_operationName, _stopwatch.Elapsed, _properties).Wait();
                    _apmService.TrackEventAsync($"Operation_End_{_operationName}", _properties).Wait();
                }
                catch (Exception ex)
                {
                    _apmService._logger.LogError(ex, "Error completing operation tracking: {OperationName}", _operationName);
                }
            }
        }

        private class NoOpTracker : IOperationTracker
        {
            public void SetSuccess(bool success) { }
            public void SetResult(string result) { }
            public void AddProperty(string key, string value) { }
            public void AddProperty(string key, object value) { }
            public void Dispose() { }
        }

        #endregion
    }
} 