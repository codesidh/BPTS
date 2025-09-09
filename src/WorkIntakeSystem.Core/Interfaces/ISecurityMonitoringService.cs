using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface ISecurityMonitoringService
{
    // Threat Detection
    Task<bool> DetectSuspiciousActivityAsync(SecurityEvent securityEvent);
    Task<List<SecurityThreat>> GetActiveThreatsAsync();
    Task<bool> IsSuspiciousLoginAsync(string userId, string ipAddress, string userAgent);
    Task<bool> IsBruteForceAttemptAsync(string userId, string ipAddress);
    
    // Compliance Monitoring
    Task<ComplianceStatus> CheckComplianceAsync(ComplianceFramework framework);
    Task<List<ComplianceViolation>> GetComplianceViolationsAsync(ComplianceFramework framework, DateTime? fromDate = null);
    Task<bool> IsDataAccessCompliantAsync(string userId, string dataType, string operation);
    Task<ComplianceReport> GenerateComplianceReportAsync(ComplianceFramework framework, DateTime fromDate, DateTime toDate);
    
    // Security Metrics
    Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime fromDate, DateTime toDate);
    Task<List<SecurityIncident>> GetSecurityIncidentsAsync(DateTime? fromDate = null, int? limit = null);
    Task<SecurityDashboard> GetSecurityDashboardAsync();
    
    // Real-time Monitoring
    Task StartRealTimeMonitoringAsync();
    Task StopRealTimeMonitoringAsync();
    Task<bool> IsMonitoringActiveAsync();
    
    // Alerting
    Task SendSecurityAlertAsync(SecurityAlert alert);
    Task<List<SecurityAlert>> GetActiveAlertsAsync();
    Task AcknowledgeAlertAsync(int alertId, string acknowledgedBy);
    
    // Data Protection
    Task<bool> IsDataEncryptedAsync(string dataId);
    Task<EncryptionStatus> GetEncryptionStatusAsync();
    Task<bool> ValidateDataIntegrityAsync(string dataId);
    
    // Audit and Forensics
    Task<List<SecurityAuditLog>> GetSecurityAuditLogsAsync(DateTime fromDate, DateTime toDate, string? userId = null);
    Task<ForensicAnalysis> PerformForensicAnalysisAsync(string incidentId);
    Task<List<SecurityEvent>> GetSecurityEventsAsync(DateTime fromDate, DateTime toDate, SecurityEventType? eventType = null);
}

public enum ComplianceFramework
{
    HIPAA,
    SOX,
    PCI_DSS,
    GDPR,
    ISO27001,
    NIST
}

public enum SecurityEventType
{
    Login,
    Logout,
    DataAccess,
    DataModification,
    PermissionChange,
    SystemAccess,
    FailedAuthentication,
    PrivilegeEscalation,
    DataExport,
    ConfigurationChange,
    SecurityPolicyViolation,
    AnomalousBehavior
}

// DTOs for security monitoring
public class SecurityMetrics
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalSecurityEvents { get; set; }
    public int SuspiciousEvents { get; set; }
    public int FailedLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int DataAccessEvents { get; set; }
    public int PrivilegeEscalations { get; set; }
    public int SecurityIncidents { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public Dictionary<string, int> EventsBySeverity { get; set; } = new();
    public List<TopThreat> TopThreats { get; set; } = new();
}

public class TopThreat
{
    public string ThreatType { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime LastOccurrence { get; set; }
}

public class SecurityDashboard
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public SecurityMetrics Metrics { get; set; } = new();
    public List<SecurityThreat> ActiveThreats { get; set; } = new();
    public List<SecurityAlert> ActiveAlerts { get; set; } = new();
    public List<ComplianceStatus> ComplianceStatuses { get; set; } = new();
    public List<SecurityIncident> RecentIncidents { get; set; } = new();
    public Dictionary<string, object> SystemHealth { get; set; } = new();
}

public class ComplianceStatus
{
    public ComplianceFramework Framework { get; set; }
    public bool IsCompliant { get; set; }
    public double ComplianceScore { get; set; }
    public List<ComplianceViolation> Violations { get; set; } = new();
    public List<ComplianceRequirement> Requirements { get; set; } = new();
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
}

public class ComplianceRequirement
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsImplemented { get; set; }
    public string ImplementationStatus { get; set; } = string.Empty;
    public DateTime LastVerified { get; set; } = DateTime.UtcNow;
}

public class ComplianceReport
{
    public ComplianceFramework Framework { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public double OverallComplianceScore { get; set; }
    public List<ComplianceViolation> Violations { get; set; } = new();
    public List<ComplianceRequirement> Requirements { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}

public class EncryptionStatus
{
    public bool IsEnabled { get; set; }
    public string Algorithm { get; set; } = string.Empty;
    public int KeyLength { get; set; }
    public DateTime LastKeyRotation { get; set; }
    public DateTime NextKeyRotation { get; set; }
    public Dictionary<string, bool> DataTypesEncrypted { get; set; } = new();
    public List<string> EncryptionAlerts { get; set; } = new();
}

public class ForensicAnalysis
{
    public string IncidentId { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public string Analyst { get; set; } = string.Empty;
    public List<SecurityEvent> Timeline { get; set; } = new();
    public List<string> Indicators { get; set; } = new();
    public string Analysis { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public string Conclusion { get; set; } = string.Empty;
}