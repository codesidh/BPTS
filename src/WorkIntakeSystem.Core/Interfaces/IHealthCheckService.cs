using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IHealthCheckService
    {
        // Database Health Checks
        Task<HealthCheckResult> CheckDatabaseConnectivityAsync();
        Task<HealthCheckResult> CheckDatabasePerformanceAsync();
        Task<HealthCheckResult> CheckDatabaseMigrationStatusAsync();
        
        // Cache Health Checks
        Task<HealthCheckResult> CheckRedisConnectivityAsync();
        Task<HealthCheckResult> CheckRedisPerformanceAsync();
        Task<HealthCheckResult> CheckMemoryCacheStatusAsync();
        
        // External Services Health Checks
        Task<HealthCheckResult> CheckExternalServiceAsync(string serviceName, string endpoint);
        Task<HealthCheckResult> CheckMicrosoft365ServicesAsync();
        Task<HealthCheckResult> CheckDevOpsServicesAsync();
        Task<HealthCheckResult> CheckFinancialSystemsAsync();
        
        // System Health Checks
        Task<HealthCheckResult> CheckSystemResourcesAsync();
        Task<HealthCheckResult> CheckDiskSpaceAsync();
        Task<HealthCheckResult> CheckMemoryUsageAsync();
        Task<HealthCheckResult> CheckCpuUsageAsync();
        
        // Application Health Checks
        Task<HealthCheckResult> CheckApplicationPerformanceAsync();
        Task<HealthCheckResult> CheckApiEndpointsAsync();
        Task<HealthCheckResult> CheckBackgroundServicesAsync();
        Task<HealthCheckResult> CheckWorkflowEngineAsync();
        
        // Comprehensive Health Check
        Task<ComprehensiveHealthReport> RunComprehensiveHealthCheckAsync();
        
        // Health Check Configuration
        Task<bool> IsHealthCheckEnabledAsync(string checkName);
        Task SetHealthCheckEnabledAsync(string checkName, bool enabled);
        Task<TimeSpan> GetHealthCheckIntervalAsync(string checkName);
        Task SetHealthCheckIntervalAsync(string checkName, TimeSpan interval);
    }

    public class ComprehensiveHealthReport
    {
        public DateTime Timestamp { get; set; }
        public bool OverallHealth { get; set; }
        public required string OverallStatus { get; set; }
        public TimeSpan TotalCheckTime { get; set; }
        public List<HealthCheckResult> DatabaseChecks { get; set; } = new List<HealthCheckResult>();
        public List<HealthCheckResult> CacheChecks { get; set; } = new List<HealthCheckResult>();
        public List<HealthCheckResult> ExternalServiceChecks { get; set; } = new List<HealthCheckResult>();
        public List<HealthCheckResult> SystemChecks { get; set; } = new List<HealthCheckResult>();
        public List<HealthCheckResult> ApplicationChecks { get; set; } = new List<HealthCheckResult>();
        public Dictionary<string, object> Summary { get; set; } = new Dictionary<string, object>();
    }
} 