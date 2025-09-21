using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkIntakeSystem.API.Controllers
{
    /// <summary>
    /// Controller for performance monitoring and optimization
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SystemAdministrator,Admin")]
    public class PerformanceMonitoringController : ControllerBase
    {
        private readonly IPerformanceOptimizationService _performanceService;
        private readonly ILogger<PerformanceMonitoringController> _logger;

        public PerformanceMonitoringController(
            IPerformanceOptimizationService performanceService,
            ILogger<PerformanceMonitoringController> logger)
        {
            _performanceService = performanceService;
            _logger = logger;
        }

        #region Performance Metrics

        /// <summary>
        /// Track a performance metric
        /// </summary>
        /// <param name="request">Performance metric request</param>
        [HttpPost("metrics")]
        public async Task<IActionResult> TrackPerformanceMetric([FromBody] PerformanceMetricRequest request)
        {
            try
            {
                await _performanceService.TrackPerformanceMetricAsync(
                    request.MetricName, 
                    request.Value, 
                    request.Tags);

                return Ok(new { message = "Performance metric tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking performance metric: {MetricName}", request.MetricName);
                return StatusCode(500, new { error = "Failed to track performance metric", message = ex.Message });
            }
        }

        /// <summary>
        /// Track response time for an operation
        /// </summary>
        /// <param name="request">Response time tracking request</param>
        [HttpPost("response-time")]
        public async Task<IActionResult> TrackResponseTime([FromBody] TrackResponseTimeRequest request)
        {
            try
            {
                await _performanceService.TrackResponseTimeAsync(
                    request.Operation, 
                    request.ResponseTimeMs, 
                    request.Tags);

                return Ok(new { message = "Response time tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking response time for operation: {Operation}", request.Operation);
                return StatusCode(500, new { error = "Failed to track response time", message = ex.Message });
            }
        }

        /// <summary>
        /// Track throughput for an operation
        /// </summary>
        /// <param name="request">Throughput tracking request</param>
        [HttpPost("throughput")]
        public async Task<IActionResult> TrackThroughput([FromBody] TrackThroughputRequest request)
        {
            try
            {
                await _performanceService.TrackThroughputAsync(
                    request.Operation, 
                    request.RequestCount, 
                    TimeSpan.FromSeconds(request.DurationSeconds), 
                    request.Tags);

                return Ok(new { message = "Throughput tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking throughput for operation: {Operation}", request.Operation);
                return StatusCode(500, new { error = "Failed to track throughput", message = ex.Message });
            }
        }

        /// <summary>
        /// Track error rate for an operation
        /// </summary>
        /// <param name="request">Error rate tracking request</param>
        [HttpPost("error-rate")]
        public async Task<IActionResult> TrackErrorRate([FromBody] TrackErrorRateRequest request)
        {
            try
            {
                await _performanceService.TrackErrorRateAsync(
                    request.Operation, 
                    request.ErrorCount, 
                    request.TotalCount, 
                    request.Tags);

                return Ok(new { message = "Error rate tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking error rate for operation: {Operation}", request.Operation);
                return StatusCode(500, new { error = "Failed to track error rate", message = ex.Message });
            }
        }

        #endregion

        #region Performance Analysis

        /// <summary>
        /// Analyze performance for a specific operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        /// <param name="timeWindowHours">Time window in hours (default: 1)</param>
        [HttpGet("analyze/{operation}")]
        public async Task<IActionResult> AnalyzePerformance(string operation, [FromQuery] int timeWindowHours = 1)
        {
            try
            {
                var timeWindow = TimeSpan.FromHours(timeWindowHours);
                var analysis = await _performanceService.AnalyzePerformanceAsync(operation, timeWindow);

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing performance for operation: {Operation}", operation);
                return StatusCode(500, new { error = "Failed to analyze performance", message = ex.Message });
            }
        }

        /// <summary>
        /// Get performance recommendations for an operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        [HttpGet("recommendations/{operation}")]
        public async Task<IActionResult> GetPerformanceRecommendations(string operation)
        {
            try
            {
                var recommendations = await _performanceService.GetPerformanceRecommendationsAsync(operation);

                return Ok(new
                {
                    operation,
                    recommendations,
                    count = recommendations.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance recommendations for operation: {Operation}", operation);
                return StatusCode(500, new { error = "Failed to get performance recommendations", message = ex.Message });
            }
        }

        /// <summary>
        /// Generate a comprehensive performance report
        /// </summary>
        /// <param name="timeWindowHours">Time window in hours (default: 24)</param>
        [HttpGet("report")]
        public async Task<IActionResult> GeneratePerformanceReport([FromQuery] int timeWindowHours = 24)
        {
            try
            {
                var timeWindow = TimeSpan.FromHours(timeWindowHours);
                var report = await _performanceService.GeneratePerformanceReportAsync(timeWindow);

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance report");
                return StatusCode(500, new { error = "Failed to generate performance report", message = ex.Message });
            }
        }

        #endregion

        #region Performance Optimization

        /// <summary>
        /// Optimize performance for a specific operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        [HttpPost("optimize/{operation}")]
        public async Task<IActionResult> OptimizePerformance(string operation)
        {
            try
            {
                await _performanceService.OptimizePerformanceAsync(operation);

                return Ok(new { message = $"Performance optimization initiated for operation: {operation}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing performance for operation: {Operation}", operation);
                return StatusCode(500, new { error = "Failed to optimize performance", message = ex.Message });
            }
        }

        /// <summary>
        /// Get performance status overview
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetPerformanceStatus()
        {
            try
            {
                // Generate a 1-hour performance report for status overview
                var report = await _performanceService.GeneratePerformanceReportAsync(TimeSpan.FromHours(1));

                var status = new
                {
                    overall_status = report.OverallStatus,
                    total_operations = report.TotalOperations,
                    average_response_time = report.AverageResponseTime,
                    average_throughput = report.AverageThroughput,
                    average_error_rate = report.AverageErrorRate,
                    timestamp = DateTime.UtcNow,
                    operations = report.Operations.Select(op => new
                    {
                        operation = op.Operation,
                        status = op.PerformanceStatus,
                        response_time = op.AverageResponseTime,
                        throughput = op.AverageThroughput,
                        error_rate = op.AverageErrorRate
                    })
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance status");
                return StatusCode(500, new { error = "Failed to get performance status", message = ex.Message });
            }
        }

        #endregion

        #region Load Testing

        /// <summary>
        /// Start a load test for a specific operation
        /// </summary>
        /// <param name="request">Load test request</param>
        [HttpPost("load-test")]
        public async Task<IActionResult> StartLoadTest([FromBody] LoadTestRequest request)
        {
            try
            {
                // This is a placeholder for actual load testing implementation
                // In a real implementation, this would start a background load test

                _logger.LogInformation("Load test requested for operation: {Operation} with {ConcurrentUsers} concurrent users", 
                    request.Operation, request.ConcurrentUsers);

                return Ok(new
                {
                    message = "Load test initiated",
                    operation = request.Operation,
                    concurrent_users = request.ConcurrentUsers,
                    duration_seconds = request.DurationSeconds,
                    test_id = Guid.NewGuid().ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting load test for operation: {Operation}", request.Operation);
                return StatusCode(500, new { error = "Failed to start load test", message = ex.Message });
            }
        }

        /// <summary>
        /// Get load test results
        /// </summary>
        /// <param name="testId">Load test ID</param>
        [HttpGet("load-test/{testId}")]
        public async Task<IActionResult> GetLoadTestResults(string testId)
        {
            try
            {
                // This is a placeholder for actual load test results retrieval
                // In a real implementation, this would retrieve actual load test results

                var results = new
                {
                    test_id = testId,
                    status = "Completed",
                    start_time = DateTime.UtcNow.AddMinutes(-5),
                    end_time = DateTime.UtcNow,
                    total_requests = 1000,
                    successful_requests = 950,
                    failed_requests = 50,
                    average_response_time = 250.5,
                    percentile_95 = 500.0,
                    throughput = 200.0,
                    error_rate = 5.0
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting load test results for test ID: {TestId}", testId);
                return StatusCode(500, new { error = "Failed to get load test results", message = ex.Message });
            }
        }

        #endregion
    }

    #region Request Models

    public class PerformanceMetricRequest
    {
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class TrackResponseTimeRequest
    {
        public string Operation { get; set; } = string.Empty;
        public long ResponseTimeMs { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class TrackThroughputRequest
    {
        public string Operation { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public int DurationSeconds { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class TrackErrorRateRequest
    {
        public string Operation { get; set; } = string.Empty;
        public int ErrorCount { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class LoadTestRequest
    {
        public string Operation { get; set; } = string.Empty;
        public int ConcurrentUsers { get; set; }
        public int DurationSeconds { get; set; }
        public int RequestsPerUser { get; set; }
    }

    #endregion
}
