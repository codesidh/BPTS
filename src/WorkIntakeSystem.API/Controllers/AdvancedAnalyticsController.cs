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

    // Predictive Analytics
    [HttpPost("predict/priority")]
    public async Task<IActionResult> PredictWorkRequestPriority([FromBody] WorkRequest workRequest)
    {
        try
        {
            var prediction = await _advancedAnalyticsService.PredictWorkRequestPriorityAsync(workRequest);
            return Ok(new { Success = true, Prediction = prediction });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting work request priority");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("predict/workload/{departmentId}")]
    public async Task<IActionResult> PredictDepartmentWorkload(int departmentId, [FromQuery] DateTime forecastDate)
    {
        try
        {
            var prediction = await _advancedAnalyticsService.PredictDepartmentWorkloadAsync(departmentId, forecastDate);
            return Ok(new { Success = true, Prediction = prediction });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting department workload");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("workflow/bottlenecks")]
    public async Task<IActionResult> IdentifyWorkflowBottlenecks()
    {
        try
        {
            var bottlenecks = await _advancedAnalyticsService.IdentifyWorkflowBottlenecksAsync();
            return Ok(new { Success = true, Bottlenecks = bottlenecks });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying workflow bottlenecks");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("optimization/resources/{departmentId}")]
    public async Task<IActionResult> GetResourceOptimizationSuggestions(int departmentId)
    {
        try
        {
            var suggestions = await _advancedAnalyticsService.GetResourceOptimizationSuggestionsAsync(departmentId);
            return Ok(new { Success = true, Suggestions = suggestions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource optimization suggestions");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Business Intelligence Dashboards
    [HttpGet("dashboard/executive")]
    public async Task<IActionResult> GetExecutiveDashboard([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var dashboard = await _advancedAnalyticsService.GetExecutiveDashboardAsync(startDate, endDate);
            return Ok(new { Success = true, Dashboard = dashboard });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting executive dashboard");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("dashboard/department/{departmentId}")]
    public async Task<IActionResult> GetDepartmentDashboard(int departmentId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var dashboard = await _advancedAnalyticsService.GetDepartmentDashboardAsync(departmentId, startDate, endDate);
            return Ok(new { Success = true, Dashboard = dashboard });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department dashboard");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("dashboard/project/{projectId}")]
    public async Task<IActionResult> GetProjectDashboard(int projectId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var dashboard = await _advancedAnalyticsService.GetProjectDashboardAsync(projectId, startDate, endDate);
            return Ok(new { Success = true, Dashboard = dashboard });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project dashboard");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Custom Report Builder
    [HttpPost("reports/custom")]
    public async Task<IActionResult> BuildCustomReport([FromBody] CustomReportRequest request)
    {
        try
        {
            var report = await _advancedAnalyticsService.BuildCustomReportAsync(request);
            return Ok(new { Success = true, Report = report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building custom report");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("reports/templates")]
    public async Task<IActionResult> GetReportTemplates()
    {
        try
        {
            var templates = await _advancedAnalyticsService.GetReportTemplatesAsync();
            return Ok(new { Success = true, Templates = templates });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report templates");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("reports/templates")]
    public async Task<IActionResult> SaveReportTemplate([FromBody] ReportTemplate template)
    {
        try
        {
            var templateId = await _advancedAnalyticsService.SaveReportTemplateAsync(template);
            
            if (!string.IsNullOrEmpty(templateId))
            {
                return Ok(new { Success = true, TemplateId = templateId });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to save report template" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving report template");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("reports/{reportId}/export")]
    public async Task<IActionResult> ExportReport(string reportId, [FromQuery] ExportFormat format)
    {
        try
        {
            var data = await _advancedAnalyticsService.ExportReportAsync(reportId, format);
            
            if (data.Length > 0)
            {
                var contentType = format switch
                {
                    ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ExportFormat.CSV => "text/csv",
                    ExportFormat.JSON => "application/json",
                    ExportFormat.PDF => "application/pdf",
                    _ => "application/octet-stream"
                };

                var fileName = $"report_{reportId}.{format.ToString().ToLower()}";
                return File(data, contentType, fileName);
            }
            
            return BadRequest(new { Success = false, Message = "Failed to export report" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Data Export
    [HttpPost("export/data")]
    public async Task<IActionResult> ExportData([FromBody] DataExportRequest request)
    {
        try
        {
            var data = await _advancedAnalyticsService.ExportDataAsync(request);
            
            if (data.Length > 0)
            {
                var contentType = request.Format switch
                {
                    ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ExportFormat.CSV => "text/csv",
                    ExportFormat.JSON => "application/json",
                    ExportFormat.PDF => "application/pdf",
                    _ => "application/octet-stream"
                };

                var fileName = $"{request.EntityType}_export.{request.Format.ToString().ToLower()}";
                return File(data, contentType, fileName);
            }
            
            return BadRequest(new { Success = false, Message = "Failed to export data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("export/schedule")]
    public async Task<IActionResult> ScheduleDataExport([FromBody] DataExportSchedule schedule)
    {
        try
        {
            var scheduleId = await _advancedAnalyticsService.ScheduleDataExportAsync(schedule);
            
            if (!string.IsNullOrEmpty(scheduleId))
            {
                return Ok(new { Success = true, ScheduleId = scheduleId });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to schedule data export" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling data export");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("export/history")]
    public async Task<IActionResult> GetExportHistory()
    {
        try
        {
            var history = await _advancedAnalyticsService.GetExportHistoryAsync();
            return Ok(new { Success = true, History = history });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting export history");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }
} 