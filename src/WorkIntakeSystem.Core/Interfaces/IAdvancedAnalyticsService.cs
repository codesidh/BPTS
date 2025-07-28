using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IAdvancedAnalyticsService
{
    // Predictive Analytics
    Task<PriorityPrediction> PredictWorkRequestPriorityAsync(WorkRequest workRequest);
    Task<WorkloadPrediction> PredictDepartmentWorkloadAsync(int departmentId, DateTime forecastDate);
    Task<List<WorkflowBottleneck>> IdentifyWorkflowBottlenecksAsync();
    Task<ResourceOptimizationSuggestion> GetResourceOptimizationSuggestionsAsync(int departmentId);
    
    // Business Intelligence Dashboards
    Task<ExecutiveDashboard> GetExecutiveDashboardAsync(DateTime startDate, DateTime endDate);
    Task<DepartmentDashboard> GetDepartmentDashboardAsync(int departmentId, DateTime startDate, DateTime endDate);
    Task<ProjectDashboard> GetProjectDashboardAsync(int projectId, DateTime startDate, DateTime endDate);
    
    // Custom Report Builder
    Task<CustomReport> BuildCustomReportAsync(CustomReportRequest request);
    Task<List<ReportTemplate>> GetReportTemplatesAsync();
    Task<string> SaveReportTemplateAsync(ReportTemplate template);
    Task<byte[]> ExportReportAsync(string reportId, ExportFormat format);
    
    // Data Export
    Task<byte[]> ExportDataAsync(DataExportRequest request);
    Task<string> ScheduleDataExportAsync(DataExportSchedule schedule);
    Task<List<DataExportHistory>> GetExportHistoryAsync();
}

public class PriorityPrediction
{
    public int WorkRequestId { get; set; }
    public double PredictedScore { get; set; }
    public double Confidence { get; set; }
    public List<string> InfluencingFactors { get; set; } = new();
    public DateTime PredictionDate { get; set; }
}

public class WorkloadPrediction
{
    public int DepartmentId { get; set; }
    public DateTime ForecastDate { get; set; }
    public double PredictedWorkload { get; set; }
    public double Confidence { get; set; }
    public List<WorkloadFactor> Factors { get; set; } = new();
}

public class WorkflowBottleneck
{
    public string Stage { get; set; } = string.Empty;
    public double AverageTimeInStage { get; set; }
    public int ItemsInStage { get; set; }
    public double BottleneckScore { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public class ResourceOptimizationSuggestion
{
    public int DepartmentId { get; set; }
    public string SuggestionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double PotentialImpact { get; set; }
    public string Priority { get; set; } = string.Empty;
}

public class ExecutiveDashboard
{
    public int TotalWorkRequests { get; set; }
    public double AverageCompletionTime { get; set; }
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public List<DepartmentMetrics> DepartmentMetrics { get; set; } = new();
    public List<TrendData> TrendData { get; set; } = new();
}

public class DepartmentDashboard
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int ActiveWorkRequests { get; set; }
    public double TeamUtilization { get; set; }
    public List<UserMetrics> UserMetrics { get; set; } = new();
    public List<WorkloadTrend> WorkloadTrends { get; set; } = new();
}

public class CustomReportRequest
{
    public string ReportName { get; set; } = string.Empty;
    public List<string> DataSources { get; set; } = new();
    public List<ReportFilter> Filters { get; set; } = new();
    public List<ReportColumn> Columns { get; set; } = new();
    public List<ReportGrouping> Groupings { get; set; } = new();
    public List<ReportChart> Charts { get; set; } = new();
}

public class DataExportRequest
{
    public string EntityType { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
    public ExportFormat Format { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// ExportFormat moved to Phase4Models.cs 