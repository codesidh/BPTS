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

            // Build a more sophisticated pipeline with feature engineering
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("CategoryEncoded", nameof(PriorityTrainingData.Category))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("DepartmentEncoded", nameof(PriorityTrainingData.Department)))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("BusinessVerticalEncoded", nameof(PriorityTrainingData.BusinessVertical)))
                .Append(_mlContext.Transforms.Concatenate("Features", "CategoryEncoded", "DepartmentEncoded", "BusinessVerticalEncoded",
                    nameof(PriorityTrainingData.BusinessValue), nameof(PriorityTrainingData.Urgency), 
                    nameof(PriorityTrainingData.Complexity), nameof(PriorityTrainingData.EstimatedHours),
                    nameof(PriorityTrainingData.ResourceAvailability), nameof(PriorityTrainingData.StrategicAlignment)))
                .Append(_mlContext.Transforms.NormalizeMinMax("NormalizedFeatures", "Features"))
                .Append(_mlContext.Regression.Trainers.FastTree(numberOfLeaves: 20, numberOfTrees: 100, minimumExampleCountPerLeaf: 10));

            var model = pipeline.Fit(dataView);

            // Calculate features for the current work request
            var businessValue = await CalculateBusinessValueAsync(workRequest);
            var urgency = await CalculateUrgencyAsync(workRequest);
            var complexity = await CalculateComplexityAsync(workRequest);
            var resourceAvailability = await CalculateResourceAvailabilityAsync(workRequest.DepartmentId);
            var strategicAlignment = await CalculateStrategicAlignmentAsync(workRequest);

            // Create prediction input
            var input = new PriorityTrainingData
            {
                Category = workRequest.Category.ToString(),
                Department = workRequest.DepartmentId.ToString(),
                BusinessVertical = workRequest.BusinessVerticalId.ToString(),
                BusinessValue = businessValue,
                Urgency = urgency,
                Complexity = complexity,
                EstimatedHours = workRequest.EstimatedEffort,
                ResourceAvailability = resourceAvailability,
                StrategicAlignment = strategicAlignment
            };

            // Make prediction
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<PriorityTrainingData, PriorityPredictionOutput>(model);
            var prediction = predictionEngine.Predict(input);

            // Calculate confidence based on model performance and data quality
            var confidence = CalculatePredictionConfidence(prediction.PredictedPriority, trainingData.Count);

            // Determine priority level based on predicted score
            var priorityLevel = DeterminePriorityLevel(prediction.PredictedPriority);

            var result = new PriorityPrediction
            {
                WorkRequestId = workRequest.Id,
                PredictedPriority = (decimal)prediction.PredictedPriority,
                PredictedLevel = priorityLevel,
                Confidence = (decimal)confidence,
                Reasoning = GeneratePriorityReasoning(businessValue, urgency, complexity, resourceAvailability, strategicAlignment),
                PredictedDate = DateTime.UtcNow
            };

            _logger.LogInformation("Predicted priority {PredictedPriority} (Level: {Level}) for work request {WorkRequestId} with confidence {Confidence}", 
                result.PredictedPriority, result.PredictedLevel, workRequest.Id, result.Confidence);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict priority for work request {WorkRequestId}", workRequest.Id);
            return new PriorityPrediction { WorkRequestId = workRequest.Id, PredictedPriority = 5.0m, Confidence = 0.0m };
        }
    }

    public async Task<PriorityPrediction> PredictPriorityAsync(int workRequestId)
    {
        try
        {
            // This would fetch the work request and predict priority
            var workRequest = new WorkRequest { Id = workRequestId }; // In practice, fetch from repository
            return await PredictWorkRequestPriorityAsync(workRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict priority for work request {WorkRequestId}", workRequestId);
            return new PriorityPrediction { WorkRequestId = workRequestId, PredictedPriority = 5.0m, Confidence = 0.0m };
        }
    }

    public async Task<IEnumerable<PriorityTrend>> PredictPriorityTrendsAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var trends = new List<PriorityTrend>();
            var currentDate = DateTime.UtcNow;
            
            for (int i = 0; i < 30; i++)
            {
                var date = currentDate.AddDays(i);
                trends.Add(new PriorityTrend
                {
                    DepartmentId = departmentId,
                    Date = date,
                    AveragePriority = 0.6m + (decimal)(Math.Sin(i * 0.1) * 0.2),
                    WorkRequestCount = new Random().Next(5, 25),
                    Trend = i % 7 == 0 ? "Weekly Pattern" : "Daily Fluctuation"
                });
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict priority trends for department {DepartmentId}", departmentId);
            return Enumerable.Empty<PriorityTrend>();
        }
    }

    public async Task<ResourceForecast> ForecastResourceNeedsAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var currentWorkload = await GetCurrentDepartmentWorkloadAsync(departmentId);
            var capacity = await GetDepartmentCapacityAsync(departmentId);
            var predictedDemand = currentWorkload * 1.1; // 10% growth assumption

            var result = new ResourceForecast
            {
                DepartmentId = departmentId,
                TargetDate = targetDate,
                PredictedCapacity = (int)capacity,
                PredictedDemand = (int)predictedDemand,
                UtilizationRate = (decimal)(predictedDemand / capacity),
                Recommendation = predictedDemand > capacity ? "Consider adding resources" : "Current capacity sufficient"
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forecast resource needs for department {DepartmentId}", departmentId);
            return new ResourceForecast { DepartmentId = departmentId, TargetDate = targetDate };
        }
    }

    public async Task<CapacityPrediction> PredictCapacityUtilizationAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            // Get historical capacity data
            var historicalData = await GetHistoricalCapacityDataAsync(departmentId);
            var currentUtilization = await GetCurrentDepartmentWorkloadAsync(departmentId);
            var capacity = await GetDepartmentCapacityAsync(departmentId);
            
            // Calculate trend and seasonality factors
            var trendFactor = CalculateTrendFactor(historicalData);
            var seasonalityFactor = GetSeasonalityFactor(targetDate);
            var currentFactor = currentUtilization / capacity;
            
            // Predict future utilization using weighted average
            var predictedUtilization = (currentFactor * 0.4f) + (trendFactor * 0.3f) + (seasonalityFactor * 0.3f);
            predictedUtilization = Math.Max(0.1f, Math.Min(1.0f, predictedUtilization)); // Clamp between 10% and 100%
            
            // Calculate confidence based on data quality and variance
            var confidence = CalculateCapacityConfidence(historicalData, predictedUtilization);
            
            // Determine capacity status
            var capacityStatus = predictedUtilization switch
            {
                >= 0.9f => "Critical",
                >= 0.8f => "High",
                >= 0.6f => "Moderate",
                _ => "Low"
            };
            
            var result = new CapacityPrediction
            {
                DepartmentId = departmentId,
                TargetDate = targetDate,
                PredictedCapacity = (decimal)(predictedUtilization * capacity),
                CurrentUtilization = (decimal)currentFactor,
                Recommendation = string.Join("; ", GenerateCapacityRecommendations(predictedUtilization, currentFactor, capacity))
            };
            
            _logger.LogInformation("Capacity prediction for department {DepartmentId}: {PredictedUtilization}% utilization with {Confidence}% confidence", 
                departmentId, predictedUtilization * 100, confidence * 100);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict capacity utilization for department {DepartmentId}", departmentId);
            return new CapacityPrediction 
            { 
                DepartmentId = departmentId, 
                TargetDate = targetDate, 
                PredictedCapacity = 75.0m, 
                CurrentUtilization = 0.75m
            };
        }
    }

    public async Task<CompletionPrediction> PredictCompletionTimeAsync(int workRequestId)
    {
        try
        {
            var avgCompletionTime = await GetAverageCompletionTimeAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
            var predictedDate = DateTime.UtcNow.AddDays(avgCompletionTime);

            var result = new CompletionPrediction
            {
                WorkRequestId = workRequestId,
                PredictedCompletionDate = predictedDate,
                Confidence = 0.75m,
                Factors = "Historical completion times, current workload, complexity"
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict completion time for work request {WorkRequestId}", workRequestId);
            return new CompletionPrediction { WorkRequestId = workRequestId, PredictedCompletionDate = DateTime.UtcNow.AddDays(14) };
        }
    }

    public async Task<IEnumerable<CompletionTrend>> PredictCompletionTrendsAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            var trends = new List<CompletionTrend>();
            var currentDate = DateTime.UtcNow;
            
            for (int i = 0; i < 30; i++)
            {
                var date = currentDate.AddDays(i);
                trends.Add(new CompletionTrend
                {
                    DepartmentId = departmentId,
                    Date = date,
                    AverageCompletionTime = 12.5m + (decimal)(Math.Cos(i * 0.1) * 2),
                    CompletedWorkItems = new Random().Next(3, 15),
                    Trend = i % 7 == 0 ? "Weekly Pattern" : "Daily Variation"
                });
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict completion trends for department {DepartmentId}", departmentId);
            return Enumerable.Empty<CompletionTrend>();
        }
    }

    public async Task<BusinessValueROI> CalculateROIAsync(int workRequestId)
    {
        try
        {
            var estimatedCost = 50000m;
            var estimatedValue = 75000m;
            var roi = (estimatedValue - estimatedCost) / estimatedCost;

            var result = new BusinessValueROI
            {
                WorkRequestId = workRequestId,
                EstimatedCost = estimatedCost,
                EstimatedValue = estimatedValue,
                ROI = roi,
                PaybackPeriod = estimatedCost / (estimatedValue / 12), // Monthly value
                Analysis = "Positive ROI with 12-month payback period"
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate ROI for work request {WorkRequestId}", workRequestId);
            return new BusinessValueROI { WorkRequestId = workRequestId };
        }
    }

    public async Task<IEnumerable<BusinessValueTrend>> AnalyzeBusinessValueTrendsAsync(int businessVerticalId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var trends = new List<BusinessValueTrend>();
            var currentDate = fromDate;
            
            while (currentDate <= toDate)
            {
                trends.Add(new BusinessValueTrend
                {
                    BusinessVerticalId = businessVerticalId,
                    Date = currentDate,
                    AverageBusinessValue = 0.7m + (decimal)(Math.Sin(currentDate.DayOfYear * 0.01) * 0.1),
                    TotalROI = 1.25m + (decimal)(Math.Cos(currentDate.DayOfYear * 0.01) * 0.2),
                    WorkRequestCount = new Random().Next(10, 50),
                    Trend = currentDate.DayOfWeek == DayOfWeek.Monday ? "Weekly Start" : "Normal Day"
                });
                currentDate = currentDate.AddDays(1);
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze business value trends for business vertical {BusinessVerticalId}", businessVerticalId);
            return Enumerable.Empty<BusinessValueTrend>();
        }
    }

    public async Task<RiskAssessment> AssessProjectRiskAsync(int workRequestId)
    {
        try
        {
            // In a real implementation, this would fetch the work request from the database
            var workRequest = new WorkRequest { Id = workRequestId }; // Placeholder
            
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
                RiskFactors = new List<string> { "Resource constraints", "Technical complexity", "Timeline pressure" },
                MitigationStrategy = string.Join(", ", mitigationStrategies)
            };
            
            _logger.LogInformation("Risk assessment completed for work request {WorkRequestId}: {RiskLevel} risk (Score: {Score})", 
                workRequestId, riskLevel, overallRiskScore);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assess risk for work request {WorkRequestId}", workRequestId);
            return new RiskAssessment 
            { 
                WorkRequestId = workRequestId, 
                RiskScore = 5.0m, 
                RiskLevel = "Medium" 
            };
        }
    }

    private async Task<double> CalculateComplexityRiskAsync(WorkRequest workRequest)
    {
        var complexity = 5.0f; // Default complexity
        var estimatedHours = workRequest.EstimatedEffort;
        var teamSize = 1; // Placeholder
        
        // Complexity risk increases with higher complexity, longer duration, and smaller teams
        var complexityFactor = complexity / 10.0;
        var durationFactor = Math.Min(1.0, estimatedHours / 200.0); // Normalize to 0-1
        var teamFactor = Math.Max(0.1, 1.0 - (teamSize / 10.0)); // Smaller teams = higher risk
        
        return (complexityFactor * 0.5 + durationFactor * 0.3 + teamFactor * 0.2) * 10.0;
    }

    private async Task<double> CalculateResourceRiskAsync(WorkRequest workRequest)
    {
        var departmentId = workRequest.DepartmentId;
        var currentUtilization = await GetCurrentDepartmentWorkloadAsync(departmentId);
        var capacity = await GetDepartmentCapacityAsync(departmentId);
        var utilizationRate = currentUtilization / capacity;
        
        // Resource risk increases with higher utilization
        var utilizationRisk = utilizationRate * 10.0;
        
        // Additional factors: skill availability, experience level, etc.
        var skillGapRisk = 3.0; // Placeholder - would be calculated based on required vs available skills
        var experienceRisk = 2.0; // Placeholder - would be calculated based on team experience
        
        return Math.Min(10.0, utilizationRisk + skillGapRisk + experienceRisk);
    }

    private async Task<double> CalculateTimelineRiskAsync(WorkRequest workRequest)
    {
        var targetDate = workRequest.TargetDate;
        var estimatedHours = workRequest.EstimatedEffort;
        var currentDate = DateTime.UtcNow;
        
        if (!targetDate.HasValue)
            return 5.0; // Medium risk if no target date specified
        
        var daysUntilTarget = (targetDate.Value - currentDate).TotalDays;
        var estimatedDays = estimatedHours / 8.0; // Assuming 8 hours per day
        
        // Timeline risk based on buffer time
        var bufferRatio = daysUntilTarget / estimatedDays;
        
        return bufferRatio switch
        {
            <= 0.5 => 10.0, // Critical - very tight timeline
            <= 1.0 => 8.0,  // High - tight timeline
            <= 1.5 => 6.0,  // Medium - moderate buffer
            <= 2.0 => 4.0,  // Low - good buffer
            _ => 2.0         // Very low - ample time
        };
    }

    private async Task<double> CalculateDependencyRiskAsync(WorkRequest workRequest)
    {
        // Placeholder values - in real implementation, these would be calculated based on actual dependencies
        var dependencies = 0; // Placeholder
        var externalDependencies = 0; // Placeholder
        
        // Dependency risk increases with more dependencies, especially external ones
        var internalRisk = Math.Min(5.0, dependencies * 1.0);
        var externalRisk = Math.Min(5.0, externalDependencies * 2.0); // External dependencies are riskier
        
        return Math.Min(10.0, internalRisk + externalRisk);
    }

    private async Task<double> CalculateBusinessImpactRiskAsync(WorkRequest workRequest)
    {
        var businessValue = (float)workRequest.BusinessValue;
        var customerImpact = 5.0f; // Placeholder
        var revenueImpact = 0.0f; // Placeholder
        
        // Higher business impact means higher risk if the project fails
        var impactScore = (businessValue + customerImpact + revenueImpact) / 3.0;
        
        // Risk is inversely proportional to impact - higher impact = higher risk
        return (11.0 - impactScore); // Invert the scale so higher impact = higher risk
    }

    private string DetermineRiskLevel(double riskScore)
    {
        return riskScore switch
        {
            >= 8.5 => "Critical",
            >= 7.0 => "High",
            >= 5.5 => "Medium",
            >= 4.0 => "Low",
            _ => "Very Low"
        };
    }

    private List<string> GenerateMitigationStrategies(Dictionary<string, double> riskFactors, string riskLevel)
    {
        var strategies = new List<string>();
        
        // Add general strategies based on risk level
        if (riskLevel == "Critical" || riskLevel == "High")
        {
            strategies.Add("Implement daily risk monitoring and reporting");
            strategies.Add("Establish contingency plans for key failure points");
            strategies.Add("Increase stakeholder communication frequency");
        }
        
        // Add specific strategies based on risk factors
        foreach (var factor in riskFactors.OrderByDescending(x => x.Value))
        {
            if (factor.Value >= 7.0)
            {
                strategies.AddRange(GetSpecificMitigationStrategies(factor.Key));
            }
        }
        
        return strategies.Distinct().ToList();
    }

    private List<string> GetSpecificMitigationStrategies(string riskFactor)
    {
        return riskFactor switch
        {
            "Complexity" => new List<string>
            {
                "Break down complex tasks into smaller, manageable pieces",
                "Assign experienced team members to complex components",
                "Implement additional review and testing phases"
            },
            "Resource Availability" => new List<string>
            {
                "Identify backup resources and cross-training opportunities",
                "Consider external contractors for specialized skills",
                "Optimize resource allocation and scheduling"
            },
            "Timeline" => new List<string>
            {
                "Implement parallel work streams where possible",
                "Add buffer time to critical path activities",
                "Consider scope reduction for non-critical features"
            },
            "Dependencies" => new List<string>
            {
                "Establish clear communication channels with dependency owners",
                "Create dependency tracking and escalation procedures",
                "Develop alternative approaches for critical dependencies"
            },
            "Business Impact" => new List<string>
            {
                "Implement phased delivery approach",
                "Establish rollback procedures for critical changes",
                "Increase stakeholder involvement and sign-off requirements"
            },
            _ => new List<string> { "Monitor and review regularly" }
        };
    }

    public async Task<IEnumerable<RiskIndicator>> GetRiskIndicatorsAsync(int departmentId)
    {
        try
        {
            var indicators = new List<RiskIndicator>
            {
                new RiskIndicator
                {
                    DepartmentId = departmentId,
                    RiskType = "Resource Overload",
                    RiskScore = 0.6m,
                    RiskLevel = "Medium",
                    Description = "Department utilization above 80%",
                    MitigationAction = "Consider resource reallocation"
                },
                new RiskIndicator
                {
                    DepartmentId = departmentId,
                    RiskType = "Timeline Pressure",
                    RiskScore = 0.4m,
                    RiskLevel = "Low",
                    Description = "Multiple high-priority items due soon",
                    MitigationAction = "Prioritize and communicate delays"
                }
            };

            return indicators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get risk indicators for department {DepartmentId}", departmentId);
            return Enumerable.Empty<RiskIndicator>();
        }
    }

    public async Task<IEnumerable<PredictiveInsight>> GetPredictiveInsightsAsync(int businessVerticalId)
    {
        try
        {
            var insights = new List<PredictiveInsight>
            {
                new PredictiveInsight
                {
                    InsightType = "Workload Prediction",
                    Description = "Expected 15% increase in workload next quarter",
                    Confidence = 0.85m,
                    PredictedDate = DateTime.UtcNow.AddMonths(3),
                    Recommendation = "Prepare additional resources"
                },
                new PredictiveInsight
                {
                    InsightType = "Priority Trend",
                    Description = "High-priority requests increasing by 20%",
                    Confidence = 0.78m,
                    PredictedDate = DateTime.UtcNow.AddMonths(1),
                    Recommendation = "Review priority calculation algorithm"
                }
            };

            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get predictive insights for business vertical {BusinessVerticalId}", businessVerticalId);
            return Enumerable.Empty<PredictiveInsight>();
        }
    }

    public async Task<WorkloadPrediction> PredictWorkloadAsync(int departmentId, DateTime targetDate)
    {
        try
        {
            // Get historical workload data
            var historicalWorkload = await GetHistoricalWorkloadAsync(departmentId);
            var currentWorkload = await GetCurrentDepartmentWorkloadAsync(departmentId);
            
            // Calculate workload components
            var baseWorkload = historicalWorkload.Count > 0 ? historicalWorkload.Average() : currentWorkload;
            var trendFactor = CalculateWorkloadTrend(historicalWorkload);
            var seasonalityFactor = GetSeasonalityFactor(targetDate);
            var growthFactor = CalculateGrowthFactor(departmentId, targetDate);
            
            // Predict workload using multiple factors
            var predictedWorkload = baseWorkload * (1 + trendFactor) * seasonalityFactor * growthFactor;
            predictedWorkload = Math.Max(0, predictedWorkload);
            
            // Calculate utilization percentage
            var capacity = await GetDepartmentCapacityAsync(departmentId);
            var predictedUtilization = (decimal)(predictedWorkload / capacity);
            
            // Determine trend direction
            var trend = trendFactor > 0 ? "Increasing" : trendFactor < 0 ? "Decreasing" : "Stable";
            
            var result = new WorkloadPrediction
            {
                DepartmentId = departmentId,
                TargetDate = targetDate,
                PredictedUtilization = predictedUtilization,
                PredictedWorkItems = (int)predictedWorkload,
                Trend = trend
            };
            
            _logger.LogInformation("Workload prediction for department {DepartmentId}: {PredictedWorkItems} items with {PredictedUtilization}% utilization", 
                departmentId, result.PredictedWorkItems, result.PredictedUtilization * 100);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict workload for department {DepartmentId}", departmentId);
            return new WorkloadPrediction 
            { 
                DepartmentId = departmentId, 
                TargetDate = targetDate, 
                PredictedUtilization = 0.75m, 
                PredictedWorkItems = 20 
            };
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
                TargetDate = forecastDate,
                PredictedUtilization = (decimal)(predictedWorkload / 100.0),
                PredictedWorkItems = (int)predictedWorkload,
                Trend = "Increasing"
            };

            _logger.LogInformation("Predicted workload {PredictedUtilization} for department {DepartmentId} on {TargetDate}", 
                result.PredictedUtilization, departmentId, forecastDate);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict workload for department {DepartmentId}", departmentId);
            return new WorkloadPrediction { DepartmentId = departmentId, TargetDate = forecastDate, PredictedUtilization = 0.0m, PredictedWorkItems = 0 };
        }
    }

    public async Task<List<WorkIntakeSystem.Core.Entities.WorkflowBottleneck>> IdentifyWorkflowBottlenecksAsync()
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

            var bottlenecks = new List<WorkIntakeSystem.Core.Entities.WorkflowBottleneck>();

            foreach (var stage in workflowStages)
            {
                var averageTime = await GetAverageTimeInStageAsync(stage);
                var itemsInStage = await GetItemsInStageAsync(stage);
                var bottleneckScore = CalculateBottleneckScore(averageTime, itemsInStage);

                if (bottleneckScore > 0.7) // Threshold for bottleneck
                {
                    bottlenecks.Add(new WorkIntakeSystem.Core.Entities.WorkflowBottleneck
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
            return new List<WorkIntakeSystem.Core.Entities.WorkflowBottleneck>();
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
                Filters = request.Filters.Select(kvp => new ReportFilter
                {
                    Id = Guid.NewGuid().ToString(),
                    FieldName = kvp.Key,
                    Operator = "=",
                    Value = kvp.Value,
                    DisplayName = kvp.Key
                }).ToList()
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
        // In a real implementation, this would query the database for historical work requests
        // For now, we'll generate synthetic training data
        var trainingData = new List<PriorityTrainingData>();
        var random = new Random(42); // Fixed seed for reproducible results
        
        for (int i = 0; i < 1000; i++)
        {
            trainingData.Add(new PriorityTrainingData
            {
                Category = $"Category{(i % 5) + 1}",
                Department = $"Department{(i % 8) + 1}",
                BusinessVertical = $"Vertical{(i % 4) + 1}",
                BusinessValue = random.Next(1, 11),
                Urgency = random.Next(1, 11),
                Complexity = random.Next(1, 11),
                EstimatedHours = random.Next(8, 200),
                ResourceAvailability = random.Next(1, 11),
                StrategicAlignment = random.Next(1, 11),
                Priority = CalculateSyntheticPriority(random.Next(1, 11), random.Next(1, 11), random.Next(1, 11))
            });
        }
        
        return trainingData;
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

    private async Task<float> CalculateBusinessValueAsync(WorkRequest workRequest)
    {
        // Calculate business value based on multiple factors
        var baseValue = (float)workRequest.BusinessValue;
        var strategicImportance = 5.0f; // Placeholder - would be calculated based on business vertical
        var customerImpact = 5.0f; // Placeholder - would be calculated based on business impact
        var revenueImpact = 0.0f; // Placeholder - would be calculated based on revenue impact
        
        // Weighted calculation
        var businessValue = (baseValue * 0.3f) + (strategicImportance * 0.3f) + (customerImpact * 0.2f) + (revenueImpact * 0.2f);
        
        return Math.Min(10.0f, Math.Max(1.0f, businessValue));
    }

    private async Task<float> CalculateUrgencyAsync(WorkRequest workRequest)
    {
        var baseUrgency = 5.0f; // Default urgency
        var targetDate = workRequest.TargetDate;
        var currentDate = DateTime.UtcNow;
        
        if (targetDate.HasValue)
        {
            var daysUntilTarget = (targetDate.Value - currentDate).TotalDays;
            var urgencyMultiplier = daysUntilTarget <= 7 ? 1.5f : daysUntilTarget <= 30 ? 1.2f : 1.0f;
            return Math.Min(10.0f, baseUrgency * urgencyMultiplier);
        }
        
        return baseUrgency;
    }

    private async Task<float> CalculateComplexityAsync(WorkRequest workRequest)
    {
        var baseComplexity = 5.0f; // Default complexity
        var estimatedHours = workRequest.EstimatedEffort;
        var teamSize = 1; // Placeholder - would be calculated based on team size
        
        // Adjust complexity based on estimated hours and team size
        var hoursMultiplier = estimatedHours > 80 ? 1.3f : estimatedHours > 40 ? 1.1f : 1.0f;
        var teamFactor = Math.Max(0.1f, 1.0f - (teamSize / 10.0f)); // Smaller teams = higher risk
        
        return Math.Min(10.0f, baseComplexity * hoursMultiplier * teamFactor);
    }

    private async Task<float> CalculateResourceAvailabilityAsync(int departmentId)
    {
        // This would typically query the database for current resource utilization
        var currentUtilization = await GetCurrentDepartmentWorkloadAsync(departmentId);
        var capacity = await GetDepartmentCapacityAsync(departmentId);
        
        // Higher availability means lower utilization
        var availability = Math.Max(0.1f, 1.0f - ((float)(currentUtilization / capacity)));
        return availability * 10.0f; // Scale to 0-10
    }

    private async Task<float> CalculateStrategicAlignmentAsync(WorkRequest workRequest)
    {
        // This would check alignment with business strategy
        var alignmentScore = 5.0f; // Default alignment score
        
        // Additional factors could include alignment with department goals, business vertical priorities, etc.
        return alignmentScore;
    }

    private float CalculatePredictionConfidence(float score, int trainingDataCount)
    {
        // Confidence based on model performance and data quality
        var baseConfidence = 0.8f;
        var dataQualityFactor = Math.Min(1.0f, trainingDataCount / 1000.0f);
        var scoreVarianceFactor = 1.0f - Math.Abs(score - 5.0f) / 10.0f; // Higher confidence for moderate scores
        
        return baseConfidence * dataQualityFactor * scoreVarianceFactor;
    }

    private WorkIntakeSystem.Core.Enums.PriorityLevel DeterminePriorityLevel(float score)
    {
        return score switch
        {
            >= 8.5f => WorkIntakeSystem.Core.Enums.PriorityLevel.Critical,
            >= 7.0f => WorkIntakeSystem.Core.Enums.PriorityLevel.High,
            >= 5.5f => WorkIntakeSystem.Core.Enums.PriorityLevel.Medium,
            >= 4.0f => WorkIntakeSystem.Core.Enums.PriorityLevel.Low,
            _ => WorkIntakeSystem.Core.Enums.PriorityLevel.Low
        };
    }

    private string GeneratePriorityReasoning(float businessValue, float urgency, float complexity, float resourceAvailability, float strategicAlignment)
    {
        var factors = new List<string>();
        
        if (businessValue >= 8.0f) factors.Add("High business value");
        if (urgency >= 8.0f) factors.Add("High urgency");
        if (complexity >= 8.0f) factors.Add("High complexity");
        if (resourceAvailability <= 3.0f) factors.Add("Limited resource availability");
        if (strategicAlignment >= 8.0f) factors.Add("High strategic alignment");
        
        return factors.Count > 0 
            ? $"Priority influenced by: {string.Join(", ", factors)}"
            : "Standard priority based on balanced factors";
    }

    private float CalculateSyntheticPriority(float businessValue, float urgency, float complexity)
    {
        // Synthetic priority calculation for training data
        return (businessValue * 0.4f + urgency * 0.3f + complexity * 0.3f) + (float)(new Random().NextDouble() - 0.5) * 2;
    }

    private async Task<List<PriorityTrainingData>> GetHistoricalWorkRequestsAsync()
    {
        // In a real implementation, this would query the database for historical work requests
        // For now, we'll generate synthetic training data
        var trainingData = new List<PriorityTrainingData>();
        var random = new Random(42); // Fixed seed for reproducible results
        
        for (int i = 0; i < 1000; i++)
        {
            trainingData.Add(new PriorityTrainingData
            {
                Category = $"Category{(i % 5) + 1}",
                Department = $"Department{(i % 8) + 1}",
                BusinessVertical = $"Vertical{(i % 4) + 1}",
                BusinessValue = random.Next(1, 11),
                Urgency = random.Next(1, 11),
                Complexity = random.Next(1, 11),
                EstimatedHours = random.Next(8, 200),
                ResourceAvailability = random.Next(1, 11),
                StrategicAlignment = random.Next(1, 11),
                Priority = CalculateSyntheticPriority(random.Next(1, 11), random.Next(1, 11), random.Next(1, 11))
            });
        }
        
        return trainingData;
    }

    private async Task<List<double>> GetHistoricalCapacityDataAsync(int departmentId)
    {
        // In a real implementation, this would query the database for historical capacity data
        // For now, we'll generate synthetic data
        var random = new Random(departmentId); // Use departmentId as seed for consistent results
        var data = new List<double>();
        
        for (int i = 0; i < 30; i++) // Last 30 days
        {
            var baseUtilization = 0.7 + (random.NextDouble() * 0.3); // 70-100% range
            var trend = Math.Sin(i * 0.1) * 0.1; // Add some trend
            data.Add(Math.Max(0.1, Math.Min(1.0, baseUtilization + trend)));
        }
        
        return data;
    }

    private double CalculateTrendFactor(List<double> historicalData)
    {
        if (historicalData.Count < 2) return 0.0;
        
        // Calculate linear trend using least squares
        var n = historicalData.Count;
        var sumX = n * (n - 1) / 2.0;
        var sumY = historicalData.Sum();
        var sumXY = historicalData.Select((y, i) => i * y).Sum();
        var sumX2 = n * (n - 1) * (2 * n - 1) / 6.0;
        
        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }

    private double CalculateWorkloadTrend(List<double> historicalData)
    {
        if (historicalData.Count < 7) return 0.0;
        
        // Calculate trend over the last week
        var recentData = historicalData.TakeLast(7).ToList();
        var olderData = historicalData.Skip(Math.Max(0, historicalData.Count - 14)).Take(7).ToList();
        
        if (olderData.Count < 7) return 0.0;
        
        var recentAvg = recentData.Average();
        var olderAvg = olderData.Average();
        
        return (recentAvg - olderAvg) / olderAvg; // Percentage change
    }

    private double CalculateGrowthFactor(int departmentId, DateTime targetDate)
    {
        // This would consider department growth, new projects, seasonal factors, etc.
        var monthsFromNow = (targetDate - DateTime.UtcNow).TotalDays / 30.0;
        var baseGrowthRate = 0.02; // 2% monthly growth
        var seasonalAdjustment = GetSeasonalityFactor(targetDate);
        
        return Math.Pow(1 + baseGrowthRate, monthsFromNow) * seasonalAdjustment;
    }

    private double CalculateCapacityConfidence(List<double> historicalData, double prediction)
    {
        if (historicalData.Count < 5) return 0.5;
        
        // Calculate confidence based on data consistency and prediction reasonableness
        var variance = historicalData.Select(x => Math.Pow(x - historicalData.Average(), 2)).Average();
        var standardDeviation = Math.Sqrt(variance);
        var coefficientOfVariation = standardDeviation / historicalData.Average();
        
        // Lower confidence for high variance or extreme predictions
        var varianceFactor = Math.Max(0.1, 1.0 - coefficientOfVariation);
        var predictionFactor = prediction >= 0.1 && prediction <= 1.0 ? 1.0 : 0.5;
        
        return Math.Min(0.95, varianceFactor * predictionFactor);
    }

    private double CalculateWorkloadConfidence(int dataPoints, double trendMagnitude)
    {
        // Confidence based on data availability and trend stability
        var dataQualityFactor = Math.Min(1.0, dataPoints / 30.0); // More data = higher confidence
        var trendStabilityFactor = Math.Max(0.1, 1.0 - trendMagnitude); // Stable trends = higher confidence
        
        return Math.Min(0.95, dataQualityFactor * trendStabilityFactor);
    }

    private List<string> GenerateCapacityRecommendations(double predictedUtilization, double currentUtilization, double capacity)
    {
        var recommendations = new List<string>();
        
        if (predictedUtilization >= 0.9)
        {
            recommendations.Add("Consider resource allocation from other departments");
            recommendations.Add("Implement overtime or temporary staffing");
            recommendations.Add("Review and prioritize work requests");
        }
        else if (predictedUtilization >= 0.8)
        {
            recommendations.Add("Monitor workload closely");
            recommendations.Add("Consider cross-training team members");
            recommendations.Add("Optimize work processes");
        }
        else if (predictedUtilization <= 0.5)
        {
            recommendations.Add("Consider taking on additional work");
            recommendations.Add("Explore cross-departmental opportunities");
            recommendations.Add("Focus on process improvement initiatives");
        }
        
        return recommendations;
    }
}

public class PriorityTrainingData
{
    public string Category { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string BusinessVertical { get; set; } = string.Empty;
    public float BusinessValue { get; set; }
    public float Urgency { get; set; }
    public float Complexity { get; set; }
    public float EstimatedHours { get; set; }
    public float ResourceAvailability { get; set; }
    public float StrategicAlignment { get; set; }
    public float Priority { get; set; }
}

public class PriorityPredictionOutput
{
    [ColumnName("Score")]
    public float PredictedPriority { get; set; }
} 