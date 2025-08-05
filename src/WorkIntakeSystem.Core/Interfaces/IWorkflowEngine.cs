using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IWorkflowEngine
    {
        // Existing methods
        Task<bool> CanAdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId);
        Task AdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId, string? comments = null);

        // Enhanced workflow management
        Task<IEnumerable<WorkflowStage>> GetAvailableTransitionsAsync(WorkRequest workRequest, int userId);
        Task<WorkflowTransition?> GetTransitionAsync(WorkRequest workRequest, WorkflowStage nextStage, int? businessVerticalId = null);
        Task<bool> ValidateTransitionConditionsAsync(WorkRequest workRequest, WorkflowTransition transition, int userId);

        // Auto-transition capabilities
        Task ProcessAutoTransitionsAsync();
        Task ProcessAutoTransitionsForWorkRequestAsync(int workRequestId);
        Task<bool> ShouldAutoTransitionAsync(WorkRequest workRequest, WorkflowTransition transition);

        // SLA tracking and notifications
        Task<SLAStatus> GetSLAStatusAsync(WorkRequest workRequest);
        Task<IEnumerable<WorkRequest>> GetSLAViolationsAsync(DateTime? asOfDate = null);
        Task ProcessSLANotificationsAsync();
        Task NotifyStakeholdersAsync(WorkRequest workRequest, WorkflowTransition transition, string eventType);

        // Workflow state management
        Task<WorkflowState> GetWorkflowStateAsync(int workRequestId);
        Task<IEnumerable<WorkflowState>> GetWorkflowHistoryAsync(int workRequestId);
        Task<bool> ReplayWorkflowStateAsync(int workRequestId, DateTime targetDate);
        Task<WorkflowValidationResult> ValidateWorkflowConfigurationAsync(int? businessVerticalId = null);

        // Business rule evaluation
        Task<bool> EvaluateBusinessRuleAsync(string ruleScript, WorkRequest workRequest, int userId);
        Task<ApprovalResult> ProcessApprovalWorkflowAsync(WorkRequest workRequest, int approverId, bool approved, string? comments = null);
        Task<IEnumerable<WorkRequest>> GetPendingApprovalsAsync(int userId);

        // Workflow metrics and analytics
        Task<WorkflowMetrics> GetWorkflowMetricsAsync(DateTime fromDate, DateTime toDate, int? businessVerticalId = null);
        Task<IEnumerable<WorkflowBottleneckAnalysis>> IdentifyBottlenecksAsync(int? businessVerticalId = null);
        Task<double> GetAverageCompletionTimeAsync(WorkflowStage stage, int? businessVerticalId = null);
    }

    // Supporting classes
    public class SLAStatus
    {
        public int WorkRequestId { get; set; }
        public WorkflowStage CurrentStage { get; set; }
        public DateTime StageEntryTime { get; set; }
        public int? SLAHours { get; set; }
        public DateTime? SLADeadline { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public bool IsViolated { get; set; }
        public bool IsAtRisk { get; set; } // Within 25% of deadline
        public string Status { get; set; } = string.Empty;
    }

    public class WorkflowState
    {
        public int WorkRequestId { get; set; }
        public WorkflowStage Stage { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public int UserId { get; set; }
        public string? Comments { get; set; }
        public TimeSpan? Duration { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class WorkflowValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
    }

    public class ApprovalResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public WorkflowStage? NextStage { get; set; }
        public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;
    }

    public class WorkflowMetrics
    {
        public int TotalWorkRequests { get; set; }
        public int CompletedWorkRequests { get; set; }
        public double AverageCompletionDays { get; set; }
        public Dictionary<WorkflowStage, int> StageDistribution { get; set; } = new();
        public Dictionary<WorkflowStage, double> AverageStageTime { get; set; } = new();
        public int SLAViolations { get; set; }
        public double SLAComplianceRate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class WorkflowBottleneckAnalysis
    {
        public WorkflowStage Stage { get; set; }
        public int PendingCount { get; set; }
        public double AverageWaitTime { get; set; }
        public string BottleneckType { get; set; } = string.Empty; // Resource, Approval, System
        public string Recommendation { get; set; } = string.Empty;
    }
} 