using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IMonitoringService
    {
        // ELK Stack Integration
        Task<bool> SendToElasticsearchAsync(string index, object data);
        Task<bool> SendToLogstashAsync(string pipeline, object data);
        Task<List<object>> QueryElasticsearchAsync(string index, string query);
        
        // Application Performance Monitoring
        Task TrackMetricAsync(string metricName, double value, Dictionary<string, string> tags = null);
        Task TrackDependencyAsync(string dependencyType, string target, TimeSpan duration, bool success);
        Task TrackExceptionAsync(Exception exception, Dictionary<string, string> properties = null);
        Task TrackEventAsync(string eventName, Dictionary<string, string> properties = null);
        
        // Health Checks
        Task<HealthCheckResult> CheckDatabaseHealthAsync();
        Task<HealthCheckResult> CheckRedisHealthAsync();
        Task<HealthCheckResult> CheckExternalServicesHealthAsync();
        Task<HealthCheckResult> CheckPerformanceHealthAsync();
        
        // Custom Metrics
        Task IncrementCounterAsync(string counterName, Dictionary<string, string> tags = null);
        Task SetGaugeAsync(string gaugeName, double value, Dictionary<string, string> tags = null);
        Task RecordHistogramAsync(string histogramName, double value, Dictionary<string, string> tags = null);
    }

    public class HealthCheckResult
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public TimeSpan ResponseTime { get; set; }
    }
} 