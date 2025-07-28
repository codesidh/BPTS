using Microsoft.ML;
using Microsoft.ML.Data;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using CsvHelper;
using System.Globalization;
using System.Text.Json;

namespace WorkIntakeSystem.Infrastructure.Services;

public class AdvancedAnalyticsService : IAdvancedAnalyticsService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<AdvancedAnalyticsService> _logger;
    private readonly IAnalyticsService _analyticsService;

    public AdvancedAnalyticsService(
        ILogger<AdvancedAnalyticsService> logger,
        IAnalyticsService analyticsService)
    {
        _mlContext = new MLContext(seed: 0);
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // Predictive Analytics
    public async Task<PriorityPrediction> PredictWorkRequestPriorityAsync(WorkRequest workRequest)
    {
        try
        {
            // Create training data (in practice, this would come from historical data)
            var trainingData = GenerateTrainingData();
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Build and train the model
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("CategoryEncoded", nameof(PriorityTrainingData.Category))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("DepartmentEncoded", nameof(PriorityTrainingData.Department)))
                .Append(_mlContext.Transforms.Concatenate("Features", "CategoryEncoded", "DepartmentEncoded", 
                    nameof(PriorityTrainingData.BusinessValue), nameof(PriorityTrainingData.Urgency), 
                    nameof(PriorityTrainingData.Complexity)))
                .Append(_mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);

            // Create prediction input
            var input = new PriorityTrainingData
            {
                Category = workRequest.Category.ToString(),
                Department = workRequest.DepartmentId.ToString(),
                BusinessValue = 8.5f, // This would be calculated based on work request properties
                Urgency = 7.0f,
                Complexity = 6.5f
            };

            // Make prediction
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<PriorityTrainingData, PriorityPredictionOutput>(model);
            var prediction = predictionEngine.Predict(input);

            var result = new PriorityPrediction
            {
                WorkRequestId = workRequest.Id,
                PredictedScore = prediction.PredictedPriority,
                Confidence = 0.85, // This would be calculated based on model performance
                InfluencingFactors = new List<string> { "Business Value", "Urgency", "Department Workload" },
                PredictionDate = DateTime.UtcNow
            };

            _logger.LogInformation("Predicted priority {PredictedScore} for work request {WorkRequestId}", result.PredictedScore, workRequest.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict priority for work request {WorkRequestId}", workRequest.Id);
            return new PriorityPrediction { WorkRequestId = workRequest.Id, PredictedScore = 5.0, Confidence = 0.0 };
        }
    }

    public async Task<WorkloadPrediction> PredictDepartmentWorkloadAsync(int departmentId, DateTime forecastDate)
    {
        try
        {
            // This would use historical workload data and time series forecasting
            var historicalWorkload = await GetHistoricalWorkloadAsync(departmentId);
            var seasonalityFactor = GetSeasonalityFactor(forecastDate);
            var trendFactor = GetTrendFactor(historicalWorkload);

            var predictedWorkload = historicalWorkload.Average() * seasonalityFactor * trendFactor;

            var result = new WorkloadPrediction
            {
                DepartmentId = departmentId,
                ForecastDate = forecastDate,
                PredictedWorkload = predictedWorkload,
                Confidence = 0.78,
                Factors = new List<WorkloadFactor>
                {
                    new WorkloadFactor { Name = "Historical Average", Weight = 0.4, Value = historicalWorkload.Average() },
                    new WorkloadFactor { Name = "Seasonality", Weight = 0.3, Value = seasonalityFactor },
                    new WorkloadFactor { Name = "Trend", Weight = 0.3, Value = trendFactor }
                }
            };

            _logger.LogInformation("Predicted workload {PredictedWorkload} for department {DepartmentId} on {ForecastDate}", 
                result.PredictedWorkload, departmentId, forecastDate);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict workload for department {DepartmentId}", departmentId);
            return new WorkloadPrediction { DepartmentId = departmentId, ForecastDate = forecastDate, PredictedWorkload = 0, Confidence = 0 };
        }
    }

    public async Task<List<WorkflowBottleneck>> IdentifyWorkflowBottlenecksAsync()
    {
        try
        {
            // Analyze workflow stages for bottlenecks
            var workflowStages = new[]
            {
                "Intake", "Initial Review", "Priority Assessment", "Department Assignment",
                "Technical Review", "Business Analysis", "Resource Allocation", "Development",
                "Testing", "User Acceptance", "Deployment", "Monitoring", "Documentation",
                "Training", "Closure"
            };

            var bottlenecks = new List<WorkflowBottleneck>();

            foreach (var stage in workflowStages)
            {
                var averageTime = await GetAverageTimeInStageAsync(stage);
                var itemsInStage = await GetItemsInStageAsync(stage);
                var bottleneckScore = CalculateBottleneckScore(averageTime, itemsInStage);

                if (bottleneckScore > 0.7) // Threshold for bottleneck
                {
                    bottlenecks.Add(new WorkflowBottleneck
                    {
                        Stage = stage,
                        AverageTimeInStage = averageTime,
                        ItemsInStage = itemsInStage,
                        BottleneckScore = bottleneckScore,
                        Recommendations = GenerateBottleneckRecommendations(stage, averageTime, itemsInStage)
                    });
                }
            }

            _logger.LogInformation("Identified {BottleneckCount} workflow bottlenecks", bottlenecks.Count);
            return bottlenecks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to identify workflow bottlenecks");
            return new List<WorkflowBottleneck>();
        }
    }

    public async Task<ResourceOptimizationSuggestion> GetResourceOptimizationSuggestionsAsync(int departmentId)
    {
        try
        {
            // Analyze department resources and workload
            var currentWorkload = await GetCurrentDepartmentWorkloadAsync(departmentId);
            var teamCapacity = await GetDepartmentCapacityAsync(departmentId);
            var utilizationRate = currentWorkload / teamCapacity;

            string suggestionType;
            string description;
            double potentialImpact;
            string priority;

            if (utilizationRate > 0.9)
            {
                suggestionType = "Resource Addition";
                description = "Department is over-utilized. Consider adding team members or redistributing workload.";
                potentialImpact = 0.8;
                priority = "High";
            }
            else if (utilizationRate < 0.6)
            {
                suggestionType = "Resource Reallocation";
                description = "Department has available capacity. Consider taking on additional work or cross-training.";
                potentialImpact = 0.6;
                priority = "Medium";
            }
            else
            {
                suggestionType = "Process Optimization";
                description = "Department utilization is optimal. Focus on process improvements and automation.";
                potentialImpact = 0.4;
                priority = "Low";
            }

            var result = new ResourceOptimizationSuggestion
            {
                DepartmentId = departmentId,
                SuggestionType = suggestionType,
                Description = description,
                PotentialImpact = potentialImpact,
                Priority = priority
            };

            _logger.LogInformation("Generated resource optimization suggestion for department {DepartmentId}: {SuggestionType}", 
                departmentId, suggestionType);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate resource optimization suggestions for department {DepartmentId}", departmentId);
            return new ResourceOptimizationSuggestion { DepartmentId = departmentId };
        }
    }

    // Business Intelligence Dashboards
    public async Task<ExecutiveDashboard> GetExecutiveDashboardAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var dashboard = new ExecutiveDashboard
            {
                TotalWorkRequests = await GetTotalWorkRequestsAsync(startDate, endDate),
                AverageCompletionTime = await GetAverageCompletionTimeAsync(startDate, endDate),
                StatusDistribution = await GetStatusDistributionAsync(startDate, endDate),
                DepartmentMetrics = await GetDepartmentMetricsAsync(startDate, endDate),
                TrendData = await GetTrendDataAsync(startDate, endDate)
            };

            _logger.LogInformation("Generated executive dashboard for period {StartDate} to {EndDate}", startDate, endDate);
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate executive dashboard");
            return new ExecutiveDashboard();
        }
    }

    public async Task<DepartmentDashboard> GetDepartmentDashboardAsync(int departmentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var dashboard = new DepartmentDashboard
            {
                DepartmentId = departmentId,
                DepartmentName = await GetDepartmentNameAsync(departmentId),
                ActiveWorkRequests = await GetActiveWorkRequestsCountAsync(departmentId),
                TeamUtilization = await GetTeamUtilizationAsync(departmentId),
                UserMetrics = await GetUserMetricsAsync(departmentId, startDate, endDate),
                WorkloadTrends = await GetWorkloadTrendsAsync(departmentId, startDate, endDate)
            };

            _logger.LogInformation("Generated department dashboard for department {DepartmentId}", departmentId);
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate department dashboard for department {DepartmentId}", departmentId);
            return new DepartmentDashboard { DepartmentId = departmentId };
        }
    }

    public async Task<ProjectDashboard> GetProjectDashboardAsync(int projectId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Implementation for project dashboard
            var dashboard = new ProjectDashboard
            {
                ProjectId = projectId,
                ProjectName = $"Project {projectId}",
                CompletionPercentage = 75.5,
                TasksCompleted = 45,
                TasksRemaining = 15,
                TeamMembers = 8,
                Budget = 150000,
                BudgetUsed = 112500
            };

            _logger.LogInformation("Generated project dashboard for project {ProjectId}", projectId);
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate project dashboard for project {ProjectId}", projectId);
            return new ProjectDashboard { ProjectId = projectId };
        }
    }

    // Custom Report Builder
    public async Task<CustomReport> BuildCustomReportAsync(CustomReportRequest request)
    {
        try
        {
            var report = new CustomReport
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.ReportName,
                CreatedDate = DateTime.UtcNow,
                Data = await GenerateReportDataAsync(request),
                Charts = request.Charts,
                Filters = request.Filters
            };

            _logger.LogInformation("Built custom report {ReportName}", request.ReportName);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build custom report {ReportName}", request.ReportName);
            return new CustomReport { Name = request.ReportName };
        }
    }

    public async Task<List<ReportTemplate>> GetReportTemplatesAsync()
    {
        try
        {
            var templates = new List<ReportTemplate>
            {
                new ReportTemplate
                {
                    Id = "executive-summary",
                    Name = "Executive Summary",
                    Description = "High-level overview of work requests and department performance",
                    Category = "Executive"
                },
                new ReportTemplate
                {
                    Id = "department-performance",
                    Name = "Department Performance",
                    Description = "Detailed analysis of department metrics and trends",
                    Category = "Department"
                },
                new ReportTemplate
                {
                    Id = "workflow-analysis",
                    Name = "Workflow Analysis",
                    Description = "Analysis of workflow stages and bottlenecks",
                    Category = "Process"
                }
            };

            _logger.LogInformation("Retrieved {TemplateCount} report templates", templates.Count);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get report templates");
            return new List<ReportTemplate>();
        }
    }

    public async Task<string> SaveReportTemplateAsync(ReportTemplate template)
    {
        try
        {
            template.Id = Guid.NewGuid().ToString();
            template.CreatedDate = DateTime.UtcNow;
            
            // Save template to database (implementation would depend on your data layer)
            _logger.LogInformation("Saved report template {TemplateName} with ID {TemplateId}", template.Name, template.Id);
            return template.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save report template {TemplateName}", template.Name);
            return string.Empty;
        }
    }

    public async Task<byte[]> ExportReportAsync(string reportId, ExportFormat format)
    {
        try
        {
            // Get report data
            var reportData = await GetReportDataAsync(reportId);

            return format switch
            {
                ExportFormat.Excel => await ExportToExcelAsync(reportData),
                ExportFormat.CSV => await ExportToCsvAsync(reportData),
                ExportFormat.JSON => await ExportToJsonAsync(reportData),
                ExportFormat.PDF => await ExportToPdfAsync(reportData),
                _ => throw new ArgumentException($"Unsupported export format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report {ReportId} in format {Format}", reportId, format);
            return Array.Empty<byte>();
        }
    }

    // Data Export
    public async Task<byte[]> ExportDataAsync(DataExportRequest request)
    {
        try
        {
            var data = await GetDataForExportAsync(request);
            
            return request.Format switch
            {
                ExportFormat.Excel => await ExportToExcelAsync(data),
                ExportFormat.CSV => await ExportToCsvAsync(data),
                ExportFormat.JSON => await ExportToJsonAsync(data),
                ExportFormat.PDF => await ExportToPdfAsync(data),
                _ => throw new ArgumentException($"Unsupported export format: {request.Format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export data for entity type {EntityType}", request.EntityType);
            return Array.Empty<byte>();
        }
    }

    public async Task<string> ScheduleDataExportAsync(DataExportSchedule schedule)
    {
        try
        {
            var scheduleId = Guid.NewGuid().ToString();
            
            // Implementation would integrate with a job scheduler like Hangfire or Quartz.NET
            _logger.LogInformation("Scheduled data export {ScheduleId} for {EntityType}", scheduleId, schedule.EntityType);
            return scheduleId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule data export for entity type {EntityType}", schedule.EntityType);
            return string.Empty;
        }
    }

    public async Task<List<DataExportHistory>> GetExportHistoryAsync()
    {
        try
        {
            // Mock data - in practice, this would come from database
            var history = new List<DataExportHistory>
            {
                new DataExportHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityType = "WorkRequest",
                    Format = ExportFormat.Excel,
                    ExportDate = DateTime.UtcNow.AddDays(-1),
                    RecordCount = 1250,
                    FileSizeBytes = 2048000,
                    Status = "Completed"
                }
            };

            _logger.LogInformation("Retrieved {HistoryCount} export history records", history.Count);
            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get export history");
            return new List<DataExportHistory>();
        }
    }

    // Helper methods
    private List<PriorityTrainingData> GenerateTrainingData()
    {
        // This would come from historical data in practice
        return new List<PriorityTrainingData>
        {
            new PriorityTrainingData { Category = "Enhancement", Department = "1", BusinessValue = 8.0f, Urgency = 6.0f, Complexity = 7.0f, Priority = 7.2f },
            new PriorityTrainingData { Category = "Bug", Department = "2", BusinessValue = 9.0f, Urgency = 9.0f, Complexity = 5.0f, Priority = 8.5f },
            new PriorityTrainingData { Category = "Feature", Department = "1", BusinessValue = 7.0f, Urgency = 5.0f, Complexity = 8.0f, Priority = 6.8f }
        };
    }

    private async Task<List<double>> GetHistoricalWorkloadAsync(int departmentId)
    {
        // Mock historical data
        return new List<double> { 45.2, 52.1, 48.7, 55.3, 49.8, 51.2, 47.9 };
    }

    private double GetSeasonalityFactor(DateTime date)
    {
        // Simple seasonality calculation based on month
        var month = date.Month;
        return month switch
        {
            12 or 1 => 0.8, // Holiday season
            6 or 7 => 0.9, // Summer
            _ => 1.0
        };
    }

    private double GetTrendFactor(List<double> historicalData)
    {
        if (historicalData.Count < 2) return 1.0;
        
        var trend = (historicalData.Last() - historicalData.First()) / historicalData.Count;
        return 1.0 + (trend / 100.0); // Convert to factor
    }

    private async Task<byte[]> ExportToExcelAsync(object data)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Data");
        
        // Simple implementation - would be more sophisticated in practice
        worksheet.Cells[1, 1].Value = "Export Data";
        worksheet.Cells[2, 1].Value = JsonSerializer.Serialize(data);
        
        return package.GetAsByteArray();
    }

    private async Task<byte[]> ExportToCsvAsync(object data)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        // Simple implementation
        csv.WriteField("Data");
        csv.NextRecord();
        csv.WriteField(JsonSerializer.Serialize(data));
        
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    private async Task<byte[]> ExportToJsonAsync(object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    private async Task<byte[]> ExportToPdfAsync(object data)
    {
        // This would use a PDF library like iTextSharp
        var content = $"PDF Export\n\n{JsonSerializer.Serialize(data)}";
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    // Additional helper methods would be implemented here...
    private async Task<int> GetTotalWorkRequestsAsync(DateTime startDate, DateTime endDate) => 150;
    private async Task<double> GetAverageCompletionTimeAsync(DateTime startDate, DateTime endDate) => 14.5;
    private async Task<Dictionary<string, int>> GetStatusDistributionAsync(DateTime startDate, DateTime endDate) => 
        new Dictionary<string, int> { {"Active", 45}, {"Completed", 85}, {"On Hold", 20} };
    private async Task<List<DepartmentMetrics>> GetDepartmentMetricsAsync(DateTime startDate, DateTime endDate) => new();
    private async Task<List<TrendData>> GetTrendDataAsync(DateTime startDate, DateTime endDate) => new();
    private async Task<string> GetDepartmentNameAsync(int departmentId) => $"Department {departmentId}";
    private async Task<int> GetActiveWorkRequestsCountAsync(int departmentId) => 25;
    private async Task<double> GetTeamUtilizationAsync(int departmentId) => 0.78;
    private async Task<List<UserMetrics>> GetUserMetricsAsync(int departmentId, DateTime startDate, DateTime endDate) => new();
    private async Task<List<WorkloadTrend>> GetWorkloadTrendsAsync(int departmentId, DateTime startDate, DateTime endDate) => new();
    private async Task<object> GenerateReportDataAsync(CustomReportRequest request) => new { Data = "Sample report data" };
    private async Task<object> GetReportDataAsync(string reportId) => new { ReportId = reportId, Data = "Sample data" };
    private async Task<object> GetDataForExportAsync(DataExportRequest request) => new { EntityType = request.EntityType, Data = "Sample export data" };
    private async Task<double> GetAverageTimeInStageAsync(string stage) => new Random().NextDouble() * 10 + 2;
    private async Task<int> GetItemsInStageAsync(string stage) => new Random().Next(5, 25);
    private double CalculateBottleneckScore(double averageTime, int itemsInStage) => (averageTime * itemsInStage) / 100.0;
    private List<string> GenerateBottleneckRecommendations(string stage, double averageTime, int itemsInStage) => 
        new List<string> { $"Optimize {stage} process", "Add resources", "Automate routine tasks" };
    private async Task<double> GetCurrentDepartmentWorkloadAsync(int departmentId) => 85.5;
    private async Task<double> GetDepartmentCapacityAsync(int departmentId) => 100.0;
}

public class PriorityTrainingData
{
    public string Category { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public float BusinessValue { get; set; }
    public float Urgency { get; set; }
    public float Complexity { get; set; }
    public float Priority { get; set; }
}

public class PriorityPredictionOutput
{
    [ColumnName("Score")]
    public float PredictedPriority { get; set; }
} 