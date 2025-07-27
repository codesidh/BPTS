using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardAnalytics> GetDashboardAnalyticsAsync(int? businessVerticalId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<DepartmentAnalytics> GetDepartmentAnalyticsAsync(int departmentId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<WorkflowAnalytics> GetWorkflowAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<PriorityAnalytics> GetPriorityAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ResourceUtilizationAnalytics> GetResourceUtilizationAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<SLAComplianceAnalytics> GetSLAComplianceAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TrendData>> GetTrendDataAsync(string metric, DateTime fromDate, DateTime toDate, string? groupBy = null);
    }

    public class DashboardAnalytics
    {
        public int TotalActiveRequests { get; set; }
        public int TotalCompletedRequests { get; set; }
        public decimal AverageCompletionTime { get; set; }
        public decimal SLAComplianceRate { get; set; }
        public decimal ResourceUtilization { get; set; }
        public Dictionary<WorkCategory, int> RequestsByCategory { get; set; } = new();
        public Dictionary<PriorityLevel, int> RequestsByPriority { get; set; } = new();
        public Dictionary<WorkStatus, int> RequestsByStatus { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
    }

    public class DepartmentAnalytics
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int ActiveRequests { get; set; }
        public int CompletedRequests { get; set; }
        public decimal AverageCompletionTime { get; set; }
        public decimal ResourceUtilization { get; set; }
        public Dictionary<WorkflowStage, int> RequestsByStage { get; set; } = new();
        public List<TeamMemberWorkload> TeamWorkload { get; set; } = new();
    }

    public class WorkflowAnalytics
    {
        public Dictionary<WorkflowStage, StageMetrics> StageMetrics { get; set; } = new();
        public List<WorkflowBottleneck> Bottlenecks { get; set; } = new();
        public decimal AverageTimeInStage { get; set; }
        public int TotalTransitions { get; set; }
    }

    public class PriorityAnalytics
    {
        public Dictionary<PriorityLevel, int> Distribution { get; set; } = new();
        public decimal AveragePriorityScore { get; set; }
        public List<PriorityTrend> Trends { get; set; } = new();
        public Dictionary<int, decimal> DepartmentVotingPatterns { get; set; } = new();
    }

    public class ResourceUtilizationAnalytics
    {
        public decimal OverallUtilization { get; set; }
        public Dictionary<int, decimal> DepartmentUtilization { get; set; } = new();
        public List<ResourceAllocation> Allocations { get; set; } = new();
        public List<CapacityGap> CapacityGaps { get; set; } = new();
    }

    public class SLAComplianceAnalytics
    {
        public decimal OverallComplianceRate { get; set; }
        public Dictionary<WorkCategory, decimal> ComplianceByCategory { get; set; } = new();
        public Dictionary<int, decimal> ComplianceByDepartment { get; set; } = new();
        public List<SLAViolation> Violations { get; set; } = new();
    }

    public class StageMetrics
    {
        public int RequestCount { get; set; }
        public decimal AverageTimeInStage { get; set; }
        public decimal CompletionRate { get; set; }
        public List<string> Blockers { get; set; } = new();
    }

    public class WorkflowBottleneck
    {
        public WorkflowStage Stage { get; set; }
        public decimal AverageTimeInStage { get; set; }
        public int RequestCount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class PriorityTrend
    {
        public DateTime Date { get; set; }
        public decimal AveragePriority { get; set; }
        public int RequestCount { get; set; }
    }

    public class ResourceAllocation
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int AllocatedHours { get; set; }
        public int AvailableHours { get; set; }
        public decimal UtilizationRate { get; set; }
    }

    public class CapacityGap
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int RequiredHours { get; set; }
        public int AvailableHours { get; set; }
        public int Gap { get; set; }
    }

    public class SLAViolation
    {
        public int WorkRequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public WorkflowStage Stage { get; set; }
        public int DaysOverdue { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class RecentActivity
    {
        public int WorkRequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public PriorityLevel? PriorityLevel { get; set; }
    }

    public class TeamMemberWorkload
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int AssignedRequests { get; set; }
        public int CompletedRequests { get; set; }
        public decimal UtilizationRate { get; set; }
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public string Group { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public int Count { get; set; }
    }
} 