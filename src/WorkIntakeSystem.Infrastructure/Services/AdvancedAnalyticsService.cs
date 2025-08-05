using Microsoft.ML;
using Microsoft.ML.Data;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Infrastructure.Data;
using System.Text.Json;

namespace WorkIntakeSystem.Infrastructure.Services;

public class AdvancedAnalyticsService : IAdvancedAnalyticsService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<AdvancedAnalyticsService> _logger;
    private readonly IAnalyticsService _analyticsService;
    private readonly WorkIntakeDbContext _context;
    private readonly IPriorityCalculationService _priorityCalculationService;

    public AdvancedAnalyticsService(
        ILogger<AdvancedAnalyticsService> logger,
        IAnalyticsService analyticsService,
        WorkIntakeDbContext context,
        IPriorityCalculationService priorityCalculationService)
    {
        _mlContext = new MLContext(seed: 0);
        _logger = logger;
        _analyticsService = analyticsService;
        _context = context;
        _priorityCalculationService = priorityCalculationService;
    }

    // Priority Prediction
    public async Task<PriorityPrediction> PredictPriorityAsync(int workRequestId)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.PriorityVotes)
                .Include(wr => wr.Department)
                .Include(wr => wr.BusinessVertical)
                .FirstOrDefaultAsync(wr => wr.Id == workRequestId);

            if (workRequest == null)
                throw new InvalidOperationException($"Work request {workRequestId} not found");

            // Calculate base priority using existing service
            var basePriority = await _priorityCalculationService.CalculatePriorityScoreAsync(workRequest);

            // Apply ML-based adjustments
            var mlAdjustment = await CalculateMLPriorityAdjustmentAsync(workRequest);
            var finalPriority = Math.Min(1.0m, Math.Max(0.0m, basePriority + mlAdjustment));

            // Determine confidence based on data quality
            var confidence = CalculatePredictionConfidence(workRequest);

            // Generate reasoning
            var reasoning = GeneratePriorityReasoning(workRequest, basePriority, mlAdjustment, confidence);

            var prediction = new PriorityPrediction
            {
                WorkRequestId = workRequestId,
                PredictedPriority = finalPriority,
                PredictedLevel = finalPriority switch
                {
                    >= 0.8m => PriorityLevel.Critical,
                    >= 0.6m => PriorityLevel.High,
                    >= 0.4m => PriorityLevel.Medium,
                    _ => PriorityLevel.Low
                },
                Confidence = confidence,
                Reasoning = reasoning,
                PredictedDate = DateTime.UtcNow
            };

            _logger.LogInformation("Priority prediction completed for work request {WorkRequestId}: {Priority} (Confidence: {Confidence})", 
                workRequestId, prediction.PredictedLevel, confidence);

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting priority for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<IEnumerable<PriorityTrend>> PredictPriorityTrendsAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var trends = new List<PriorityTrend>();
            var currentDate = DateTime.UtcNow;
            var daysToPredict = (targetDate - currentDate).Days;

            // Get historical priority data for the department
            var historicalData = await _context.WorkRequests
                .Where(wr => wr.DepartmentId == departmentId && wr.CreatedDate >= currentDate.AddDays(-90))
                .Select(wr => new { wr.CreatedDate, wr.Priority })
                .ToListAsync();

            if (!historicalData.Any())
                return trends;

            // Calculate trend using linear regression
            var trendSlope = CalculateTrendSlope(historicalData.Select(h => new { Date = h.CreatedDate, Value = (double)h.Priority }));

            // Predict future trends
            for (int i = 1; i <= Math.Min(daysToPredict, 30); i++)
            {
                var predictionDate = currentDate.AddDays(i);
                var predictedPriority = CalculateTrendPrediction(historicalData, trendSlope, predictionDate);
                var workRequestCount = await PredictWorkRequestCountAsync(departmentId, predictionDate);

                trends.Add(new PriorityTrend
                {
                    DepartmentId = departmentId,
                    Date = predictionDate,
                    AveragePriority = (decimal)predictedPriority,
                    WorkRequestCount = workRequestCount,
                    Trend = trendSlope > 0.01 ? "Increasing" : trendSlope < -0.01 ? "Decreasing" : "Stable"
                });
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting priority trends for department {DepartmentId}", departmentId);
            return new List<PriorityTrend>();
        }
    }

    // Resource Forecasting
    public async Task<ResourceForecast> ForecastResourceNeedsAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var department = await _context.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department == null)
                throw new InvalidOperationException($"Department {departmentId} not found");

            // Calculate current capacity
            var currentCapacity = department.Users.Count * 8 * 5; // 8 hours/day, 5 days/week
            var currentUtilization = department.CurrentUtilization;

            // Predict demand based on historical data
            var historicalDemand = await _context.WorkRequests
                .Where(wr => wr.DepartmentId == departmentId && wr.CreatedDate >= targetDate.AddDays(-90))
                .GroupBy(wr => wr.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var averageDailyDemand = historicalDemand.Any() ? historicalDemand.Average(h => h.Count) : 5;
            var predictedDemand = (int)(averageDailyDemand * 8); // Convert to hours

            // Calculate utilization rate
            var utilizationRate = predictedDemand > 0 ? (decimal)predictedDemand / currentCapacity : 0;

            // Generate recommendation
            var recommendation = utilizationRate switch
            {
                > 1.2m => "Critical: Immediate resource allocation needed",
                > 1.0m => "High: Consider adding resources or redistributing workload",
                > 0.8m => "Moderate: Monitor closely, may need additional resources",
                > 0.6m => "Good: Current capacity is adequate",
                _ => "Low: Consider optimizing resource allocation"
            };

            return new ResourceForecast
            {
                DepartmentId = departmentId,
                TargetDate = targetDate,
                PredictedCapacity = currentCapacity,
                PredictedDemand = predictedDemand,
                UtilizationRate = utilizationRate,
                Recommendation = recommendation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting resource needs for department {DepartmentId}", departmentId);
            throw;
        }
    }

    public async Task<CapacityPrediction> PredictCapacityUtilizationAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var department = await _context.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department == null)
                throw new InvalidOperationException($"Department {departmentId} not found");

            // Get current utilization
            var currentUtilization = department.CurrentUtilization;

            // Predict future utilization using historical trends
            var historicalUtilization = await _context.WorkRequests
                .Where(wr => wr.DepartmentId == departmentId && wr.CreatedDate >= targetDate.AddDays(-60))
                .GroupBy(wr => wr.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var trendSlope = CalculateTrendSlope(historicalUtilization.Select(h => new { Date = h.Date, Value = (double)h.Count }));
            var predictedUtilization = Math.Min(100.0m, Math.Max(0.0m, currentUtilization + (decimal)(trendSlope * 30)));

            var recommendation = predictedUtilization switch
            {
                > 90m => "Critical: Immediate capacity expansion required",
                > 80m => "High: Plan for capacity increase",
                > 70m => "Moderate: Monitor capacity trends",
                > 50m => "Good: Current capacity is sufficient",
                _ => "Low: Consider capacity optimization"
            };

            return new CapacityPrediction
            {
                DepartmentId = departmentId,
                TargetDate = targetDate,
                PredictedCapacity = predictedUtilization,
                CurrentUtilization = currentUtilization,
                Recommendation = recommendation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting capacity utilization for department {DepartmentId}", departmentId);
            throw;
        }
    }

    // Completion Prediction
    public async Task<CompletionPrediction> PredictCompletionTimeAsync(int workRequestId)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Department)
                .Include(wr => wr.BusinessVertical)
                .FirstOrDefaultAsync(wr => wr.Id == workRequestId);

            if (workRequest == null)
                throw new InvalidOperationException($"Work request {workRequestId} not found");

            // Get historical completion times for similar work requests
            var similarWorkRequests = await _context.WorkRequests
                .Where(wr => wr.DepartmentId == workRequest.DepartmentId && 
                            wr.BusinessVerticalId == workRequest.BusinessVerticalId &&
                            wr.Status == WorkStatus.Completed &&
                            wr.CreatedDate >= DateTime.UtcNow.AddDays(-365))
                .ToListAsync();

            var averageCompletionDays = similarWorkRequests.Any() 
                ? similarWorkRequests.Average(wr => (wr.ActualDate - wr.CreatedDate)?.TotalDays ?? 14)
                : 14.0;

            // Apply complexity adjustments
            var complexityMultiplier = CalculateComplexityMultiplier(workRequest);
            var adjustedCompletionDays = averageCompletionDays * complexityMultiplier;

            // Calculate confidence based on data quality
            var confidence = CalculateCompletionConfidence(similarWorkRequests.Count, workRequest);

            // Generate factors affecting completion
            var factors = GenerateCompletionFactors(workRequest, complexityMultiplier);

            var predictedCompletionDate = workRequest.CreatedDate.AddDays(adjustedCompletionDays);

            return new CompletionPrediction
            {
                WorkRequestId = workRequestId,
                PredictedCompletionDate = predictedCompletionDate,
                Confidence = (decimal)confidence,
                Factors = factors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting completion time for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<IEnumerable<CompletionTrend>> PredictCompletionTrendsAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var trends = new List<CompletionTrend>();
            var currentDate = DateTime.UtcNow;

            // Get historical completion data
            var historicalData = await _context.WorkRequests
                .Where(wr => wr.DepartmentId == departmentId && 
                            wr.Status == WorkStatus.Completed &&
                            wr.CreatedDate >= currentDate.AddDays(-180))
                .Select(wr => new { wr.CreatedDate, wr.ActualDate })
                .ToListAsync();

            if (!historicalData.Any())
                return trends;

            // Calculate trend
            var trendSlope = CalculateCompletionTrendSlope(historicalData);

            // Predict future trends
            for (int i = 1; i <= 30; i++)
            {
                var predictionDate = currentDate.AddDays(i);
                var predictedCompletionTime = CalculateCompletionTrendPrediction(historicalData, trendSlope, predictionDate);
                var completedWorkItems = await PredictCompletedWorkItemsAsync(departmentId, predictionDate);

                trends.Add(new CompletionTrend
                {
                    DepartmentId = departmentId,
                    Date = predictionDate,
                    AverageCompletionTime = (decimal)predictedCompletionTime,
                    CompletedWorkItems = completedWorkItems,
                    Trend = trendSlope < -0.5 ? "Improving" : trendSlope > 0.5 ? "Declining" : "Stable"
                });
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting completion trends for department {DepartmentId}", departmentId);
            return new List<CompletionTrend>();
        }
    }

    // Business Value Analysis
    public async Task<BusinessValueROI> CalculateROIAsync(int workRequestId)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.BusinessVertical)
                .FirstOrDefaultAsync(wr => wr.Id == workRequestId);

            if (workRequest == null)
                throw new InvalidOperationException($"Work request {workRequestId} not found");

            // Calculate estimated cost based on complexity and department
            var estimatedCost = await CalculateEstimatedCostAsync(workRequest);

            // Calculate estimated value based on business value and strategic importance
            var estimatedValue = await CalculateEstimatedValueAsync(workRequest);

            // Calculate ROI
            var roi = estimatedCost > 0 ? (estimatedValue - estimatedCost) / estimatedCost : 0;

            // Calculate payback period
            var paybackPeriod = estimatedCost > 0 ? estimatedCost / (estimatedValue / 12) : 0; // months

            // Generate analysis
            var analysis = GenerateROIAnalysis(workRequest, estimatedCost, estimatedValue, roi, paybackPeriod);

            return new BusinessValueROI
            {
                WorkRequestId = workRequestId,
                EstimatedCost = estimatedCost,
                EstimatedValue = estimatedValue,
                ROI = roi,
                PaybackPeriod = paybackPeriod,
                Analysis = analysis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ROI for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<IEnumerable<BusinessValueTrend>> AnalyzeBusinessValueTrendsAsync(int businessVerticalId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var trends = new List<BusinessValueTrend>();

            // Get work requests for the business vertical
            var workRequests = await _context.WorkRequests
                .Where(wr => wr.BusinessVerticalId == businessVerticalId &&
                            wr.CreatedDate >= fromDate &&
                            wr.CreatedDate <= toDate)
                .ToListAsync();

            if (!workRequests.Any())
                return trends;

            // Group by month and calculate trends
            var monthlyData = workRequests
                .GroupBy(wr => new { wr.CreatedDate.Year, wr.CreatedDate.Month })
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    AverageBusinessValue = g.Average(wr => wr.BusinessValue),
                    TotalROI = g.Sum(wr => wr.BusinessValue),
                    WorkRequestCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            foreach (var data in monthlyData)
            {
                trends.Add(new BusinessValueTrend
                {
                    BusinessVerticalId = businessVerticalId,
                    Date = data.Date,
                    AverageBusinessValue = (decimal)data.AverageBusinessValue,
                    TotalROI = (decimal)data.TotalROI,
                    WorkRequestCount = data.WorkRequestCount,
                    Trend = CalculateBusinessValueTrend(monthlyData, data.Date)
                });
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing business value trends for business vertical {BusinessVerticalId}", businessVerticalId);
            return new List<BusinessValueTrend>();
        }
    }

    // Risk Assessment
    public async Task<RiskAssessment> AssessProjectRiskAsync(int workRequestId)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Department)
                .Include(wr => wr.BusinessVertical)
                .Include(wr => wr.PriorityVotes)
                .FirstOrDefaultAsync(wr => wr.Id == workRequestId);

            if (workRequest == null)
                throw new InvalidOperationException($"Work request {workRequestId} not found");

            // Calculate risk factors
            var complexityRisk = await CalculateComplexityRiskAsync(workRequest);
            var resourceRisk = await CalculateResourceRiskAsync(workRequest);
            var timelineRisk = await CalculateTimelineRiskAsync(workRequest);
            var dependencyRisk = await CalculateDependencyRiskAsync(workRequest);
            var businessImpactRisk = await CalculateBusinessImpactRiskAsync(workRequest);

            // Calculate overall risk score using weighted average
            var riskFactors = new Dictionary<string, double>
            {
                { "Complexity", complexityRisk },
                { "Resource Availability", resourceRisk },
                { "Timeline", timelineRisk },
                { "Dependencies", dependencyRisk },
                { "Business Impact", businessImpactRisk }
            };

            var overallRiskScore = riskFactors.Values.Average();
            var riskLevel = DetermineRiskLevel(overallRiskScore);

            // Generate mitigation strategies
            var mitigationStrategies = GenerateMitigationStrategies(riskFactors, riskLevel);

            var result = new RiskAssessment
            {
                WorkRequestId = workRequestId,
                RiskScore = (decimal)overallRiskScore,
                RiskLevel = riskLevel,
                RiskFactors = riskFactors.Where(kvp => kvp.Value > 0.3).Select(kvp => kvp.Key).ToList(),
                MitigationStrategy = string.Join(", ", mitigationStrategies)
            };

            _logger.LogInformation("Risk assessment completed for work request {WorkRequestId}: {RiskLevel} risk (Score: {Score})", 
                workRequestId, riskLevel, overallRiskScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing project risk for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<IEnumerable<RiskIndicator>> GetRiskIndicatorsAsync(int departmentId)
    {
        try
        {
            var indicators = new List<RiskIndicator>();

            // Get department work requests
            var workRequests = await _context.WorkRequests
                .Where(wr => wr.DepartmentId == departmentId && wr.Status != WorkStatus.Completed)
                .ToListAsync();

            if (!workRequests.Any())
                return indicators;

            // Calculate various risk indicators
            var overdueRisk = CalculateOverdueRisk(workRequests);
            var capacityRisk = CalculateCapacityRisk(departmentId);
            var complexityRisk = CalculateComplexityRisk(workRequests);
            var dependencyRisk = CalculateDependencyRisk(workRequests);

            if (overdueRisk > 0.3)
            {
                indicators.Add(new RiskIndicator
                {
                    DepartmentId = departmentId,
                    RiskType = "Overdue Work",
                    RiskScore = overdueRisk,
                    RiskLevel = DetermineRiskLevel(overdueRisk),
                    Description = "High number of overdue work requests",
                    MitigationAction = "Review and reprioritize overdue items"
                });
            }

            if (capacityRisk > 0.3)
            {
                indicators.Add(new RiskIndicator
                {
                    DepartmentId = departmentId,
                    RiskType = "Capacity Overload",
                    RiskScore = capacityRisk,
                    RiskLevel = DetermineRiskLevel(capacityRisk),
                    Description = "Department capacity utilization is high",
                    MitigationAction = "Consider resource allocation or workload redistribution"
                });
            }

            if (complexityRisk > 0.3)
            {
                indicators.Add(new RiskIndicator
                {
                    DepartmentId = departmentId,
                    RiskType = "High Complexity",
                    RiskScore = complexityRisk,
                    RiskLevel = DetermineRiskLevel(complexityRisk),
                    Description = "High complexity work requests may cause delays",
                    MitigationAction = "Break down complex work into smaller tasks"
                });
            }

            if (dependencyRisk > 0.3)
            {
                indicators.Add(new RiskIndicator
                {
                    DepartmentId = departmentId,
                    RiskType = "Dependency Risk",
                    RiskScore = dependencyRisk,
                    RiskLevel = DetermineRiskLevel(dependencyRisk),
                    Description = "Work requests have external dependencies",
                    MitigationAction = "Coordinate with dependent teams and set clear expectations"
                });
            }

            return indicators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk indicators for department {DepartmentId}", departmentId);
            return new List<RiskIndicator>();
        }
    }

    // Predictive Insights
    public async Task<IEnumerable<PredictiveInsight>> GetPredictiveInsightsAsync(int businessVerticalId)
    {
        try
        {
            var insights = new List<PredictiveInsight>();

            // Get work requests for the business vertical
            var workRequests = await _context.WorkRequests
                .Where(wr => wr.BusinessVerticalId == businessVerticalId &&
                            wr.CreatedDate >= DateTime.UtcNow.AddDays(-90))
                .ToListAsync();

            if (!workRequests.Any())
                return insights;

            // Analyze patterns and generate insights
            var priorityInsight = GeneratePriorityInsight(workRequests);
            var capacityInsight = GenerateCapacityInsight(workRequests);
            var trendInsight = GenerateTrendInsight(workRequests);

            if (priorityInsight != null)
                insights.Add(priorityInsight);

            if (capacityInsight != null)
                insights.Add(capacityInsight);

            if (trendInsight != null)
                insights.Add(trendInsight);

            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting predictive insights for business vertical {BusinessVerticalId}", businessVerticalId);
            return new List<PredictiveInsight>();
        }
    }

    public async Task<WorkloadPrediction> PredictWorkloadAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department == null)
                throw new InvalidOperationException($"Department {departmentId} not found");

            // Get historical workload data
            var historicalData = await _context.WorkRequests
                .Where(wr => wr.DepartmentId == departmentId &&
                            wr.CreatedDate >= targetDate.AddDays(-60))
                .GroupBy(wr => wr.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            if (!historicalData.Any())
                return new WorkloadPrediction
                {
                    DepartmentId = departmentId,
                    TargetDate = targetDate,
                    PredictedUtilization = department.CurrentUtilization,
                    PredictedWorkItems = 0,
                    Trend = "Stable"
                };

            // Calculate trend
            var trendSlope = CalculateTrendSlope(historicalData.Select(h => new { Date = h.Date, Value = (double)h.Count }));
            var averageWorkItems = historicalData.Average(h => h.Count);
            var predictedWorkItems = (int)(averageWorkItems + (trendSlope * 30));

            // Calculate predicted utilization
            var predictedUtilization = Math.Min(100.0m, Math.Max(0.0m, 
                department.CurrentUtilization + (decimal)(trendSlope * 10)));

            var trend = trendSlope switch
            {
                > 0.1 => "Increasing",
                < -0.1 => "Decreasing",
                _ => "Stable"
            };

            return new WorkloadPrediction
            {
                DepartmentId = departmentId,
                TargetDate = targetDate,
                PredictedUtilization = predictedUtilization,
                PredictedWorkItems = predictedWorkItems,
                Trend = trend
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting workload for department {DepartmentId}", departmentId);
            throw;
        }
    }

    // Helper methods for real business logic
    private async Task<decimal> CalculateMLPriorityAdjustmentAsync(WorkRequest workRequest)
    {
        // In a real implementation, this would use a trained ML model
        // For now, we'll use a simplified algorithm based on business rules
        
        var adjustment = 0.0m;

        // Business value adjustment
        adjustment += workRequest.BusinessValue * 0.1m;

        // Strategic alignment adjustment
        if (workRequest.BusinessVertical?.StrategicImportance > 0.7m)
            adjustment += 0.05m;

        // Urgency adjustment based on target date
        if (workRequest.TargetDate.HasValue)
        {
            var daysUntilTarget = (workRequest.TargetDate.Value - DateTime.UtcNow).Days;
            if (daysUntilTarget <= 7)
                adjustment += 0.1m;
            else if (daysUntilTarget <= 30)
                adjustment += 0.05m;
        }

        // Department capacity adjustment
        if (workRequest.Department?.CurrentUtilization > 80m)
            adjustment -= 0.05m; // Lower priority for overloaded departments

        return Math.Max(-0.2m, Math.Min(0.2m, adjustment));
    }

    private decimal CalculatePredictionConfidence(WorkRequest workRequest)
    {
        var confidence = 0.7m; // Base confidence

        // Increase confidence based on data quality
        if (workRequest.PriorityVotes?.Any() == true)
            confidence += 0.1m;

        if (workRequest.BusinessValue > 0)
            confidence += 0.1m;

        if (workRequest.TargetDate.HasValue)
            confidence += 0.05m;

        return Math.Min(1.0m, confidence);
    }

    private string GeneratePriorityReasoning(WorkRequest workRequest, decimal basePriority, decimal mlAdjustment, decimal confidence)
    {
        var reasons = new List<string>();

        reasons.Add($"Base priority: {basePriority:F2}");

        if (mlAdjustment > 0)
            reasons.Add($"ML adjustment: +{mlAdjustment:F2} (business value, strategic alignment)");
        else if (mlAdjustment < 0)
            reasons.Add($"ML adjustment: {mlAdjustment:F2} (department capacity constraints)");

        if (workRequest.BusinessValue > 0.7m)
            reasons.Add("High business value");

        if (workRequest.TargetDate.HasValue && (workRequest.TargetDate.Value - DateTime.UtcNow).Days <= 30)
            reasons.Add("Urgent timeline");

        reasons.Add($"Confidence: {confidence:P0}");

        return string.Join("; ", reasons);
    }

    private double CalculateTrendSlope(IEnumerable<dynamic> data)
    {
        var dataList = data.ToList();
        if (dataList.Count < 2) return 0;

        var xValues = Enumerable.Range(0, dataList.Count).Select(i => (double)i).ToArray();
        var yValues = dataList.Select(d => (double)d.Value).ToArray();

        var n = xValues.Length;
        var sumX = xValues.Sum();
        var sumY = yValues.Sum();
        var sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
        var sumX2 = xValues.Select(x => x * x).Sum();

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }

    private double CalculateTrendPrediction(List<dynamic> historicalData, double trendSlope, DateTime predictionDate)
    {
        if (!historicalData.Any()) return 0.5;

        var averageValue = historicalData.Average(d => (double)d.Value);
        var daysFromLastData = (predictionDate - historicalData.Max(d => d.Date)).Days;
        
        return Math.Max(0, Math.Min(1, averageValue + (trendSlope * daysFromLastData)));
    }

    private async Task<int> PredictWorkRequestCountAsync(int departmentId, DateTime predictionDate)
    {
        var historicalCounts = await _context.WorkRequests
            .Where(wr => wr.DepartmentId == departmentId && 
                        wr.CreatedDate >= predictionDate.AddDays(-30) &&
                        wr.CreatedDate <= predictionDate.AddDays(-1))
            .GroupBy(wr => wr.CreatedDate.Date)
            .Select(g => g.Count())
            .ToListAsync();

        return historicalCounts.Any() ? (int)historicalCounts.Average() : 5;
    }

    private double CalculateComplexityMultiplier(WorkRequest workRequest)
    {
        var multiplier = 1.0;

        // Adjust based on business value (higher value = more complex)
        multiplier += (double)workRequest.BusinessValue * 0.2;

        // Adjust based on department (some departments handle more complex work)
        if (workRequest.Department?.Name?.Contains("Engineering") == true)
            multiplier += 0.3;

        // Adjust based on business vertical
        if (workRequest.BusinessVertical?.StrategicImportance > 0.8m)
            multiplier += 0.2;

        return Math.Max(0.5, Math.Min(3.0, multiplier));
    }

    private double CalculateCompletionConfidence(int similarWorkRequestsCount, WorkRequest workRequest)
    {
        var confidence = 0.5; // Base confidence

        // Increase confidence with more historical data
        if (similarWorkRequestsCount >= 10)
            confidence += 0.3;
        else if (similarWorkRequestsCount >= 5)
            confidence += 0.2;
        else if (similarWorkRequestsCount >= 1)
            confidence += 0.1;

        // Adjust based on data quality
        if (workRequest.BusinessValue > 0)
            confidence += 0.1;

        if (workRequest.TargetDate.HasValue)
            confidence += 0.05;

        return Math.Min(1.0, confidence);
    }

    private string GenerateCompletionFactors(WorkRequest workRequest, double complexityMultiplier)
    {
        var factors = new List<string>();

        if (complexityMultiplier > 1.5)
            factors.Add("High complexity");

        if (workRequest.BusinessValue > 0.7m)
            factors.Add("High business value");

        if (workRequest.Department?.CurrentUtilization > 80m)
            factors.Add("Department capacity constraints");

        if (workRequest.TargetDate.HasValue && (workRequest.TargetDate.Value - DateTime.UtcNow).Days <= 30)
            factors.Add("Urgent timeline");

        return string.Join(", ", factors);
    }

    private double CalculateCompletionTrendSlope(List<dynamic> historicalData)
    {
        if (historicalData.Count < 2) return 0;

        var completionTimes = historicalData
            .Select(h => (h.ActualDate - h.CreatedDate)?.TotalDays ?? 14)
            .ToList();

        var xValues = Enumerable.Range(0, completionTimes.Count).Select(i => (double)i).ToArray();
        var yValues = completionTimes.ToArray();

        var n = xValues.Length;
        var sumX = xValues.Sum();
        var sumY = yValues.Sum();
        var sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
        var sumX2 = xValues.Select(x => x * x).Sum();

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }

    private double CalculateCompletionTrendPrediction(List<dynamic> historicalData, double trendSlope, DateTime predictionDate)
    {
        if (!historicalData.Any()) return 14.0;

        var averageCompletionTime = historicalData
            .Average(h => (h.ActualDate - h.CreatedDate)?.TotalDays ?? 14);

        var daysFromLastData = (predictionDate - historicalData.Max(h => h.CreatedDate)).Days;
        
        return Math.Max(1, averageCompletionTime + (trendSlope * daysFromLastData));
    }

    private async Task<int> PredictCompletedWorkItemsAsync(int departmentId, DateTime predictionDate)
    {
        var historicalCompletions = await _context.WorkRequests
            .Where(wr => wr.DepartmentId == departmentId && 
                        wr.Status == WorkStatus.Completed &&
                        wr.ActualDate >= predictionDate.AddDays(-30) &&
                        wr.ActualDate <= predictionDate.AddDays(-1))
            .GroupBy(wr => wr.ActualDate.Value.Date)
            .Select(g => g.Count())
            .ToListAsync();

        return historicalCompletions.Any() ? (int)historicalCompletions.Average() : 3;
    }

    private async Task<decimal> CalculateEstimatedCostAsync(WorkRequest workRequest)
    {
        var baseCost = 1000m; // Base cost per work request

        // Adjust based on complexity
        var complexityMultiplier = CalculateComplexityMultiplier(workRequest);
        baseCost *= (decimal)complexityMultiplier;

        // Adjust based on department (some departments are more expensive)
        if (workRequest.Department?.Name?.Contains("Engineering") == true)
            baseCost *= 1.5m;

        // Adjust based on business vertical
        if (workRequest.BusinessVertical?.StrategicImportance > 0.8m)
            baseCost *= 1.3m;

        return baseCost;
    }

    private async Task<decimal> CalculateEstimatedValueAsync(WorkRequest workRequest)
    {
        var baseValue = workRequest.BusinessValue * 10000m; // Convert to monetary value

        // Adjust based on strategic importance
        if (workRequest.BusinessVertical?.StrategicImportance > 0.8m)
            baseValue *= 1.5m;

        // Adjust based on urgency
        if (workRequest.TargetDate.HasValue && (workRequest.TargetDate.Value - DateTime.UtcNow).Days <= 30)
            baseValue *= 1.2m;

        return baseValue;
    }

    private string GenerateROIAnalysis(WorkRequest workRequest, decimal estimatedCost, decimal estimatedValue, decimal roi, decimal paybackPeriod)
    {
        var analysis = new List<string>();

        analysis.Add($"Estimated cost: ${estimatedCost:N0}");
        analysis.Add($"Estimated value: ${estimatedValue:N0}");
        analysis.Add($"ROI: {roi:P0}");

        if (roi > 0.5m)
            analysis.Add("Excellent ROI - highly recommended");
        else if (roi > 0.2m)
            analysis.Add("Good ROI - recommended");
        else if (roi > 0m)
            analysis.Add("Positive ROI - consider if strategic");
        else
            analysis.Add("Negative ROI - reconsider unless strategic");

        if (paybackPeriod <= 6)
            analysis.Add("Quick payback period");
        else if (paybackPeriod <= 12)
            analysis.Add("Moderate payback period");
        else
            analysis.Add("Long payback period - consider alternatives");

        return string.Join("; ", analysis);
    }

    private string CalculateBusinessValueTrend(List<dynamic> monthlyData, DateTime date)
    {
        var currentData = monthlyData.FirstOrDefault(d => d.Date == date);
        var previousData = monthlyData.FirstOrDefault(d => d.Date < date);

        if (currentData == null || previousData == null)
            return "Stable";

        var change = currentData.AverageBusinessValue - previousData.AverageBusinessValue;
        var percentageChange = change / previousData.AverageBusinessValue;

        return percentageChange switch
        {
            > 0.1 => "Increasing",
            < -0.1 => "Decreasing",
            _ => "Stable"
        };
    }

    private async Task<double> CalculateComplexityRiskAsync(WorkRequest workRequest)
    {
        var risk = 0.0;

        // High business value often indicates complexity
        risk += (double)workRequest.BusinessValue * 0.3;

        // Strategic importance can indicate complexity
        if (workRequest.BusinessVertical?.StrategicImportance > 0.8m)
            risk += 0.2;

        // Engineering departments often handle complex work
        if (workRequest.Department?.Name?.Contains("Engineering") == true)
            risk += 0.2;

        return Math.Min(1.0, risk);
    }

    private async Task<double> CalculateResourceRiskAsync(WorkRequest workRequest)
    {
        var risk = 0.0;

        // High department utilization indicates resource constraints
        if (workRequest.Department?.CurrentUtilization > 80m)
            risk += 0.4;
        else if (workRequest.Department?.CurrentUtilization > 60m)
            risk += 0.2;

        // Complex work requires more resources
        var complexityMultiplier = CalculateComplexityMultiplier(workRequest);
        risk += (complexityMultiplier - 1.0) * 0.3;

        return Math.Min(1.0, risk);
    }

    private async Task<double> CalculateTimelineRiskAsync(WorkRequest workRequest)
    {
        var risk = 0.0;

        if (workRequest.TargetDate.HasValue)
        {
            var daysUntilTarget = (workRequest.TargetDate.Value - DateTime.UtcNow).Days;
            
            if (daysUntilTarget <= 7)
                risk += 0.5;
            else if (daysUntilTarget <= 30)
                risk += 0.3;
            else if (daysUntilTarget <= 90)
                risk += 0.1;
        }

        // Complex work takes longer
        var complexityMultiplier = CalculateComplexityMultiplier(workRequest);
        risk += (complexityMultiplier - 1.0) * 0.2;

        return Math.Min(1.0, risk);
    }

    private async Task<double> CalculateDependencyRiskAsync(WorkRequest workRequest)
    {
        // Simplified dependency risk calculation
        // In a real implementation, this would analyze actual dependencies
        var risk = 0.0;

        // High business value work often has dependencies
        if (workRequest.BusinessValue > 0.7m)
            risk += 0.3;

        // Strategic work often has external dependencies
        if (workRequest.BusinessVertical?.StrategicImportance > 0.8m)
            risk += 0.2;

        return Math.Min(1.0, risk);
    }

    private async Task<double> CalculateBusinessImpactRiskAsync(WorkRequest workRequest)
    {
        var risk = 0.0;

        // High business value means high impact if it fails
        risk += (double)workRequest.BusinessValue * 0.4;

        // Strategic importance increases impact
        if (workRequest.BusinessVertical?.StrategicImportance > 0.8m)
            risk += 0.3;

        return Math.Min(1.0, risk);
    }

    private string DetermineRiskLevel(double riskScore)
    {
        return riskScore switch
        {
            >= 0.7 => "High",
            >= 0.4 => "Medium",
            >= 0.2 => "Low",
            _ => "Minimal"
        };
    }

    private List<string> GenerateMitigationStrategies(Dictionary<string, double> riskFactors, string riskLevel)
    {
        var strategies = new List<string>();

        if (riskLevel == "High")
        {
            strategies.Add("Increase monitoring and oversight");
            strategies.Add("Allocate additional resources");
            strategies.Add("Develop contingency plans");
        }
        else if (riskLevel == "Medium")
        {
            strategies.Add("Regular status updates");
            strategies.Add("Monitor key risk factors");
        }
        else
        {
            strategies.Add("Standard project management practices");
        }

        // Add specific strategies based on risk factors
        if (riskFactors.ContainsKey("Resource Availability") && riskFactors["Resource Availability"] > 0.5)
            strategies.Add("Consider resource reallocation");

        if (riskFactors.ContainsKey("Timeline") && riskFactors["Timeline"] > 0.5)
            strategies.Add("Review timeline and milestones");

        return strategies;
    }

    private double CalculateOverdueRisk(List<WorkRequest> workRequests)
    {
        var overdueCount = workRequests.Count(wr => 
            wr.TargetDate.HasValue && wr.TargetDate.Value < DateTime.UtcNow);
        
        return workRequests.Any() ? (double)overdueCount / workRequests.Count : 0;
    }

    private double CalculateCapacityRisk(int departmentId)
    {
        var department = _context.Departments.FirstOrDefault(d => d.Id == departmentId);
        if (department == null) return 0;

        return department.CurrentUtilization > 80m ? 0.8 : department.CurrentUtilization > 60m ? 0.5 : 0.2;
    }

    private double CalculateComplexityRisk(List<WorkRequest> workRequests)
    {
        if (!workRequests.Any()) return 0;

        var highComplexityCount = workRequests.Count(wr => wr.BusinessValue > 0.7m);
        return (double)highComplexityCount / workRequests.Count;
    }

    private double CalculateDependencyRisk(List<WorkRequest> workRequests)
    {
        if (!workRequests.Any()) return 0;

        var highDependencyCount = workRequests.Count(wr => 
            wr.BusinessValue > 0.7m || wr.BusinessVertical?.StrategicImportance > 0.8m);
        
        return (double)highDependencyCount / workRequests.Count;
    }

    private PredictiveInsight? GeneratePriorityInsight(List<WorkRequest> workRequests)
    {
        var averagePriority = workRequests.Average(wr => wr.Priority);
        var highPriorityCount = workRequests.Count(wr => wr.Priority > 0.7m);

        if (highPriorityCount > workRequests.Count * 0.3)
        {
            return new PredictiveInsight
            {
                InsightType = "Priority Distribution",
                Description = $"High number of high-priority work requests ({highPriorityCount}/{workRequests.Count})",
                Confidence = 0.8m,
                PredictedDate = DateTime.UtcNow.AddDays(7),
                Recommendation = "Consider reprioritizing work to balance workload"
            };
        }

        return null;
    }

    private PredictiveInsight? GenerateCapacityInsight(List<WorkRequest> workRequests)
    {
        var departments = workRequests.Select(wr => wr.DepartmentId).Distinct();
        var overloadedDepartments = new List<int>();

        foreach (var deptId in departments)
        {
            var dept = _context.Departments.FirstOrDefault(d => d.Id == deptId);
            if (dept?.CurrentUtilization > 80m)
                overloadedDepartments.Add(deptId);
        }

        if (overloadedDepartments.Any())
        {
            return new PredictiveInsight
            {
                InsightType = "Capacity Management",
                Description = $"{overloadedDepartments.Count} departments are at high capacity utilization",
                Confidence = 0.9m,
                PredictedDate = DateTime.UtcNow.AddDays(14),
                Recommendation = "Consider resource reallocation or workload redistribution"
            };
        }

        return null;
    }

    private PredictiveInsight? GenerateTrendInsight(List<WorkRequest> workRequests)
    {
        var recentWorkRequests = workRequests.Where(wr => wr.CreatedDate >= DateTime.UtcNow.AddDays(-30));
        var olderWorkRequests = workRequests.Where(wr => wr.CreatedDate < DateTime.UtcNow.AddDays(-30) && wr.CreatedDate >= DateTime.UtcNow.AddDays(-60));

        if (recentWorkRequests.Any() && olderWorkRequests.Any())
        {
            var recentAverage = recentWorkRequests.Average(wr => wr.BusinessValue);
            var olderAverage = olderWorkRequests.Average(wr => wr.BusinessValue);
            var change = recentAverage - olderAverage;

            if (Math.Abs(change) > 0.1)
            {
                return new PredictiveInsight
                {
                    InsightType = "Business Value Trend",
                    Description = $"Business value is {(change > 0 ? "increasing" : "decreasing")} by {Math.Abs(change):P0}",
                    Confidence = 0.7m,
                    PredictedDate = DateTime.UtcNow.AddDays(30),
                    Recommendation = change > 0 ? "Continue focusing on high-value work" : "Review business value assessment criteria"
                };
            }
        }

        return null;
    }
} 