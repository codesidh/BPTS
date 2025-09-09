using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services
{
    /// <summary>
    /// Service for performance optimization and monitoring
    /// </summary>
    public class PerformanceOptimizationService : IPerformanceOptimizationService
    {
        private readonly ILogger<PerformanceOptimizationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMonitoringService _monitoringService;
        private readonly IApplicationPerformanceMonitoringService _apmService;
        private readonly Dictionary<string, PerformanceMetric> _performanceMetrics;
        private readonly Dictionary<string, List<PerformanceAlert>> _alerts;

        public PerformanceOptimizationService(
            ILogger<PerformanceOptimizationService> logger,
            IConfiguration configuration,
            IMonitoringService monitoringService,
            IApplicationPerformanceMonitoringService apmService)
        {
            _logger = logger;
            _configuration = configuration;
            _monitoringService = monitoringService;
            _apmService = apmService;
            _performanceMetrics = new Dictionary<string, PerformanceMetric>();
            _alerts = new Dictionary<string, List<PerformanceAlert>>();
        }

        #region Performance Monitoring

        public async Task TrackPerformanceMetricAsync(string metricName, double value, Dictionary<string, string> tags = null)
        {
            try
            {
                var metric = new PerformanceMetric
                {
                    Name = metricName,
                    Value = value,
                    Timestamp = DateTime.UtcNow,
                    Tags = tags ?? new Dictionary<string, string>()
                };

                if (_performanceMetrics.ContainsKey(metricName))
                {
                    _performanceMetrics[metricName] = metric;
                }
                else
                {
                    _performanceMetrics.Add(metricName, metric);
                }

                // Send to APM service
                await _apmService.TrackMetricAsync(metricName, value, tags);

                // Check for performance thresholds
                await CheckPerformanceThresholds(metric);

                _logger.LogDebug("Tracked performance metric: {MetricName} = {Value}", metricName, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking performance metric: {MetricName}", metricName);
            }
        }

        public async Task TrackResponseTimeAsync(string operation, long responseTimeMs, Dictionary<string, string> tags = null)
        {
            try
            {
                var metricName = $"response_time_{operation}";
                await TrackPerformanceMetricAsync(metricName, responseTimeMs, tags);

                // Track response time distribution
                var distributionMetric = $"response_time_distribution_{operation}";
                await TrackPerformanceMetricAsync(distributionMetric, responseTimeMs, tags);

                // Check response time thresholds
                await CheckResponseTimeThresholds(operation, responseTimeMs);

                _logger.LogDebug("Tracked response time: {Operation} = {ResponseTime}ms", operation, responseTimeMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking response time: {Operation}", operation);
            }
        }

        public async Task TrackThroughputAsync(string operation, int requestCount, TimeSpan duration, Dictionary<string, string> tags = null)
        {
            try
            {
                var throughput = requestCount / duration.TotalSeconds;
                var metricName = $"throughput_{operation}";
                await TrackPerformanceMetricAsync(metricName, throughput, tags);

                // Track request count
                var requestCountMetric = $"request_count_{operation}";
                await TrackPerformanceMetricAsync(requestCountMetric, requestCount, tags);

                _logger.LogDebug("Tracked throughput: {Operation} = {Throughput:F2} req/sec", operation, throughput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking throughput: {Operation}", operation);
            }
        }

        public async Task TrackErrorRateAsync(string operation, int errorCount, int totalCount, Dictionary<string, string> tags = null)
        {
            try
            {
                var errorRate = (double)errorCount / totalCount * 100;
                var metricName = $"error_rate_{operation}";
                await TrackPerformanceMetricAsync(metricName, errorRate, tags);

                // Track error count
                var errorCountMetric = $"error_count_{operation}";
                await TrackPerformanceMetricAsync(errorCountMetric, errorCount, tags);

                // Check error rate thresholds
                await CheckErrorRateThresholds(operation, errorRate);

                _logger.LogDebug("Tracked error rate: {Operation} = {ErrorRate:F2}%", operation, errorRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking error rate: {Operation}", operation);
            }
        }

        #endregion

        #region Performance Analysis

        public async Task<PerformanceAnalysis> AnalyzePerformanceAsync(string operation, TimeSpan timeWindow)
        {
            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime - timeWindow;

                var metrics = _performanceMetrics.Values
                    .Where(m => m.Name.Contains(operation) && m.Timestamp >= startTime && m.Timestamp <= endTime)
                    .ToList();

                if (!metrics.Any())
                {
                    return new PerformanceAnalysis
                    {
                        Operation = operation,
                        TimeWindow = timeWindow,
                        Status = "No data available"
                    };
                }

                var responseTimeMetrics = metrics.Where(m => m.Name.Contains("response_time")).ToList();
                var throughputMetrics = metrics.Where(m => m.Name.Contains("throughput")).ToList();
                var errorRateMetrics = metrics.Where(m => m.Name.Contains("error_rate")).ToList();

                var analysis = new PerformanceAnalysis
                {
                    Operation = operation,
                    TimeWindow = timeWindow,
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalRequests = metrics.Count,
                    AverageResponseTime = responseTimeMetrics.Any() ? responseTimeMetrics.Average(m => m.Value) : 0,
                    MinResponseTime = responseTimeMetrics.Any() ? responseTimeMetrics.Min(m => m.Value) : 0,
                    MaxResponseTime = responseTimeMetrics.Any() ? responseTimeMetrics.Max(m => m.Value) : 0,
                    AverageThroughput = throughputMetrics.Any() ? throughputMetrics.Average(m => m.Value) : 0,
                    AverageErrorRate = errorRateMetrics.Any() ? errorRateMetrics.Average(m => m.Value) : 0,
                    Status = "Analyzed"
                };

                // Calculate percentiles
                if (responseTimeMetrics.Any())
                {
                    var sortedResponseTimes = responseTimeMetrics.Select(m => m.Value).OrderBy(v => v).ToList();
                    analysis.Percentile95 = CalculatePercentile(sortedResponseTimes, 0.95);
                    analysis.Percentile99 = CalculatePercentile(sortedResponseTimes, 0.99);
                }

                // Determine performance status
                analysis.PerformanceStatus = DeterminePerformanceStatus(analysis);

                _logger.LogInformation("Performance analysis completed for {Operation}: {Status}", operation, analysis.PerformanceStatus);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing performance for operation: {Operation}", operation);
                return new PerformanceAnalysis
                {
                    Operation = operation,
                    TimeWindow = timeWindow,
                    Status = "Error",
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<PerformanceRecommendation>> GetPerformanceRecommendationsAsync(string operation)
        {
            try
            {
                var recommendations = new List<PerformanceRecommendation>();
                var analysis = await AnalyzePerformanceAsync(operation, TimeSpan.FromHours(1));

                if (analysis.Status != "Analyzed")
                {
                    return recommendations;
                }

                // Response time recommendations
                if (analysis.AverageResponseTime > 1000)
                {
                    recommendations.Add(new PerformanceRecommendation
                    {
                        Type = "Response Time",
                        Priority = "High",
                        Description = "Average response time exceeds 1000ms",
                        Recommendation = "Consider implementing caching, database optimization, or code refactoring",
                        Impact = "High",
                        Effort = "Medium"
                    });
                }
                else if (analysis.AverageResponseTime > 500)
                {
                    recommendations.Add(new PerformanceRecommendation
                    {
                        Type = "Response Time",
                        Priority = "Medium",
                        Description = "Average response time exceeds 500ms",
                        Recommendation = "Monitor closely and consider optimization if trend continues",
                        Impact = "Medium",
                        Effort = "Low"
                    });
                }

                // Throughput recommendations
                if (analysis.AverageThroughput < 10)
                {
                    recommendations.Add(new PerformanceRecommendation
                    {
                        Type = "Throughput",
                        Priority = "High",
                        Description = "Throughput is below 10 req/sec",
                        Recommendation = "Investigate bottlenecks in database queries, external API calls, or resource constraints",
                        Impact = "High",
                        Effort = "High"
                    });
                }

                // Error rate recommendations
                if (analysis.AverageErrorRate > 5)
                {
                    recommendations.Add(new PerformanceRecommendation
                    {
                        Type = "Error Rate",
                        Priority = "High",
                        Description = $"Error rate is {analysis.AverageErrorRate:F2}%",
                        Recommendation = "Investigate error causes and implement proper error handling",
                        Impact = "High",
                        Effort = "Medium"
                    });
                }

                // 95th percentile recommendations
                if (analysis.Percentile95 > 2000)
                {
                    recommendations.Add(new PerformanceRecommendation
                    {
                        Type = "Response Time Distribution",
                        Priority = "Medium",
                        Description = "95th percentile response time exceeds 2000ms",
                        Recommendation = "Investigate outliers and consider implementing request queuing or load balancing",
                        Impact = "Medium",
                        Effort = "Medium"
                    });
                }

                _logger.LogInformation("Generated {Count} performance recommendations for {Operation}", recommendations.Count, operation);

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance recommendations for operation: {Operation}", operation);
                return new List<PerformanceRecommendation>();
            }
        }

        #endregion

        #region Performance Optimization

        public async Task OptimizePerformanceAsync(string operation)
        {
            try
            {
                var recommendations = await GetPerformanceRecommendationsAsync(operation);
                var highPriorityRecommendations = recommendations.Where(r => r.Priority == "High").ToList();

                if (!highPriorityRecommendations.Any())
                {
                    _logger.LogInformation("No high-priority optimizations needed for {Operation}", operation);
                    return;
                }

                _logger.LogInformation("Starting performance optimization for {Operation} with {Count} high-priority recommendations", 
                    operation, highPriorityRecommendations.Count);

                foreach (var recommendation in highPriorityRecommendations)
                {
                    await ApplyOptimizationAsync(operation, recommendation);
                }

                _logger.LogInformation("Performance optimization completed for {Operation}", operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing performance for operation: {Operation}", operation);
            }
        }

        public async Task<PerformanceReport> GeneratePerformanceReportAsync(TimeSpan timeWindow)
        {
            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime - timeWindow;

                var operations = _performanceMetrics.Values
                    .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                    .Select(m => ExtractOperationFromMetricName(m.Name))
                    .Distinct()
                    .ToList();

                var report = new PerformanceReport
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    TimeWindow = timeWindow,
                    Operations = new List<PerformanceAnalysis>()
                };

                foreach (var operation in operations)
                {
                    var analysis = await AnalyzePerformanceAsync(operation, timeWindow);
                    report.Operations.Add(analysis);
                }

                report.OverallStatus = DetermineOverallPerformanceStatus(report.Operations);
                report.TotalOperations = operations.Count;
                report.AverageResponseTime = report.Operations.Average(o => o.AverageResponseTime);
                report.AverageThroughput = report.Operations.Average(o => o.AverageThroughput);
                report.AverageErrorRate = report.Operations.Average(o => o.AverageErrorRate);

                _logger.LogInformation("Generated performance report for {TimeWindow} with {OperationCount} operations", 
                    timeWindow, operations.Count);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance report");
                return new PerformanceReport
                {
                    StartTime = DateTime.UtcNow - timeWindow,
                    EndTime = DateTime.UtcNow,
                    TimeWindow = timeWindow,
                    OverallStatus = "Error",
                    ErrorMessage = ex.Message
                };
            }
        }

        #endregion

        #region Helper Methods

        private async Task CheckPerformanceThresholds(PerformanceMetric metric)
        {
            try
            {
                var thresholds = _configuration.GetSection("Performance:Thresholds").Get<Dictionary<string, double>>();
                if (thresholds == null || !thresholds.ContainsKey(metric.Name))
                    return;

                var threshold = thresholds[metric.Name];
                if (metric.Value > threshold)
                {
                    var alert = new PerformanceAlert
                    {
                        MetricName = metric.Name,
                        CurrentValue = metric.Value,
                        Threshold = threshold,
                        Severity = "Warning",
                        Timestamp = DateTime.UtcNow,
                        Message = $"Performance metric {metric.Name} exceeded threshold. Current: {metric.Value}, Threshold: {threshold}"
                    };

                    await CreatePerformanceAlertAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking performance thresholds for metric: {MetricName}", metric.Name);
            }
        }

        private async Task CheckResponseTimeThresholds(string operation, long responseTimeMs)
        {
            try
            {
                var responseTimeThreshold = _configuration.GetValue<int>("Performance:ResponseTimeThreshold", 1000);
                if (responseTimeMs > responseTimeThreshold)
                {
                    var alert = new PerformanceAlert
                    {
                        MetricName = $"response_time_{operation}",
                        CurrentValue = responseTimeMs,
                        Threshold = responseTimeThreshold,
                        Severity = "Warning",
                        Timestamp = DateTime.UtcNow,
                        Message = $"Response time for {operation} exceeded threshold. Current: {responseTimeMs}ms, Threshold: {responseTimeThreshold}ms"
                    };

                    await CreatePerformanceAlertAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking response time thresholds for operation: {Operation}", operation);
            }
        }

        private async Task CheckErrorRateThresholds(string operation, double errorRate)
        {
            try
            {
                var errorRateThreshold = _configuration.GetValue<double>("Performance:ErrorRateThreshold", 5.0);
                if (errorRate > errorRateThreshold)
                {
                    var alert = new PerformanceAlert
                    {
                        MetricName = $"error_rate_{operation}",
                        CurrentValue = errorRate,
                        Threshold = errorRateThreshold,
                        Severity = "Critical",
                        Timestamp = DateTime.UtcNow,
                        Message = $"Error rate for {operation} exceeded threshold. Current: {errorRate:F2}%, Threshold: {errorRateThreshold:F2}%"
                    };

                    await CreatePerformanceAlertAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking error rate thresholds for operation: {Operation}", operation);
            }
        }

        private async Task CreatePerformanceAlertAsync(PerformanceAlert alert)
        {
            try
            {
                if (!_alerts.ContainsKey(alert.MetricName))
                {
                    _alerts[alert.MetricName] = new List<PerformanceAlert>();
                }

                _alerts[alert.MetricName].Add(alert);

                // Send to monitoring service
                await _monitoringService.TrackEventAsync("PerformanceAlert", new Dictionary<string, string>
                {
                    { "MetricName", alert.MetricName },
                    { "Severity", alert.Severity },
                    { "CurrentValue", alert.CurrentValue.ToString() },
                    { "Threshold", alert.Threshold.ToString() }
                });

                _logger.LogWarning("Performance alert created: {Message}", alert.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating performance alert for metric: {MetricName}", alert.MetricName);
            }
        }

        private double CalculatePercentile(List<double> values, double percentile)
        {
            if (!values.Any())
                return 0;

            var sortedValues = values.OrderBy(v => v).ToList();
            var index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
            return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
        }

        private string DeterminePerformanceStatus(PerformanceAnalysis analysis)
        {
            if (analysis.AverageErrorRate > 10)
                return "Critical";
            if (analysis.AverageResponseTime > 2000 || analysis.AverageErrorRate > 5)
                return "Poor";
            if (analysis.AverageResponseTime > 1000 || analysis.AverageErrorRate > 2)
                return "Fair";
            if (analysis.AverageResponseTime > 500)
                return "Good";
            return "Excellent";
        }

        private string DetermineOverallPerformanceStatus(List<PerformanceAnalysis> analyses)
        {
            if (analyses.Any(a => a.PerformanceStatus == "Critical"))
                return "Critical";
            if (analyses.Any(a => a.PerformanceStatus == "Poor"))
                return "Poor";
            if (analyses.Any(a => a.PerformanceStatus == "Fair"))
                return "Fair";
            if (analyses.Any(a => a.PerformanceStatus == "Good"))
                return "Good";
            return "Excellent";
        }

        private string ExtractOperationFromMetricName(string metricName)
        {
            var parts = metricName.Split('_');
            if (parts.Length >= 2)
            {
                return string.Join("_", parts.Skip(1));
            }
            return metricName;
        }

        private async Task ApplyOptimizationAsync(string operation, PerformanceRecommendation recommendation)
        {
            try
            {
                _logger.LogInformation("Applying optimization for {Operation}: {Recommendation}", operation, recommendation.Recommendation);

                // This is a placeholder for actual optimization logic
                // In a real implementation, this would apply specific optimizations based on the recommendation type

                switch (recommendation.Type)
                {
                    case "Response Time":
                        await OptimizeResponseTimeAsync(operation);
                        break;
                    case "Throughput":
                        await OptimizeThroughputAsync(operation);
                        break;
                    case "Error Rate":
                        await OptimizeErrorRateAsync(operation);
                        break;
                    case "Response Time Distribution":
                        await OptimizeResponseTimeDistributionAsync(operation);
                        break;
                }

                _logger.LogInformation("Optimization applied for {Operation}: {Type}", operation, recommendation.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying optimization for {Operation}: {Type}", operation, recommendation.Type);
            }
        }

        private async Task OptimizeResponseTimeAsync(string operation)
        {
            // Placeholder for response time optimization
            await Task.Delay(100);
        }

        private async Task OptimizeThroughputAsync(string operation)
        {
            // Placeholder for throughput optimization
            await Task.Delay(100);
        }

        private async Task OptimizeErrorRateAsync(string operation)
        {
            // Placeholder for error rate optimization
            await Task.Delay(100);
        }

        private async Task OptimizeResponseTimeDistributionAsync(string operation)
        {
            // Placeholder for response time distribution optimization
            await Task.Delay(100);
        }

        #endregion
    }

    #region Data Models

    public class PerformanceMetric
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class PerformanceAlert
    {
        public string MetricName { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double Threshold { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PerformanceAnalysis
    {
        public string Operation { get; set; } = string.Empty;
        public TimeSpan TimeWindow { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalRequests { get; set; }
        public double AverageResponseTime { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public double Percentile95 { get; set; }
        public double Percentile99 { get; set; }
        public double AverageThroughput { get; set; }
        public double AverageErrorRate { get; set; }
        public string PerformanceStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class PerformanceRecommendation
    {
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string Effort { get; set; } = string.Empty;
    }

    public class PerformanceReport
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeWindow { get; set; }
        public List<PerformanceAnalysis> Operations { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public int TotalOperations { get; set; }
        public double AverageResponseTime { get; set; }
        public double AverageThroughput { get; set; }
        public double AverageErrorRate { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    #endregion
}
