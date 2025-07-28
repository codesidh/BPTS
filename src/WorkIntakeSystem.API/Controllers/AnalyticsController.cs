using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace WorkIntakeSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get comprehensive dashboard analytics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardAnalytics(
            [FromQuery] int? businessVerticalId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetDashboardAnalyticsAsync(businessVerticalId, fromDate, toDate);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve dashboard analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Get department-specific analytics
        /// </summary>
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetDepartmentAnalytics(
            int departmentId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetDepartmentAnalyticsAsync(departmentId, fromDate, toDate);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve department analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Get workflow analytics and bottlenecks
        /// </summary>
        [HttpGet("workflow")]
        public async Task<IActionResult> GetWorkflowAnalytics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetWorkflowAnalyticsAsync(fromDate, toDate);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve workflow analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Get priority analytics and trends
        /// </summary>
        [HttpGet("priority")]
        public async Task<IActionResult> GetPriorityAnalytics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetPriorityAnalyticsAsync(fromDate, toDate);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve priority analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Get resource utilization analytics
        /// </summary>
        [HttpGet("resource-utilization")]
        public async Task<IActionResult> GetResourceUtilization(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetResourceUtilizationAsync(fromDate, toDate);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve resource utilization: {ex.Message}");
            }
        }

        /// <summary>
        /// Get SLA compliance analytics
        /// </summary>
        [HttpGet("sla-compliance")]
        public async Task<IActionResult> GetSLACompliance(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetSLAComplianceAsync(fromDate, toDate);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve SLA compliance: {ex.Message}");
            }
        }

        /// <summary>
        /// Get trend data for specific metrics
        /// </summary>
        [HttpGet("trends")]
        public async Task<IActionResult> GetTrendData(
            [FromQuery] string metric,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] string? groupBy = null)
        {
            try
            {
                if (string.IsNullOrEmpty(metric))
                {
                    return BadRequest("Metric parameter is required");
                }

                var trends = await _analyticsService.GetTrendDataAsync(metric, fromDate, toDate, groupBy);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve trend data: {ex.Message}");
            }
        }

        /// <summary>
        /// Export analytics data as CSV
        /// </summary>
        [HttpGet("export")]
        [Authorize(Roles = "BusinessExecutive,SystemAdministrator")]
        public async Task<IActionResult> ExportAnalytics(
            [FromQuery] string reportType,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // This would implement CSV export functionality
                // For now, return a placeholder response
                return Ok(new { message = "Export functionality will be implemented", reportType, fromDate, toDate });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to export analytics: {ex.Message}");
            }
        }
    }
} 