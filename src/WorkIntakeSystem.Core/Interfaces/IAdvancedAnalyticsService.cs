using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IAdvancedAnalyticsService
{
    // Priority Prediction
    Task<PriorityPrediction> PredictPriorityAsync(int workRequestId);
    Task<IEnumerable<PriorityTrend>> PredictPriorityTrendsAsync(int departmentId, DateTime targetDate);
    
    // Resource Forecasting
    Task<ResourceForecast> ForecastResourceNeedsAsync(int departmentId, DateTime targetDate);
    Task<CapacityPrediction> PredictCapacityUtilizationAsync(int departmentId, DateTime targetDate);
    
    // Completion Prediction
    Task<CompletionPrediction> PredictCompletionTimeAsync(int workRequestId);
    Task<IEnumerable<CompletionTrend>> PredictCompletionTrendsAsync(int departmentId, DateTime targetDate);
    
    // Business Value Analysis
    Task<BusinessValueROI> CalculateROIAsync(int workRequestId);
    Task<IEnumerable<BusinessValueTrend>> AnalyzeBusinessValueTrendsAsync(int businessVerticalId, DateTime fromDate, DateTime toDate);
    
    // Risk Assessment
    Task<RiskAssessment> AssessProjectRiskAsync(int workRequestId);
    Task<IEnumerable<RiskIndicator>> GetRiskIndicatorsAsync(int departmentId);
    
    // Predictive Insights
    Task<IEnumerable<PredictiveInsight>> GetPredictiveInsightsAsync(int businessVerticalId);
    Task<WorkloadPrediction> PredictWorkloadAsync(int departmentId, DateTime targetDate);
}

public class PriorityPrediction
{
    public int WorkRequestId { get; set; }
    public decimal PredictedPriority { get; set; }
    public PriorityLevel PredictedLevel { get; set; }
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public DateTime PredictedDate { get; set; }
}

public class ResourceForecast
{
    public int DepartmentId { get; set; }
    public DateTime TargetDate { get; set; }
    public int PredictedCapacity { get; set; }
    public int PredictedDemand { get; set; }
    public decimal UtilizationRate { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class CompletionPrediction
{
    public int WorkRequestId { get; set; }
    public DateTime PredictedCompletionDate { get; set; }
    public decimal Confidence { get; set; }
    public string Factors { get; set; } = string.Empty;
}

public class BusinessValueROI
{
    public int WorkRequestId { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal EstimatedValue { get; set; }
    public decimal ROI { get; set; }
    public decimal PaybackPeriod { get; set; }
    public string Analysis { get; set; } = string.Empty;
}

public class RiskAssessment
{
    public int WorkRequestId { get; set; }
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public string MitigationStrategy { get; set; } = string.Empty;
}

public class PredictiveInsight
{
    public string InsightType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public DateTime PredictedDate { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class WorkloadPrediction
{
    public int DepartmentId { get; set; }
    public DateTime TargetDate { get; set; }
    public decimal PredictedUtilization { get; set; }
    public int PredictedWorkItems { get; set; }
    public string Trend { get; set; } = string.Empty;
}

public class CapacityPrediction
{
    public int DepartmentId { get; set; }
    public DateTime TargetDate { get; set; }
    public decimal PredictedCapacity { get; set; }
    public decimal CurrentUtilization { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class PriorityTrend
{
    public int DepartmentId { get; set; }
    public DateTime Date { get; set; }
    public decimal AveragePriority { get; set; }
    public int WorkRequestCount { get; set; }
    public string Trend { get; set; } = string.Empty;
}

public class CompletionTrend
{
    public int DepartmentId { get; set; }
    public DateTime Date { get; set; }
    public decimal AverageCompletionTime { get; set; }
    public int CompletedWorkItems { get; set; }
    public string Trend { get; set; } = string.Empty;
}

public class BusinessValueTrend
{
    public int BusinessVerticalId { get; set; }
    public DateTime Date { get; set; }
    public decimal AverageBusinessValue { get; set; }
    public decimal TotalROI { get; set; }
    public int WorkRequestCount { get; set; }
    public string Trend { get; set; } = string.Empty;
}

public class RiskIndicator
{
    public int DepartmentId { get; set; }
    public string RiskType { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MitigationAction { get; set; } = string.Empty;
} 