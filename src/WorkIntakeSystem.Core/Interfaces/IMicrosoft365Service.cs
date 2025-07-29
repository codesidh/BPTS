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