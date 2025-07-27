using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IMicrosoft365Service
{
    // Teams Integration
    Task<bool> CreateTeamsChannelAsync(string teamId, string channelName, string description);
    Task<bool> SendTeamsNotificationAsync(string channelId, string message, int workRequestId);
    Task<string> CreateTeamsMeetingAsync(string subject, DateTime startTime, DateTime endTime, List<string> attendees);
    
    // SharePoint Integration
    Task<string> CreateSharePointSiteAsync(string siteName, string description, int workRequestId);
    Task<bool> UploadDocumentAsync(string siteUrl, string fileName, Stream fileContent);
    Task<List<SharePointDocument>> GetDocumentsAsync(string siteUrl);
    
    // Power BI Integration
    Task<string> CreatePowerBIWorkspaceAsync(string workspaceName);
    Task<bool> PublishReportAsync(string workspaceId, string reportName, byte[] reportData);
    Task<List<PowerBIReport>> GetReportsAsync(string workspaceId);
    Task<string> GetReportEmbedTokenAsync(string reportId);
}

public class SharePointDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public long Size { get; set; }
}

public class PowerBIReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string DatasetId { get; set; } = string.Empty;
} 