using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IPriorityConfigurationService
{
    // CRUD Operations
    Task<PriorityConfiguration> CreateConfigurationAsync(PriorityConfiguration configuration);
    Task<PriorityConfiguration?> GetConfigurationAsync(int configurationId);
    Task<PriorityConfiguration?> GetConfigurationByBusinessVerticalAsync(int businessVerticalId, string priorityName);
    Task<IEnumerable<PriorityConfiguration>> GetAllConfigurationsAsync();
    Task<IEnumerable<PriorityConfiguration>> GetConfigurationsByBusinessVerticalAsync(int businessVerticalId);
    Task<PriorityConfiguration> UpdateConfigurationAsync(PriorityConfiguration configuration);
    Task<bool> DeleteConfigurationAsync(int configurationId);

    // Algorithm Management
    Task<PriorityAlgorithmConfig> GetAlgorithmConfigAsync(int businessVerticalId);
    Task<bool> SetAlgorithmConfigAsync(int businessVerticalId, PriorityAlgorithmConfig config);
    Task<decimal> TestPriorityCalculationAsync(int businessVerticalId, WorkRequest testRequest);
    Task<PriorityPreviewResult> PreviewPriorityChangesAsync(int businessVerticalId, PriorityAlgorithmConfig newConfig);

    // Time Decay Configuration
    Task<TimeDecayConfig> GetTimeDecayConfigAsync(int businessVerticalId);
    Task<bool> SetTimeDecayConfigAsync(int businessVerticalId, TimeDecayConfig config);
    Task<decimal> CalculateTimeDecayFactorAsync(DateTime createdDate, TimeDecayConfig config);

    // Business Value Weights
    Task<BusinessValueWeightConfig> GetBusinessValueWeightsAsync(int businessVerticalId);
    Task<bool> SetBusinessValueWeightsAsync(int businessVerticalId, BusinessValueWeightConfig config);

    // Capacity Adjustment Factors
    Task<CapacityAdjustmentConfig> GetCapacityAdjustmentConfigAsync(int businessVerticalId);
    Task<bool> SetCapacityAdjustmentConfigAsync(int businessVerticalId, CapacityAdjustmentConfig config);

    // Auto-adjustment Rules
    Task<AutoAdjustmentRulesConfig> GetAutoAdjustmentRulesAsync(int businessVerticalId);
    Task<bool> SetAutoAdjustmentRulesAsync(int businessVerticalId, AutoAdjustmentRulesConfig config);
    Task<bool> ProcessAutoAdjustmentsAsync(int? businessVerticalId = null);

    // Escalation and SLA Management
    Task<EscalationRulesConfig> GetEscalationRulesAsync(int businessVerticalId);
    Task<bool> SetEscalationRulesAsync(int businessVerticalId, EscalationRulesConfig config);
    Task<IEnumerable<PriorityEscalation>> GetPendingEscalationsAsync(int? businessVerticalId = null);
    Task<bool> ProcessEscalationsAsync(int? businessVerticalId = null);

    // Validation and Testing
    Task<PriorityConfigValidationResult> ValidateConfigurationAsync(PriorityConfiguration configuration);
    Task<IEnumerable<string>> GetValidationErrorsAsync(PriorityConfiguration configuration);
    Task<bool> IsConfigurationValidAsync(int configurationId);

    // Analytics and Insights
    Task<PriorityTrendAnalysis> GetPriorityTrendsAsync(int businessVerticalId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<PriorityEffectivenessMetric>> GetEffectivenessMetricsAsync(int businessVerticalId);
    Task<PriorityRecommendation> GetConfigurationRecommendationsAsync(int businessVerticalId);
}

// Supporting configuration classes
public class PriorityAlgorithmConfig
{
    public string AlgorithmType { get; set; } = "Enhanced"; // Enhanced, Simple, Custom
    public decimal BaseWeight { get; set; } = 1.0m;
    public decimal TimeDecayWeight { get; set; } = 1.0m;
    public decimal BusinessValueWeight { get; set; } = 1.0m;
    public decimal CapacityAdjustmentWeight { get; set; } = 1.0m;
    public Dictionary<string, decimal> CustomWeights { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ModifiedBy { get; set; } = string.Empty;
}

public class TimeDecayConfig
{
    public bool IsEnabled { get; set; } = true;
    public decimal MaxMultiplier { get; set; } = 2.0m;
    public decimal DecayRate { get; set; } = 0.01m; // Daily decay rate
    public int StartDelayDays { get; set; } = 7; // Days before decay starts
    public string DecayFunction { get; set; } = "Logarithmic"; // Linear, Logarithmic, Exponential
    public Dictionary<string, decimal> FunctionParameters { get; set; } = new();
}

public class BusinessValueWeightConfig
{
    public decimal BaseMultiplier { get; set; } = 1.0m;
    public Dictionary<string, decimal> CategoryWeights { get; set; } = new(); // Category-specific weights
    public Dictionary<string, decimal> VerticalWeights { get; set; } = new(); // Business vertical specific weights
    public decimal StrategicAlignmentMultiplier { get; set; } = 1.2m;
    public decimal ROIThreshold { get; set; } = 0.15m; // 15% ROI threshold for bonus weight
    public decimal ROIBonusMultiplier { get; set; } = 1.5m;
}

public class CapacityAdjustmentConfig
{
    public bool IsEnabled { get; set; } = true;
    public decimal MaxAdjustmentFactor { get; set; } = 1.5m;
    public decimal MinAdjustmentFactor { get; set; } = 0.5m;
    public decimal OptimalUtilizationPercentage { get; set; } = 80.0m;
    public string AdjustmentCurve { get; set; } = "Linear"; // Linear, Sigmoid, Step
    public Dictionary<string, decimal> DepartmentSpecificFactors { get; set; } = new();
}

public class AutoAdjustmentRulesConfig
{
    public bool IsEnabled { get; set; } = false;
    public int TriggerIntervalHours { get; set; } = 24;
    public decimal PriorityChangeThreshold { get; set; } = 0.1m;
    public List<AutoAdjustmentRule> Rules { get; set; } = new();
    public DateTime LastProcessed { get; set; }
    public bool NotifyOnAdjustment { get; set; } = true;
}

public class AutoAdjustmentRule
{
    public string RuleName { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty; // JSON condition script
    public string Action { get; set; } = string.Empty; // JSON action script
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1; // Rule execution priority
}

public class EscalationRulesConfig
{
    public bool IsEnabled { get; set; } = true;
    public List<EscalationRule> Rules { get; set; } = new();
    public int DefaultSLAHours { get; set; } = 72;
    public List<string> EscalationRecipients { get; set; } = new();
    public string NotificationTemplate { get; set; } = string.Empty;
}

public class EscalationRule
{
    public string RuleName { get; set; } = string.Empty;
    public int TriggerAfterHours { get; set; }
    public string Condition { get; set; } = string.Empty; // JSON condition
    public string EscalationAction { get; set; } = string.Empty; // Email, SMS, CreateTask
    public List<string> Recipients { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class PriorityEscalation
{
    public int WorkRequestId { get; set; }
    public string WorkRequestTitle { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime EscalationDate { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public decimal CurrentPriority { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}

public class PriorityConfigValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
}

public class PriorityPreviewResult
{
    public int TotalWorkRequests { get; set; }
    public int AffectedWorkRequests { get; set; }
    public List<PriorityChangePreview> Changes { get; set; } = new();
    public PriorityDistributionComparison DistributionComparison { get; set; } = new();
    public DateTime PreviewGeneratedDate { get; set; } = DateTime.UtcNow;
}

public class PriorityChangePreview
{
    public int WorkRequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal CurrentPriority { get; set; }
    public decimal NewPriority { get; set; }
    public decimal Change { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
}

public class PriorityDistributionComparison
{
    public Dictionary<string, int> CurrentDistribution { get; set; } = new();
    public Dictionary<string, int> NewDistribution { get; set; } = new();
    public Dictionary<string, int> Changes { get; set; } = new();
}

public class PriorityTrendAnalysis
{
    public List<PriorityTrendPoint> TrendData { get; set; } = new();
    public decimal AveragePriorityScore { get; set; }
    public decimal PriorityVolatility { get; set; }
    public List<string> Insights { get; set; } = new();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

public class PriorityTrendPoint
{
    public DateTime Date { get; set; }
    public decimal AveragePriority { get; set; }
    public int TotalWorkRequests { get; set; }
    public Dictionary<string, int> PriorityDistribution { get; set; } = new();
}

public class PriorityEffectivenessMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Trend { get; set; } = "Stable"; // Improving, Stable, Declining
    public string Description { get; set; } = string.Empty;
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;
}

public class PriorityRecommendation
{
    public string RecommendationType { get; set; } = string.Empty; // Algorithm, TimeDecay, BusinessValue, etc.
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public Dictionary<string, object> SuggestedChanges { get; set; } = new();
    public string Rationale { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
} 