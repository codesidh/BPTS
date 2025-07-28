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