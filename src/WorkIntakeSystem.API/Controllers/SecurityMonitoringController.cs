using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SystemAdministrator,SecurityOfficer")]
public class SecurityMonitoringController : ControllerBase
{
    private readonly ISecurityMonitoringService _securityMonitoringService;
    private readonly ILogger<SecurityMonitoringController> _logger;

    public SecurityMonitoringController(
        ISecurityMonitoringService securityMonitoringService,
        ILogger<SecurityMonitoringController> logger)
    {
        _securityMonitoringService = securityMonitoringService;
        _logger = logger;
    }

    #region Threat Detection

    [HttpPost("detect-threat")]
    public async Task<IActionResult> DetectThreat([FromBody] SecurityEvent securityEvent)
    {
        try
        {
            var isSuspicious = await _securityMonitoringService.DetectSuspiciousActivityAsync(securityEvent);
            return Ok(new { IsSuspicious = isSuspicious, EventId = securityEvent.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting threat");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("threats/active")]
    public async Task<IActionResult> GetActiveThreats()
    {
        try
        {
            var threats = await _securityMonitoringService.GetActiveThreatsAsync();
            return Ok(threats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active threats");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("threats/{id}")]
    public async Task<IActionResult> GetThreat(int id)
    {
        try
        {
            var threats = await _securityMonitoringService.GetActiveThreatsAsync();
            var threat = threats.FirstOrDefault(t => t.Id == id);
            
            if (threat == null)
                return NotFound();

            return Ok(threat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving threat {ThreatId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("threats/{id}/resolve")]
    public async Task<IActionResult> ResolveThreat(int id, [FromBody] ResolveThreatRequest request)
    {
        try
        {
            // This would typically update the threat status in the database
            // For now, we'll return a success response
            _logger.LogInformation("Threat {ThreatId} resolved by {ResolvedBy}: {Notes}", 
                id, request.ResolvedBy, request.ResolutionNotes);
            
            return Ok(new { Message = "Threat resolved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving threat {ThreatId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Compliance Monitoring

    [HttpGet("compliance/{framework}")]
    public async Task<IActionResult> CheckCompliance(ComplianceFramework framework)
    {
        try
        {
            var status = await _securityMonitoringService.CheckComplianceAsync(framework);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking compliance for framework {Framework}", framework);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("compliance/{framework}/violations")]
    public async Task<IActionResult> GetComplianceViolations(ComplianceFramework framework, [FromQuery] DateTime? fromDate = null)
    {
        try
        {
            var violations = await _securityMonitoringService.GetComplianceViolationsAsync(framework, fromDate);
            return Ok(violations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance violations for framework {Framework}", framework);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("compliance/validate-access")]
    public async Task<IActionResult> ValidateDataAccess([FromBody] DataAccessValidationRequest request)
    {
        try
        {
            var isCompliant = await _securityMonitoringService.IsDataAccessCompliantAsync(
                request.UserId, request.DataType, request.Operation);
            
            return Ok(new { IsCompliant = isCompliant });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data access for user {UserId}", request.UserId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("compliance/{framework}/report")]
    public async Task<IActionResult> GenerateComplianceReport(
        ComplianceFramework framework, 
        [FromQuery] DateTime fromDate, 
        [FromQuery] DateTime toDate)
    {
        try
        {
            var report = await _securityMonitoringService.GenerateComplianceReportAsync(framework, fromDate, toDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating compliance report for framework {Framework}", framework);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Security Metrics

    [HttpGet("metrics")]
    public async Task<IActionResult> GetSecurityMetrics([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var metrics = await _securityMonitoringService.GetSecurityMetricsAsync(fromDate, toDate);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security metrics");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("incidents")]
    public async Task<IActionResult> GetSecurityIncidents([FromQuery] DateTime? fromDate = null, [FromQuery] int? limit = null)
    {
        try
        {
            var incidents = await _securityMonitoringService.GetSecurityIncidentsAsync(fromDate, limit);
            return Ok(incidents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security incidents");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("incidents/{id}")]
    public async Task<IActionResult> GetSecurityIncident(int id)
    {
        try
        {
            var incidents = await _securityMonitoringService.GetSecurityIncidentsAsync();
            var incident = incidents.FirstOrDefault(i => i.Id == id);
            
            if (incident == null)
                return NotFound();

            return Ok(incident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security incident {IncidentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetSecurityDashboard()
    {
        try
        {
            var dashboard = await _securityMonitoringService.GetSecurityDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security dashboard");
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Real-time Monitoring

    [HttpPost("monitoring/start")]
    public async Task<IActionResult> StartMonitoring()
    {
        try
        {
            await _securityMonitoringService.StartRealTimeMonitoringAsync();
            return Ok(new { Message = "Real-time monitoring started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting real-time monitoring");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("monitoring/stop")]
    public async Task<IActionResult> StopMonitoring()
    {
        try
        {
            await _securityMonitoringService.StopRealTimeMonitoringAsync();
            return Ok(new { Message = "Real-time monitoring stopped" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping real-time monitoring");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("monitoring/status")]
    public async Task<IActionResult> GetMonitoringStatus()
    {
        try
        {
            var isActive = await _securityMonitoringService.IsMonitoringActiveAsync();
            return Ok(new { IsActive = isActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monitoring status");
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Alerting

    [HttpPost("alerts")]
    public async Task<IActionResult> CreateAlert([FromBody] SecurityAlert alert)
    {
        try
        {
            await _securityMonitoringService.SendSecurityAlertAsync(alert);
            return Ok(new { Message = "Alert created successfully", AlertId = alert.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security alert");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("alerts/active")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        try
        {
            var alerts = await _securityMonitoringService.GetActiveAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active alerts");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("alerts/{id}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(int id, [FromBody] AcknowledgeAlertRequest request)
    {
        try
        {
            await _securityMonitoringService.AcknowledgeAlertAsync(id, request.AcknowledgedBy);
            return Ok(new { Message = "Alert acknowledged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Data Protection

    [HttpGet("encryption/status")]
    public async Task<IActionResult> GetEncryptionStatus()
    {
        try
        {
            var status = await _securityMonitoringService.GetEncryptionStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving encryption status");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("data/{dataId}/encrypted")]
    public async Task<IActionResult> IsDataEncrypted(string dataId)
    {
        try
        {
            var isEncrypted = await _securityMonitoringService.IsDataEncryptedAsync(dataId);
            return Ok(new { IsEncrypted = isEncrypted });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking data encryption for {DataId}", dataId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("data/{dataId}/integrity")]
    public async Task<IActionResult> ValidateDataIntegrity(string dataId)
    {
        try
        {
            var isValid = await _securityMonitoringService.ValidateDataIntegrityAsync(dataId);
            return Ok(new { IsValid = isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data integrity for {DataId}", dataId);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Audit and Forensics

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetSecurityAuditLogs(
        [FromQuery] DateTime fromDate, 
        [FromQuery] DateTime toDate, 
        [FromQuery] string? userId = null)
    {
        try
        {
            var logs = await _securityMonitoringService.GetSecurityAuditLogsAsync(fromDate, toDate, userId);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security audit logs");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("forensics/{incidentId}")]
    public async Task<IActionResult> PerformForensicAnalysis(string incidentId)
    {
        try
        {
            var analysis = await _securityMonitoringService.PerformForensicAnalysisAsync(incidentId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing forensic analysis for incident {IncidentId}", incidentId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetSecurityEvents(
        [FromQuery] DateTime fromDate, 
        [FromQuery] DateTime toDate, 
        [FromQuery] SecurityEventType? eventType = null)
    {
        try
        {
            var events = await _securityMonitoringService.GetSecurityEventsAsync(fromDate, toDate, eventType);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security events");
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion
}

// Request/Response DTOs
public class ResolveThreatRequest
{
    public string ResolvedBy { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class DataAccessValidationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
}

public class AcknowledgeAlertRequest
{
    public string AcknowledgedBy { get; set; } = string.Empty;
}
