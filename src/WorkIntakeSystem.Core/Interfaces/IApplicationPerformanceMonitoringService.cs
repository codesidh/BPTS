using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IApplicationPerformanceMonitoringService
    {
        // Metric Tracking
        Task TrackMetricAsync(string metricName, double value, Dictionary<string, string> tags = null);
        Task IncrementCounterAsync(string counterName, Dictionary<string, string> tags = null);
        Task SetGaugeAsync(string gaugeName, double value, Dictionary<string, string> tags = null);
        Task RecordHistogramAsync(string histogramName, double value, Dictionary<string, string> tags = null);
        
        // Dependency Tracking
        Task TrackDependencyAsync(string dependencyType, string target, TimeSpan duration, bool success, Dictionary<string, string> properties = null);
        Task TrackHttpDependencyAsync(string method, string url, TimeSpan duration, int statusCode, bool success);
        Task TrackDatabaseDependencyAsync(string databaseName, string command, TimeSpan duration, bool success);
        Task TrackExternalServiceDependencyAsync(string serviceName, string operation, TimeSpan duration, bool success);
        
        // Exception Tracking
        Task TrackExceptionAsync(Exception exception, Dictionary<string, string> properties = null);
        Task TrackExceptionAsync(Exception exception, string context, Dictionary<string, string> properties = null);
        
        // Event Tracking
        Task TrackEventAsync(string eventName, Dictionary<string, string> properties = null);
        Task TrackUserActionAsync(string userId, string action, Dictionary<string, string> properties = null);
        Task TrackBusinessEventAsync(string eventName, Dictionary<string, object> properties = null);
        
        // Performance Tracking
        Task<IDisposable> TrackOperationAsync(string operationName, Dictionary<string, string> properties = null);
        Task TrackPerformanceAsync(string operationName, TimeSpan duration, Dictionary<string, string> properties = null);
        Task TrackMemoryUsageAsync(long memoryUsage, Dictionary<string, string> tags = null);
        Task TrackCpuUsageAsync(double cpuUsage, Dictionary<string, string> tags = null);
        
        // Custom Context
        Task SetCustomPropertyAsync(string key, string value);
        Task SetCustomPropertyAsync(string key, object value);
        Task ClearCustomPropertiesAsync();
        
        // Flush and Configuration
        Task FlushAsync();
        Task<bool> IsEnabledAsync();
        Task SetEnabledAsync(bool enabled);
    }

    public interface IOperationTracker : IDisposable
    {
        void SetSuccess(bool success);
        void SetResult(string result);
        void AddProperty(string key, string value);
        void AddProperty(string key, object value);
    }
} 