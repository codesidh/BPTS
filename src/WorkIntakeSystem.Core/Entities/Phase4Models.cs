namespace WorkIntakeSystem.Core.Entities;

public enum ExportFormat
{
    Excel,
    CSV,
    JSON,
    PDF
}

// Analytics Models
public class ProjectDashboard
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public double CompletionPercentage { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksRemaining { get; set; }
    public int TeamMembers { get; set; }
    public decimal Budget { get; set; }
    public decimal BudgetUsed { get; set; }
    public List<ProjectMilestone> Milestones { get; set; } = new();
    public List<ProjectRisk> Risks { get; set; } = new();
}

public class ProjectMilestone
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ProjectRisk
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Probability { get; set; } = string.Empty;
    public string MitigationPlan { get; set; } = string.Empty;
}

public class WorkloadFactor
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Value { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class DepartmentMetrics
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int ActiveWorkRequests { get; set; }
    public int CompletedWorkRequests { get; set; }
    public double AverageCompletionTime { get; set; }
    public double TeamUtilization { get; set; }
    public int TeamSize { get; set; }
}

public class TrendData
{
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class UserMetrics
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int AssignedWorkRequests { get; set; }
    public int CompletedWorkRequests { get; set; }
    public double AverageCompletionTime { get; set; }
    public double ProductivityScore { get; set; }
}

public class WorkloadTrend
{
    public DateTime Date { get; set; }
    public double Workload { get; set; }
    public double Capacity { get; set; }
    public double UtilizationRate { get; set; }
}

// Report Builder Models
public class CustomReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public object Data { get; set; } = new();
    public List<ReportChart> Charts { get; set; } = new();
    public List<ReportFilter> Filters { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
}

public class ReportTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<ReportFilter> DefaultFilters { get; set; } = new();
    public List<ReportColumn> DefaultColumns { get; set; } = new();
    public List<ReportChart> DefaultCharts { get; set; } = new();
}

public class ReportFilter
{
    public string Id { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public string DisplayName { get; set; } = string.Empty;
}

public class ReportColumn
{
    public string Id { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public int Order { get; set; }
    public string Format { get; set; } = string.Empty;
}

public class ReportGrouping
{
    public string Id { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class ReportChart
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public string XAxisField { get; set; } = string.Empty;
    public string YAxisField { get; set; } = string.Empty;
    public List<string> SeriesFields { get; set; } = new();
    public Dictionary<string, object> Options { get; set; } = new();
}

// Data Export Models
public class DataExportSchedule
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
    public ExportFormat Format { get; set; }
    public string Schedule { get; set; } = string.Empty; // Cron expression
    public string EmailRecipients { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class DataExportHistory
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public ExportFormat Format { get; set; }
    public DateTime ExportDate { get; set; }
    public int RecordCount { get; set; }
    public long FileSizeBytes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
}

// Microsoft 365 Integration Models
public class SharePointDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
}

public class PowerBIReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string WorkspaceId { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string DatasetId { get; set; } = string.Empty;
}

// DevOps Integration Models
public class AzureDevOpsWorkItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string WorkItemType { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime ChangedDate { get; set; }
    public string Url { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
}

public class JiraIssue
{
    public string Key { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string Assignee { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public string Url { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
}

public class IntegrationSyncResult
{
    public string Id { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
    public string IntegrationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SyncDate { get; set; }
    public string? ErrorMessage { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public DateTime SyncTime { get; set; }
    public bool Success { get; set; }
}

// Analytics Models
public class PriorityPrediction
{
    public int WorkRequestId { get; set; }
    public double PredictedScore { get; set; }
    public double Confidence { get; set; }
    public List<string> Factors { get; set; } = new();
    public List<string> InfluencingFactors { get; set; } = new();
    public DateTime PredictionDate { get; set; }
}

public class WorkloadPrediction
{
    public int DepartmentId { get; set; }
    public double PredictedWorkload { get; set; }
    public double Confidence { get; set; }
    public DateTime ForecastDate { get; set; }
    public List<string> Factors { get; set; } = new();
}

public class WorkflowBottleneck
{
    public string Id { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public double AverageWaitTime { get; set; }
    public int PendingItems { get; set; }
    public string Impact { get; set; } = string.Empty;
    public double AverageTimeInStage { get; set; }
    public int ItemsInStage { get; set; }
    public double BottleneckScore { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public class ResourceOptimizationSuggestion
{
    public int DepartmentId { get; set; }
    public string Suggestion { get; set; } = string.Empty;
    public double ExpectedImpact { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string SuggestionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double PotentialImpact { get; set; }
}

public class ExecutiveDashboard
{
    public int TotalWorkRequests { get; set; }
    public int ActiveWorkRequests { get; set; }
    public int CompletedWorkRequests { get; set; }
    public double AverageCompletionTime { get; set; }
    public double OverallEfficiency { get; set; }
    public List<DepartmentMetrics> DepartmentMetrics { get; set; } = new();
    public List<TrendData> Trends { get; set; } = new();
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public List<TrendData> TrendData { get; set; } = new();
}

public class DepartmentDashboard
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int ActiveWorkRequests { get; set; }
    public int CompletedWorkRequests { get; set; }
    public double AverageCompletionTime { get; set; }
    public double TeamUtilization { get; set; }
    public List<UserMetrics> TeamMembers { get; set; } = new();
    public List<WorkloadTrend> WorkloadTrends { get; set; } = new();
    public List<UserMetrics> UserMetrics { get; set; } = new();
}

public class CustomReportRequest
{
    public string ReportName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
    public List<ReportGrouping> Groupings { get; set; } = new();
    public List<ReportChart> Charts { get; set; } = new();
}

public class DataExportRequest
{
    public string EntityType { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
    public ExportFormat Format { get; set; }
}

// Mobile and Accessibility Models
public class PWAManifest
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string StartUrl { get; set; } = string.Empty;
    public List<PWAIcon> Icons { get; set; } = new();
}

public class PWAIcon
{
    public string Src { get; set; } = string.Empty;
    public string Sizes { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}

public class ServiceWorkerConfig
{
    public string Version { get; set; } = string.Empty;
    public List<string> CachedUrls { get; set; } = new();
    public List<string> NetworkOnlyUrls { get; set; } = new();
    public List<string> CacheFirstUrls { get; set; } = new();
    public List<string> CacheUrls { get; set; } = new();
    public List<string> CacheStrategies { get; set; } = new();
    public int CacheMaxAge { get; set; }
}

public class OfflineResource
{
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public bool IsCritical { get; set; }
}

public class OfflineWorkRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public bool IsSynced { get; set; }
    public DateTime LastSynced { get; set; }
    public bool HasPendingChanges { get; set; }
}

public class OfflineAction
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public bool IsProcessed { get; set; }
    public bool IsSynced { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AccessibilityProfile
{
    public string UserId { get; set; } = string.Empty;
    public double FontScale { get; set; } = 1.0;
    public bool HighContrast { get; set; }
    public bool ScreenReader { get; set; }
    public bool ReducedMotion { get; set; }
    public string ColorScheme { get; set; } = string.Empty;
    public bool ScreenReaderEnabled { get; set; }
    public bool KeyboardNavigation { get; set; }
    public List<string> PreferredColorSchemes { get; set; } = new();
}

public class AccessibilityReport
{
    public double ComplianceScore { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class AccessibilityIssue
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Element { get; set; } = string.Empty;
    public string WCAGCriterion { get; set; } = string.Empty;
}

public class AccessibilityRecommendation
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Implementation { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string EstimatedEffort { get; set; } = string.Empty;
}

public class MobileConfiguration
{
    public bool PushNotificationsEnabled { get; set; }
    public bool LocationServicesEnabled { get; set; }
    public bool OfflineModeEnabled { get; set; }
    public string DefaultLanguage { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
    public int OfflineSyncInterval { get; set; }
    public bool BiometricAuthEnabled { get; set; }
    public Dictionary<string, bool> FeatureFlags { get; set; } = new();
}

public class MobileNotification
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime? ScheduledAt { get; set; }
} 