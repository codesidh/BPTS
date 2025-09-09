using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkIntakeSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for performance optimization and monitoring services
    /// </summary>
    public interface IPerformanceOptimizationService
    {
        #region Performance Monitoring

        /// <summary>
        /// Track a performance metric
        /// </summary>
        /// <param name="metricName">Name of the metric</param>
        /// <param name="value">Value of the metric</param>
        /// <param name="tags">Optional tags for the metric</param>
        Task TrackPerformanceMetricAsync(string metricName, double value, Dictionary<string, string> tags = null);

        /// <summary>
        /// Track response time for an operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        /// <param name="responseTimeMs">Response time in milliseconds</param>
        /// <param name="tags">Optional tags for the metric</param>
        Task TrackResponseTimeAsync(string operation, long responseTimeMs, Dictionary<string, string> tags = null);

        /// <summary>
        /// Track throughput for an operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        /// <param name="requestCount">Number of requests</param>
        /// <param name="duration">Duration of the requests</param>
        /// <param name="tags">Optional tags for the metric</param>
        Task TrackThroughputAsync(string operation, int requestCount, TimeSpan duration, Dictionary<string, string> tags = null);

        /// <summary>
        /// Track error rate for an operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        /// <param name="errorCount">Number of errors</param>
        /// <param name="totalCount">Total number of requests</param>
        /// <param name="tags">Optional tags for the metric</param>
        Task TrackErrorRateAsync(string operation, int errorCount, int totalCount, Dictionary<string, string> tags = null);

        #endregion

        #region Performance Analysis

        /// <summary>
        /// Analyze performance for a specific operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        /// <param name="timeWindow">Time window for analysis</param>
        /// <returns>Performance analysis results</returns>
        Task<PerformanceAnalysis> AnalyzePerformanceAsync(string operation, TimeSpan timeWindow);

        /// <summary>
        /// Get performance recommendations for an operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        /// <returns>List of performance recommendations</returns>
        Task<List<PerformanceRecommendation>> GetPerformanceRecommendationsAsync(string operation);

        #endregion

        #region Performance Optimization

        /// <summary>
        /// Optimize performance for a specific operation
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        Task OptimizePerformanceAsync(string operation);

        /// <summary>
        /// Generate a comprehensive performance report
        /// </summary>
        /// <param name="timeWindow">Time window for the report</param>
        /// <returns>Performance report</returns>
        Task<PerformanceReport> GeneratePerformanceReportAsync(TimeSpan timeWindow);

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Performance analysis results
    /// </summary>
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

    /// <summary>
    /// Performance recommendation
    /// </summary>
    public class PerformanceRecommendation
    {
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string Effort { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performance report
    /// </summary>
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
