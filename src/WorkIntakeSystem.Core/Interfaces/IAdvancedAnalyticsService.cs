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