using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class MonitoringController : ControllerBase
    {
        private readonly ILogger<MonitoringController> _logger;
        private readonly IMonitoringService _monitoringService;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IApplicationPerformanceMonitoringService _apmService;
        private readonly IElasticsearchService _elasticsearchService;

        public MonitoringController(
            ILogger<MonitoringController> logger,
            IMonitoringService monitoringService,
            IHealthCheckService healthCheckService,
            IApplicationPerformanceMonitoringService apmService,
            IElasticsearchService elasticsearchService)
        {
            _logger = logger;
            _monitoringService = monitoringService;
            _healthCheckService = healthCheckService;
            _apmService = apmService;
            _elasticsearchService = elasticsearchService;
        }

        #region Health Checks

        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHealthStatus()
        {
            try
            {
                var healthReport = await _healthCheckService.RunComprehensiveHealthCheckAsync();
                
                return Ok(new
                {
                    timestamp = healthReport.Timestamp,
                    overall_health = healthReport.OverallHealth,
                    overall_status = healthReport.OverallStatus,
                    total_check_time_ms = healthReport.TotalCheckTime.TotalMilliseconds,
                    summary = healthReport.Summary,
                    database_checks = healthReport.DatabaseChecks,
                    cache_checks = healthReport.CacheChecks,
                    external_service_checks = healthReport.ExternalServiceChecks,
                    system_checks = healthReport.SystemChecks,
                    application_checks = healthReport.ApplicationChecks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                return StatusCode(500, new { error = "Failed to get health status", message = ex.Message });
            }
        }

        [HttpGet("health/database")]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            try
            {
                var connectivity = await _healthCheckService.CheckDatabaseConnectivityAsync();
                var performance = await _healthCheckService.CheckDatabasePerformanceAsync();
                var migration = await _healthCheckService.CheckDatabaseMigrationStatusAsync();

                return Ok(new
                {
                    connectivity,
                    performance,
                    migration,
                    overall_healthy = connectivity.IsHealthy && performance.IsHealthy && migration.IsHealthy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database health");
                return StatusCode(500, new { error = "Failed to get database health", message = ex.Message });
            }
        }

        [HttpGet("health/cache")]
        public async Task<IActionResult> GetCacheHealth()
        {
            try
            {
                var redisConnectivity = await _healthCheckService.CheckRedisConnectivityAsync();
                var redisPerformance = await _healthCheckService.CheckRedisPerformanceAsync();
                var memoryCache = await _healthCheckService.CheckMemoryCacheStatusAsync();

                return Ok(new
                {
                    redis_connectivity = redisConnectivity,
                    redis_performance = redisPerformance,
                    memory_cache = memoryCache,
                    overall_healthy = redisConnectivity.IsHealthy && redisPerformance.IsHealthy && memoryCache.IsHealthy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache health");
                return StatusCode(500, new { error = "Failed to get cache health", message = ex.Message });
            }
        }

        [HttpGet("health/external-services")]
        public async Task<IActionResult> GetExternalServicesHealth()
        {
            try
            {
                var microsoft365 = await _healthCheckService.CheckMicrosoft365ServicesAsync();
                var devOps = await _healthCheckService.CheckDevOpsServicesAsync();
                var financial = await _healthCheckService.CheckFinancialSystemsAsync();

                return Ok(new
                {
                    microsoft_365 = microsoft365,
                    dev_ops = devOps,
                    financial_systems = financial,
                    overall_healthy = microsoft365.IsHealthy && devOps.IsHealthy && financial.IsHealthy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting external services health");
                return StatusCode(500, new { error = "Failed to get external services health", message = ex.Message });
            }
        }

        [HttpGet("health/system")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var resources = await _healthCheckService.CheckSystemResourcesAsync();
                var diskSpace = await _healthCheckService.CheckDiskSpaceAsync();
                var memoryUsage = await _healthCheckService.CheckMemoryUsageAsync();
                var cpuUsage = await _healthCheckService.CheckCpuUsageAsync();

                return Ok(new
                {
                    resources,
                    disk_space = diskSpace,
                    memory_usage = memoryUsage,
                    cpu_usage = cpuUsage,
                    overall_healthy = resources.IsHealthy && diskSpace.IsHealthy && memoryUsage.IsHealthy && cpuUsage.IsHealthy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                return StatusCode(500, new { error = "Failed to get system health", message = ex.Message });
            }
        }

        [HttpGet("health/application")]
        public async Task<IActionResult> GetApplicationHealth()
        {
            try
            {
                var performance = await _healthCheckService.CheckApplicationPerformanceAsync();
                var apiEndpoints = await _healthCheckService.CheckApiEndpointsAsync();
                var backgroundServices = await _healthCheckService.CheckBackgroundServicesAsync();
                var workflowEngine = await _healthCheckService.CheckWorkflowEngineAsync();

                return Ok(new
                {
                    performance,
                    api_endpoints = apiEndpoints,
                    background_services = backgroundServices,
                    workflow_engine = workflowEngine,
                    overall_healthy = performance.IsHealthy && apiEndpoints.IsHealthy && backgroundServices.IsHealthy && workflowEngine.IsHealthy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application health");
                return StatusCode(500, new { error = "Failed to get application health", message = ex.Message });
            }
        }

        #endregion

        #region Metrics and Monitoring

        [HttpPost("metrics")]
        public async Task<IActionResult> TrackMetric([FromBody] TrackMetricRequest request)
        {
            try
            {
                await _monitoringService.TrackMetricAsync(request.MetricName, request.Value, request.Tags);
                
                _logger.LogDebug("Tracked metric: {MetricName} = {Value}", request.MetricName, request.Value);
                
                return Ok(new { success = true, message = "Metric tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking metric: {MetricName}", request.MetricName);
                return StatusCode(500, new { error = "Failed to track metric", message = ex.Message });
            }
        }

        [HttpPost("metrics/counter")]
        public async Task<IActionResult> IncrementCounter([FromBody] IncrementCounterRequest request)
        {
            try
            {
                await _monitoringService.IncrementCounterAsync(request.CounterName, request.Tags);
                
                _logger.LogDebug("Incremented counter: {CounterName}", request.CounterName);
                
                return Ok(new { success = true, message = "Counter incremented successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter: {CounterName}", request.CounterName);
                return StatusCode(500, new { error = "Failed to increment counter", message = ex.Message });
            }
        }

        [HttpPost("metrics/gauge")]
        public async Task<IActionResult> SetGauge([FromBody] SetGaugeRequest request)
        {
            try
            {
                await _monitoringService.SetGaugeAsync(request.GaugeName, request.Value, request.Tags);
                
                _logger.LogDebug("Set gauge: {GaugeName} = {Value}", request.GaugeName, request.Value);
                
                return Ok(new { success = true, message = "Gauge set successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting gauge: {GaugeName}", request.GaugeName);
                return StatusCode(500, new { error = "Failed to set gauge", message = ex.Message });
            }
        }

        [HttpPost("metrics/histogram")]
        public async Task<IActionResult> RecordHistogram([FromBody] RecordHistogramRequest request)
        {
            try
            {
                await _monitoringService.RecordHistogramAsync(request.HistogramName, request.Value, request.Tags);
                
                _logger.LogDebug("Recorded histogram: {HistogramName} = {Value}", request.HistogramName, request.Value);
                
                return Ok(new { success = true, message = "Histogram recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording histogram: {HistogramName}", request.HistogramName);
                return StatusCode(500, new { error = "Failed to record histogram", message = ex.Message });
            }
        }

        #endregion

        #region APM Operations

        [HttpPost("apm/dependency")]
        public async Task<IActionResult> TrackDependency([FromBody] TrackDependencyRequest request)
        {
            try
            {
                await _apmService.TrackDependencyAsync(
                    request.DependencyType,
                    request.Target,
                    TimeSpan.FromMilliseconds(request.DurationMs),
                    request.Success,
                    request.Properties);

                return Ok(new { success = true, message = "Dependency tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking dependency: {Type} -> {Target}", request.DependencyType, request.Target);
                return StatusCode(500, new { error = "Failed to track dependency", message = ex.Message });
            }
        }

        [HttpPost("apm/exception")]
        public async Task<IActionResult> TrackException([FromBody] TrackExceptionRequest request)
        {
            try
            {
                var exception = new Exception(request.Message);
                await _apmService.TrackExceptionAsync(exception, request.Properties);

                return Ok(new { success = true, message = "Exception tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking exception");
                return StatusCode(500, new { error = "Failed to track exception", message = ex.Message });
            }
        }

        [HttpPost("apm/event")]
        public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
        {
            try
            {
                await _apmService.TrackEventAsync(request.EventName, request.Properties);

                return Ok(new { success = true, message = "Event tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking event: {EventName}", request.EventName);
                return StatusCode(500, new { error = "Failed to track event", message = ex.Message });
            }
        }

        [HttpPost("apm/user-action")]
        public async Task<IActionResult> TrackUserAction([FromBody] TrackUserActionRequest request)
        {
            try
            {
                await _apmService.TrackUserActionAsync(request.UserId, request.Action, request.Properties);

                return Ok(new { success = true, message = "User action tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user action: {UserId} - {Action}", request.UserId, request.Action);
                return StatusCode(500, new { error = "Failed to track user action", message = ex.Message });
            }
        }

        #endregion

        #region Elasticsearch Operations

        [HttpGet("elasticsearch/indices")]
        public async Task<IActionResult> GetIndices()
        {
            try
            {
                var indices = await _elasticsearchService.GetIndicesAsync();
                
                return Ok(new { indices });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Elasticsearch indices");
                return StatusCode(500, new { error = "Failed to get indices", message = ex.Message });
            }
        }

        [HttpGet("elasticsearch/cluster-health")]
        public async Task<IActionResult> GetClusterHealth()
        {
            try
            {
                var clusterHealth = await _elasticsearchService.GetClusterHealthAsync();
                
                return Ok(clusterHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Elasticsearch cluster health");
                return StatusCode(500, new { error = "Failed to get cluster health", message = ex.Message });
            }
        }

        [HttpGet("elasticsearch/index-stats/{indexName}")]
        public async Task<IActionResult> GetIndexStats(string indexName)
        {
            try
            {
                var indexStats = await _elasticsearchService.GetIndexStatsAsync(indexName);
                
                return Ok(indexStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Elasticsearch index stats for {IndexName}", indexName);
                return StatusCode(500, new { error = "Failed to get index stats", message = ex.Message });
            }
        }

        [HttpPost("elasticsearch/search/{indexName}")]
        public async Task<IActionResult> Search(string indexName, [FromBody] SearchRequest request)
        {
            try
            {
                var searchResult = await _elasticsearchService.SearchAsync<object>(indexName, request.Query);
                
                return Ok(searchResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Elasticsearch index {IndexName}", indexName);
                return StatusCode(500, new { error = "Failed to search", message = ex.Message });
            }
        }

        #endregion

        #region Configuration

        [HttpGet("config/health-checks")]
        public async Task<IActionResult> GetHealthCheckConfiguration()
        {
            try
            {
                var config = new Dictionary<string, object>();
                
                foreach (var checkName in new[] { "database", "redis", "external_services", "system_resources", "application" })
                {
                    config[checkName] = new
                    {
                        enabled = await _healthCheckService.IsHealthCheckEnabledAsync(checkName),
                        interval_seconds = (await _healthCheckService.GetHealthCheckIntervalAsync(checkName)).TotalSeconds
                    };
                }
                
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health check configuration");
                return StatusCode(500, new { error = "Failed to get health check configuration", message = ex.Message });
            }
        }

        [HttpPut("config/health-checks/{checkName}/enabled")]
        public async Task<IActionResult> SetHealthCheckEnabled(string checkName, [FromBody] bool enabled)
        {
            try
            {
                await _healthCheckService.SetHealthCheckEnabledAsync(checkName, enabled);
                
                return Ok(new { success = true, message = $"Health check '{checkName}' enabled status set to {enabled}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting health check enabled status for {CheckName}", checkName);
                return StatusCode(500, new { error = "Failed to set health check enabled status", message = ex.Message });
            }
        }

        [HttpPut("config/health-checks/{checkName}/interval")]
        public async Task<IActionResult> SetHealthCheckInterval(string checkName, [FromBody] int intervalSeconds)
        {
            try
            {
                await _healthCheckService.SetHealthCheckIntervalAsync(checkName, TimeSpan.FromSeconds(intervalSeconds));
                
                return Ok(new { success = true, message = $"Health check '{checkName}' interval set to {intervalSeconds} seconds" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting health check interval for {CheckName}", checkName);
                return StatusCode(500, new { error = "Failed to set health check interval", message = ex.Message });
            }
        }

        #endregion

        #region Request Models

        public class TrackMetricRequest
        {
            public string MetricName { get; set; }
            public double Value { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class IncrementCounterRequest
        {
            public string CounterName { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class SetGaugeRequest
        {
            public string GaugeName { get; set; }
            public double Value { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class RecordHistogramRequest
        {
            public string HistogramName { get; set; }
            public double Value { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class TrackDependencyRequest
        {
            public string DependencyType { get; set; }
            public string Target { get; set; }
            public double DurationMs { get; set; }
            public bool Success { get; set; }
            public Dictionary<string, string> Properties { get; set; }
        }

        public class TrackExceptionRequest
        {
            public string Message { get; set; }
            public Dictionary<string, string> Properties { get; set; }
        }

        public class TrackEventRequest
        {
            public string EventName { get; set; }
            public Dictionary<string, string> Properties { get; set; }
        }

        public class TrackUserActionRequest
        {
            public string UserId { get; set; }
            public string Action { get; set; }
            public Dictionary<string, string> Properties { get; set; }
        }

        public class SearchRequest
        {
            public string Query { get; set; }
        }

        #endregion
    }
} 