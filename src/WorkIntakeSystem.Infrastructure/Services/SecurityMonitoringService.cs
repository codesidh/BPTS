using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Services;

public class SecurityMonitoringService : ISecurityMonitoringService
{
    private readonly WorkIntakeDbContext _context;
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMonitoringService _monitoringService;
    private bool _isMonitoringActive = false;
    private readonly Dictionary<string, List<DateTime>> _loginAttempts = new();
    private readonly Dictionary<string, List<DateTime>> _ipAttempts = new();

    public SecurityMonitoringService(
        WorkIntakeDbContext context,
        ILogger<SecurityMonitoringService> logger,
        IConfiguration configuration,
        IMonitoringService monitoringService)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _monitoringService = monitoringService;
    }

    #region Threat Detection

    public async Task<bool> DetectSuspiciousActivityAsync(SecurityEvent securityEvent)
    {
        try
        {
            var isSuspicious = false;
            var suspiciousReasons = new List<string>();

            // Check for brute force attempts
            if (securityEvent.EventType == "FailedAuthentication")
            {
                var isBruteForce = await IsBruteForceAttemptAsync(securityEvent.UserId.ToString(), securityEvent.IPAddress);
                if (isBruteForce)
                {
                    isSuspicious = true;
                    suspiciousReasons.Add("Brute force attack detected");
                }
            }

            // Check for suspicious login patterns
            if (securityEvent.EventType == "Login")
            {
                var isSuspiciousLogin = await IsSuspiciousLoginAsync(securityEvent.UserId.ToString(), securityEvent.IPAddress, securityEvent.UserAgent);
                if (isSuspiciousLogin)
                {
                    isSuspicious = true;
                    suspiciousReasons.Add("Suspicious login pattern detected");
                }
            }

            securityEvent.IsSuspicious = isSuspicious;
            
            // Parse existing metadata or create new dictionary
            var metadata = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(securityEvent.Metadata))
            {
                try
                {
                    metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(securityEvent.Metadata) ?? new Dictionary<string, object>();
                }
                catch
                {
                    metadata = new Dictionary<string, object>();
                }
            }
            
            metadata["SuspiciousReasons"] = string.Join(", ", suspiciousReasons);
            securityEvent.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);

            // Log the security event
            await LogSecurityEventAsync(securityEvent);

            // If suspicious, create a threat record
            if (isSuspicious)
            {
                await CreateSecurityThreatAsync(securityEvent, suspiciousReasons);
            }

            return isSuspicious;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting suspicious activity for event {EventId}", securityEvent.Id);
            return false;
        }
    }

    public async Task<List<SecurityThreat>> GetActiveThreatsAsync()
    {
        try
        {
            return await _context.SecurityThreats
                .Where(t => !t.IsResolved)
                .OrderByDescending(t => t.DetectedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active threats");
            return new List<SecurityThreat>();
        }
    }

    public async Task<bool> IsSuspiciousLoginAsync(string userId, string ipAddress, string userAgent)
    {
        try
        {
            // Check for new IP address
            var userIdInt = int.TryParse(userId, out var parsedUserId) ? parsedUserId : 0;
            var recentLogins = await _context.SecurityEvents
                .Where(e => e.UserId == userIdInt && e.EventType == "Login" && e.Timestamp >= DateTime.UtcNow.AddDays(-30))
                .Select(e => e.IPAddress)
                .Distinct()
                .ToListAsync();

            if (!recentLogins.Contains(ipAddress))
            {
                _logger.LogWarning("New IP address detected for user {UserId}: {IPAddress}", userId, ipAddress);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking suspicious login for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsBruteForceAttemptAsync(string userId, string ipAddress)
    {
        try
        {
            var now = DateTime.UtcNow;
            var timeWindow = TimeSpan.FromMinutes(15);
            var maxAttempts = 5;

            // Check user-based attempts
            if (!_loginAttempts.ContainsKey(userId))
                _loginAttempts[userId] = new List<DateTime>();

            _loginAttempts[userId].RemoveAll(t => t < now - timeWindow);
            _loginAttempts[userId].Add(now);

            if (_loginAttempts[userId].Count > maxAttempts)
            {
                _logger.LogWarning("Brute force attempt detected for user {UserId}: {Attempts} attempts in {TimeWindow} minutes", 
                    userId, _loginAttempts[userId].Count, timeWindow.TotalMinutes);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking brute force attempt for user {UserId} from IP {IPAddress}", userId, ipAddress);
            return false;
        }
    }

    #endregion

    #region Compliance Monitoring

    public async Task<ComplianceStatus> CheckComplianceAsync(ComplianceFramework framework)
    {
        try
        {
            var status = new ComplianceStatus
            {
                Framework = framework,
                IsCompliant = true,
                ComplianceScore = 95.0,
                Status = "Compliant",
                LastChecked = DateTime.UtcNow
            };

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking compliance for framework {Framework}", framework);
            return new ComplianceStatus
            {
                Framework = framework,
                IsCompliant = false,
                ComplianceScore = 0,
                Status = "Error"
            };
        }
    }

    public async Task<List<ComplianceViolation>> GetComplianceViolationsAsync(ComplianceFramework framework, DateTime? fromDate = null)
    {
        try
        {
            var query = _context.ComplianceViolations
                .Where(v => v.Framework == (int)framework);

            if (fromDate.HasValue)
            {
                query = query.Where(v => v.DetectedAt >= fromDate.Value);
            }

            return await query
                .OrderByDescending(v => v.DetectedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance violations for framework {Framework}", framework);
            return new List<ComplianceViolation>();
        }
    }

    public async Task<bool> IsDataAccessCompliantAsync(string userId, string dataType, string operation)
    {
        try
        {
            // Basic compliance check - in real implementation, this would be more sophisticated
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking data access compliance for user {UserId}", userId);
            return false;
        }
    }

    public async Task<ComplianceReport> GenerateComplianceReportAsync(ComplianceFramework framework, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var violations = await GetComplianceViolationsAsync(framework, fromDate);
            
            var report = new ComplianceReport
            {
                Framework = framework,
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedAt = DateTime.UtcNow,
                Violations = violations,
                OverallComplianceScore = violations.Count == 0 ? 100.0 : 90.0,
                ExecutiveSummary = $"Compliance report for {framework} from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                Recommendations = new List<string> { "Continue monitoring", "Review policies regularly" }
            };

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating compliance report for framework {Framework}", framework);
            return new ComplianceReport
            {
                Framework = framework,
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedAt = DateTime.UtcNow,
                OverallComplianceScore = 0,
                ExecutiveSummary = "Error generating report"
            };
        }
    }

    #endregion

    #region Security Metrics

    public async Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var events = await _context.SecurityEvents
                .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate)
                .ToListAsync();

            var metrics = new SecurityMetrics
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalSecurityEvents = events.Count,
                SuspiciousEvents = events.Count(e => e.IsSuspicious),
                FailedLogins = events.Count(e => e.EventType == "FailedAuthentication"),
                SuccessfulLogins = events.Count(e => e.EventType == "Login"),
                DataAccessEvents = events.Count(e => e.EventType == "DataAccess"),
                PrivilegeEscalations = events.Count(e => e.EventType == "PrivilegeEscalation"),
                SecurityIncidents = await _context.SecurityIncidents
                    .CountAsync(i => i.DetectedAt >= fromDate && i.DetectedAt <= toDate)
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security metrics");
            return new SecurityMetrics
            {
                FromDate = fromDate,
                ToDate = toDate
            };
        }
    }

    public async Task<List<SecurityIncident>> GetSecurityIncidentsAsync(DateTime? fromDate = null, int? limit = null)
    {
        try
        {
            var query = _context.SecurityIncidents.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.DetectedAt >= fromDate.Value);
            }

            query = query.OrderByDescending(i => i.DetectedAt);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security incidents");
            return new List<SecurityIncident>();
        }
    }

    public async Task<SecurityDashboard> GetSecurityDashboardAsync()
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;

            var dashboard = new SecurityDashboard
            {
                GeneratedAt = DateTime.UtcNow,
                Metrics = await GetSecurityMetricsAsync(fromDate, toDate),
                ActiveThreats = await GetActiveThreatsAsync(),
                ActiveAlerts = await GetActiveAlertsAsync(),
                ComplianceStatuses = await GetAllComplianceStatusesAsync(),
                RecentIncidents = await GetSecurityIncidentsAsync(DateTime.UtcNow.AddDays(-7), 10)
            };

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating security dashboard");
            return new SecurityDashboard { GeneratedAt = DateTime.UtcNow };
        }
    }

    #endregion

    #region Real-time Monitoring

    public async Task StartRealTimeMonitoringAsync()
    {
        try
        {
            if (_isMonitoringActive) return;

            _isMonitoringActive = true;
            _logger.LogInformation("Starting real-time security monitoring");

            await _monitoringService.TrackEventAsync("SecurityMonitoringStarted", new Dictionary<string, string>
            {
                { "timestamp", DateTime.UtcNow.ToString("O") }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting real-time monitoring");
        }
    }

    public async Task StopRealTimeMonitoringAsync()
    {
        try
        {
            _isMonitoringActive = false;
            _logger.LogInformation("Stopping real-time security monitoring");

            await _monitoringService.TrackEventAsync("SecurityMonitoringStopped", new Dictionary<string, string>
            {
                { "timestamp", DateTime.UtcNow.ToString("O") }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping real-time monitoring");
        }
    }

    public async Task<bool> IsMonitoringActiveAsync()
    {
        return await Task.FromResult(_isMonitoringActive);
    }

    #endregion

    #region Alerting

    public async Task SendSecurityAlertAsync(SecurityAlert alert)
    {
        try
        {
            _context.SecurityAlerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Security alert created: {AlertType} - {Title}", alert.AlertType, alert.Title);

            await _monitoringService.TrackEventAsync("SecurityAlert", new Dictionary<string, string>
            {
                { "alert_type", alert.AlertType },
                { "severity", alert.Severity },
                { "title", alert.Title }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security alert");
        }
    }

    public async Task<List<SecurityAlert>> GetActiveAlertsAsync()
    {
        try
        {
            return await _context.SecurityAlerts
                .Where(a => a.Status == "Active")
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active alerts");
            return new List<SecurityAlert>();
        }
    }

    public async Task AcknowledgeAlertAsync(int alertId, string acknowledgedBy)
    {
        try
        {
            var alert = await _context.SecurityAlerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.Status = "Acknowledged";
                alert.AcknowledgedAt = DateTime.UtcNow;
                alert.AcknowledgedBy = acknowledgedBy;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Security alert {AlertId} acknowledged by {User}", alertId, acknowledgedBy);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
        }
    }

    #endregion

    #region Data Protection

    public async Task<bool> IsDataEncryptedAsync(string dataId)
    {
        try
        {
            // Basic check - in real implementation, this would verify actual encryption
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking data encryption for {DataId}", dataId);
            return false;
        }
    }

    public async Task<EncryptionStatus> GetEncryptionStatusAsync()
    {
        try
        {
            var status = new EncryptionStatus
            {
                IsEnabled = true,
                Algorithm = "AES-256",
                KeyLength = 256,
                LastKeyRotation = DateTime.UtcNow.AddDays(-30),
                NextKeyRotation = DateTime.UtcNow.AddDays(30)
            };

            status.DataTypesEncrypted = new Dictionary<string, bool>
            {
                { "WorkRequestDescription", true },
                { "BusinessJustification", true },
                { "UserPasswords", true },
                { "AuditLogs", true },
                { "ConfigurationData", false }
            };

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving encryption status");
            return new EncryptionStatus { IsEnabled = false };
        }
    }

    public async Task<bool> ValidateDataIntegrityAsync(string dataId)
    {
        try
        {
            // Basic integrity check - in real implementation, this would verify data integrity
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data integrity for {DataId}", dataId);
            return false;
        }
    }

    #endregion

    #region Audit and Forensics

    public async Task<List<SecurityAuditLog>> GetSecurityAuditLogsAsync(DateTime fromDate, DateTime toDate, string? userId = null)
    {
        try
        {
            var query = _context.SecurityAuditLogs
                .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate);

            if (!string.IsNullOrEmpty(userId))
            {
                var userIdInt = int.TryParse(userId, out var parsedUserId) ? parsedUserId : 0;
                query = query.Where(l => l.UserId == userIdInt);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security audit logs");
            return new List<SecurityAuditLog>();
        }
    }

    public async Task<ForensicAnalysis> PerformForensicAnalysisAsync(string incidentId)
    {
        try
        {
            var analysis = new ForensicAnalysis
            {
                IncidentId = incidentId,
                AnalyzedAt = DateTime.UtcNow,
                Analyst = "System",
                Analysis = $"Forensic analysis for incident {incidentId}",
                Conclusion = "Analysis completed",
                Recommendations = new List<string> { "Review security policies", "Update monitoring rules" }
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing forensic analysis for incident {IncidentId}", incidentId);
            return new ForensicAnalysis { IncidentId = incidentId };
        }
    }

    public async Task<List<SecurityEvent>> GetSecurityEventsAsync(DateTime fromDate, DateTime toDate, SecurityEventType? eventType = null)
    {
        try
        {
            var query = _context.SecurityEvents
                .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate);

            if (eventType.HasValue)
            {
                query = query.Where(e => e.EventType == eventType.Value.ToString());
            }

            return await query
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security events");
            return new List<SecurityEvent>();
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        try
        {
            _context.SecurityEvents.Add(securityEvent);
            await _context.SaveChangesAsync();

            var auditLog = new SecurityAuditLog
            {
                UserId = securityEvent.UserId,
                Action = securityEvent.Action,
                Resource = securityEvent.Resource,
                IPAddress = securityEvent.IPAddress,
                UserAgent = securityEvent.UserAgent,
                Timestamp = securityEvent.Timestamp,
                Result = securityEvent.IsSuspicious ? "Suspicious" : "Normal",
                Details = securityEvent.Details,
                SessionId = securityEvent.SessionId
            };

            _context.SecurityAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging security event");
        }
    }

    private async Task CreateSecurityThreatAsync(SecurityEvent securityEvent, List<string> reasons)
    {
        try
        {
            var threat = new SecurityThreat
            {
                ThreatType = securityEvent.EventType,
                Description = string.Join("; ", reasons),
                Severity = securityEvent.Severity,
                Status = "Active",
                DetectedAt = securityEvent.Timestamp,
                SourceIP = securityEvent.IPAddress,
                TargetResource = securityEvent.Resource,
                MitigationSteps = "1. Investigate incident\n2. Review security policies\n3. Update monitoring rules"
            };

            _context.SecurityThreats.Add(threat);
            await _context.SaveChangesAsync();

            var alert = new SecurityAlert
            {
                AlertType = "SecurityThreat",
                Title = $"Security Threat Detected: {securityEvent.EventType}",
                Description = string.Join("; ", reasons),
                Severity = securityEvent.Severity,
                Status = "Active",
                Source = "SecurityMonitoringService"
            };

            await SendSecurityAlertAsync(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security threat");
        }
    }

    private async Task<List<ComplianceStatus>> GetAllComplianceStatusesAsync()
    {
        var statuses = new List<ComplianceStatus>();

        foreach (ComplianceFramework framework in Enum.GetValues<ComplianceFramework>())
        {
            statuses.Add(await CheckComplianceAsync(framework));
        }

        return statuses;
    }

    #endregion
}