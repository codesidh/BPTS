using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvancedAnalyticsController : ControllerBase
{
    private readonly IAdvancedAnalyticsService _advancedAnalyticsService;
    private readonly ILogger<AdvancedAnalyticsController> _logger;

    public AdvancedAnalyticsController(
        IAdvancedAnalyticsService advancedAnalyticsService,
        ILogger<AdvancedAnalyticsController> logger)
    {
        _advancedAnalyticsService = advancedAnalyticsService;
        _logger = logger;
    }

    // Priority Prediction
    [HttpGet("predict/priority/{workRequestId}")]
    public async Task<IActionResult> PredictPriority(int workRequestId)
    {
        try
        {
            var prediction = await _advancedAnalyticsService.PredictPriorityAsync(workRequestId);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting priority for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Error predicting priority" });
        }
    }

    [HttpGet("predict/priority-trends/{departmentId}")]
    public async Task<IActionResult> PredictPriorityTrends(int departmentId, [FromQuery] DateTime targetDate)
    {
        try
        {
            var trends = await _advancedAnalyticsService.PredictPriorityTrendsAsync(departmentId, targetDate);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting priority trends for department {DepartmentId}", departmentId);
            return StatusCode(500, new { Success = false, Message = "Error predicting priority trends" });
        }
    }

    // Resource Forecasting
    [HttpGet("forecast/resources/{departmentId}")]
    public async Task<IActionResult> ForecastResourceNeeds(int departmentId, [FromQuery] DateTime targetDate)
    {
        try
        {
            var forecast = await _advancedAnalyticsService.ForecastResourceNeedsAsync(departmentId, targetDate);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting resource needs for department {DepartmentId}", departmentId);
            return StatusCode(500, new { Success = false, Message = "Error forecasting resource needs" });
        }
    }

    [HttpGet("predict/capacity/{departmentId}")]
    public async Task<IActionResult> PredictCapacityUtilization(int departmentId, [FromQuery] DateTime targetDate)
    {
        try
        {
            var prediction = await _advancedAnalyticsService.PredictCapacityUtilizationAsync(departmentId, targetDate);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting capacity utilization for department {DepartmentId}", departmentId);
            return StatusCode(500, new { Success = false, Message = "Error predicting capacity utilization" });
        }
    }

    // Completion Prediction
    [HttpGet("predict/completion/{workRequestId}")]
    public async Task<IActionResult> PredictCompletionTime(int workRequestId)
    {
        try
        {
            var prediction = await _advancedAnalyticsService.PredictCompletionTimeAsync(workRequestId);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting completion time for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Error predicting completion time" });
        }
    }

    [HttpGet("predict/completion-trends/{departmentId}")]
    public async Task<IActionResult> PredictCompletionTrends(int departmentId, [FromQuery] DateTime targetDate)
    {
        try
        {
            var trends = await _advancedAnalyticsService.PredictCompletionTrendsAsync(departmentId, targetDate);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting completion trends for department {DepartmentId}", departmentId);
            return StatusCode(500, new { Success = false, Message = "Error predicting completion trends" });
        }
    }

    // Business Value Analysis
    [HttpGet("analyze/roi/{workRequestId}")]
    public async Task<IActionResult> CalculateROI(int workRequestId)
    {
        try
        {
            var roi = await _advancedAnalyticsService.CalculateROIAsync(workRequestId);
            return Ok(roi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ROI for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Error calculating ROI" });
        }
    }

    [HttpGet("analyze/business-value-trends/{businessVerticalId}")]
    public async Task<IActionResult> AnalyzeBusinessValueTrends(int businessVerticalId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var trends = await _advancedAnalyticsService.AnalyzeBusinessValueTrendsAsync(businessVerticalId, fromDate, toDate);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing business value trends for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, new { Success = false, Message = "Error analyzing business value trends" });
        }
    }

    // Risk Assessment
    [HttpGet("assess/risk/{workRequestId}")]
    public async Task<IActionResult> AssessProjectRisk(int workRequestId)
    {
        try
        {
            var assessment = await _advancedAnalyticsService.AssessProjectRiskAsync(workRequestId);
            return Ok(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing project risk for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Error assessing project risk" });
        }
    }

    [HttpGet("assess/risk-indicators/{departmentId}")]
    public async Task<IActionResult> GetRiskIndicators(int departmentId)
    {
        try
        {
            var indicators = await _advancedAnalyticsService.GetRiskIndicatorsAsync(departmentId);
            return Ok(indicators);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk indicators for department {DepartmentId}", departmentId);
            return StatusCode(500, new { Success = false, Message = "Error getting risk indicators" });
        }
    }

    // Predictive Insights
    [HttpGet("insights/predictive/{businessVerticalId}")]
    public async Task<IActionResult> GetPredictiveInsights(int businessVerticalId)
    {
        try
        {
            var insights = await _advancedAnalyticsService.GetPredictiveInsightsAsync(businessVerticalId);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting predictive insights for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, new { Success = false, Message = "Error getting predictive insights" });
        }
    }

    [HttpGet("predict/workload/{departmentId}")]
    public async Task<IActionResult> PredictWorkload(int departmentId, [FromQuery] DateTime targetDate)
    {
        try
        {
            var prediction = await _advancedAnalyticsService.PredictWorkloadAsync(departmentId, targetDate);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting workload for department {DepartmentId}", departmentId);
            return StatusCode(500, new { Success = false, Message = "Error predicting workload" });
        }
    }

    // Business Intelligence Dashboards
    [HttpGet("dashboard/executive")]
    public async Task<IActionResult> GetExecutiveDashboard([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            // Combine multiple analytics for executive dashboard
            var dashboard = new
            {
                PriorityPredictions = await GetPriorityPredictionsForPeriod(startDate, endDate),
                ResourceForecasts = await GetResourceForecastsForPeriod(startDate, endDate),
                RiskAssessments = await GetRiskAssessmentsForPeriod(startDate, endDate),
                BusinessValueTrends = await GetBusinessValueTrendsForPeriod(startDate, endDate),
                CompletionPredictions = await GetCompletionPredictionsForPeriod(startDate, endDate)
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting executive dashboard");
            return StatusCode(500, new { Success = false, Message = "Error getting executive dashboard" });
        }
    }

    [HttpGet("dashboard/department/{departmentId}")]
    public async Task<IActionResult> GetDepartmentDashboard(int departmentId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var dashboard = new
            {
                PriorityTrends = await _advancedAnalyticsService.PredictPriorityTrendsAsync(departmentId, endDate),
                ResourceForecast = await _advancedAnalyticsService.ForecastResourceNeedsAsync(departmentId, endDate),
                CapacityPrediction = await _advancedAnalyticsService.PredictCapacityUtilizationAsync(departmentId, endDate),
                CompletionTrends = await _advancedAnalyticsService.PredictCompletionTrendsAsync(departmentId, endDate),
                WorkloadPrediction = await _advancedAnalyticsService.PredictWorkloadAsync(departmentId, endDate),
                RiskIndicators = await _advancedAnalyticsService.GetRiskIndicatorsAsync(departmentId)
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department dashboard for department {DepartmentId}", departmentId);
            return StatusCode(500, new { Success = false, Message = "Error getting department dashboard" });
        }
    }

    [HttpGet("dashboard/project/{workRequestId}")]
    public async Task<IActionResult> GetProjectDashboard(int workRequestId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var dashboard = new
            {
                PriorityPrediction = await _advancedAnalyticsService.PredictPriorityAsync(workRequestId),
                CompletionPrediction = await _advancedAnalyticsService.PredictCompletionTimeAsync(workRequestId),
                RiskAssessment = await _advancedAnalyticsService.AssessProjectRiskAsync(workRequestId),
                ROIAnalysis = await _advancedAnalyticsService.CalculateROIAsync(workRequestId)
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project dashboard for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Error getting project dashboard" });
        }
    }

    // Analytics Summary
    [HttpGet("summary/priority")]
    public async Task<IActionResult> GetPrioritySummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var summary = new
            {
                TotalWorkRequests = await GetTotalWorkRequestsInPeriod(fromDate, toDate),
                AveragePriority = await GetAveragePriorityInPeriod(fromDate, toDate),
                PriorityDistribution = await GetPriorityDistributionInPeriod(fromDate, toDate),
                HighPriorityCount = await GetHighPriorityCountInPeriod(fromDate, toDate)
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority summary");
            return StatusCode(500, new { Success = false, Message = "Error getting priority summary" });
        }
    }

    [HttpGet("summary/completion")]
    public async Task<IActionResult> GetCompletionSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var summary = new
            {
                CompletedWorkRequests = await GetCompletedWorkRequestsInPeriod(fromDate, toDate),
                AverageCompletionTime = await GetAverageCompletionTimeInPeriod(fromDate, toDate),
                CompletionRate = await GetCompletionRateInPeriod(fromDate, toDate),
                OnTimeCompletionRate = await GetOnTimeCompletionRateInPeriod(fromDate, toDate)
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion summary");
            return StatusCode(500, new { Success = false, Message = "Error getting completion summary" });
        }
    }

    [HttpGet("summary/risk")]
    public async Task<IActionResult> GetRiskSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var summary = new
            {
                TotalRiskAssessments = await GetTotalRiskAssessmentsInPeriod(fromDate, toDate),
                HighRiskCount = await GetHighRiskCountInPeriod(fromDate, toDate),
                AverageRiskScore = await GetAverageRiskScoreInPeriod(fromDate, toDate),
                RiskTrends = await GetRiskTrendsInPeriod(fromDate, toDate)
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk summary");
            return StatusCode(500, new { Success = false, Message = "Error getting risk summary" });
        }
    }

    // Helper methods for dashboard data
    private async Task<object> GetPriorityPredictionsForPeriod(DateTime startDate, DateTime endDate)
    {
        // This would aggregate priority predictions across all departments
        return new { TotalPredictions = 150, AveragePredictedPriority = 0.65m, Trend = "Stable" };
    }

    private async Task<object> GetResourceForecastsForPeriod(DateTime startDate, DateTime endDate)
    {
        // This would aggregate resource forecasts across all departments
        return new { TotalDepartments = 8, AverageUtilization = 0.75m, CapacityGap = 15 };
    }

    private async Task<object> GetRiskAssessmentsForPeriod(DateTime startDate, DateTime endDate)
    {
        // This would aggregate risk assessments across all work requests
        return new { TotalAssessments = 45, HighRiskCount = 8, AverageRiskScore = 0.42m };
    }

    private async Task<object> GetBusinessValueTrendsForPeriod(DateTime startDate, DateTime endDate)
    {
        // This would aggregate business value trends across all business verticals
        return new { TotalTrends = 12, AverageBusinessValue = 0.68m, Trend = "Increasing" };
    }

    private async Task<object> GetCompletionPredictionsForPeriod(DateTime startDate, DateTime endDate)
    {
        // This would aggregate completion predictions across all work requests
        return new { TotalPredictions = 89, AveragePredictedDays = 12.5m, OnTimeRate = 0.78m };
    }

    private async Task<int> GetTotalWorkRequestsInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would query the database for total work requests in the period
        return 150;
    }

    private async Task<decimal> GetAveragePriorityInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would calculate average priority for work requests in the period
        return 0.65m;
    }

    private async Task<object> GetPriorityDistributionInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would calculate priority distribution for work requests in the period
        return new { Critical = 15, High = 45, Medium = 60, Low = 30 };
    }

    private async Task<int> GetHighPriorityCountInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would count high priority work requests in the period
        return 60;
    }

    private async Task<int> GetCompletedWorkRequestsInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would count completed work requests in the period
        return 85;
    }

    private async Task<decimal> GetAverageCompletionTimeInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would calculate average completion time for work requests in the period
        return 14.2m;
    }

    private async Task<decimal> GetCompletionRateInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would calculate completion rate for work requests in the period
        return 0.78m;
    }

    private async Task<decimal> GetOnTimeCompletionRateInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would calculate on-time completion rate for work requests in the period
        return 0.65m;
    }

    private async Task<int> GetTotalRiskAssessmentsInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would count total risk assessments in the period
        return 45;
    }

    private async Task<int> GetHighRiskCountInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would count high risk work requests in the period
        return 8;
    }

    private async Task<decimal> GetAverageRiskScoreInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would calculate average risk score for work requests in the period
        return 0.42m;
    }

    private async Task<object> GetRiskTrendsInPeriod(DateTime fromDate, DateTime toDate)
    {
        // This would calculate risk trends in the period
        return new { Trend = "Decreasing", Change = -0.05m };
    }
} 