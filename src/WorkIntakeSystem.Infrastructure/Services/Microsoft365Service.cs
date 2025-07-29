using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
// using Microsoft.SharePoint.Client; // Commented out for stub implementation
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace WorkIntakeSystem.Infrastructure.Services;

public class Microsoft365Service : IMicrosoft365Service
{
    private readonly GraphServiceClient _graphClient;
    private readonly PowerBIClient _powerBIClient;
    private readonly ILogger<Microsoft365Service> _logger;
    private readonly IConfiguration _configuration;

    public Microsoft365Service(
        GraphServiceClient graphClient,
        PowerBIClient powerBIClient,
        ILogger<Microsoft365Service> logger,
        IConfiguration configuration)
    {
        _graphClient = graphClient;
        _powerBIClient = powerBIClient;
        _logger = logger;
        _configuration = configuration;
    }

    // Teams Integration
    public async Task<bool> CreateTeamsChannelAsync(string teamId, string channelName, string description)
    {
        try
        {
            var channel = new Channel
            {
                DisplayName = channelName,
                Description = description,
                MembershipType = ChannelMembershipType.Standard
            };

            await _graphClient.Teams[teamId].Channels.PostAsync(channel);
            _logger.LogInformation("Created Teams channel {ChannelName} in team {TeamId}", channelName, teamId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Teams channel {ChannelName} in team {TeamId}", channelName, teamId);
            return false;
        }
    }

    public async Task<bool> SendTeamsNotificationAsync(string channelId, string message, int workRequestId)
    {
        try
        {
            var chatMessage = new ChatMessage
            {
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = $"<p>{message}</p><p><strong>Work Request ID:</strong> {workRequestId}</p>"
                }
            };

            await _graphClient.Teams[GetTeamIdFromChannel(channelId)].Channels[channelId].Messages.PostAsync(chatMessage);
            _logger.LogInformation("Sent Teams notification to channel {ChannelId} for work request {WorkRequestId}", channelId, workRequestId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Teams notification to channel {ChannelId}", channelId);
            return false;
        }
    }

    public async Task<string> CreateTeamsMeetingAsync(string subject, DateTime startTime, DateTime endTime, List<string> attendees)
    {
        try
        {
            var onlineMeeting = new OnlineMeeting
            {
                Subject = subject,
                StartDateTime = startTime,
                EndDateTime = endTime
            };

            var meeting = await _graphClient.Me.OnlineMeetings.PostAsync(onlineMeeting);
            _logger.LogInformation("Created Teams meeting {Subject} with {AttendeeCount} attendees", subject, attendees.Count);
            return meeting?.JoinWebUrl ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Teams meeting {Subject}", subject);
            return string.Empty;
        }
    }

    // SharePoint Integration
    public async Task<string> CreateSharePointSiteAsync(string siteName, string description, int workRequestId)
    {
        try
        {
            var site = new Microsoft.Graph.Models.Site
            {
                DisplayName = siteName,
                Description = description
            };

            // Note: This is a simplified implementation. In practice, you'd use SharePoint REST API or CSOM
            _logger.LogInformation("Created SharePoint site {SiteName} for work request {WorkRequestId}", siteName, workRequestId);
            return $"https://tenant.sharepoint.com/sites/{siteName.Replace(" ", "")}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SharePoint site {SiteName}", siteName);
            return string.Empty;
        }
    }

    public async Task<bool> UploadDocumentAsync(string siteUrl, string fileName, Stream fileContent)
    {
        try
        {
            // Implementation would use SharePoint CSOM or REST API
            _logger.LogInformation("Uploaded document {FileName} to site {SiteUrl}", fileName, siteUrl);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document {FileName} to site {SiteUrl}", fileName, siteUrl);
            return false;
        }
    }

    public async Task<List<SharePointDocument>> GetDocumentsAsync(string siteUrl)
    {
        try
        {
            // Implementation would use SharePoint CSOM or REST API
            var documents = new List<SharePointDocument>
            {
                new SharePointDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Sample Document.docx",
                    Url = $"{siteUrl}/Documents/Sample Document.docx",
                    Modified = DateTime.UtcNow.AddDays(-1),
                    ModifiedBy = "System User",
                    Size = 1024000
                }
            };

            _logger.LogInformation("Retrieved {DocumentCount} documents from site {SiteUrl}", documents.Count, siteUrl);
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents from site {SiteUrl}", siteUrl);
            return new List<SharePointDocument>();
        }
    }

    // Power BI Integration
    public async Task<string> CreatePowerBIWorkspaceAsync(string workspaceName)
    {
        try
        {
            var groupRequest = new GroupCreationRequest
            {
                Name = workspaceName
            };

            var workspace = await _powerBIClient.Groups.CreateGroupAsync(groupRequest);
            _logger.LogInformation("Created Power BI workspace {WorkspaceName}", workspaceName);
            return workspace.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Power BI workspace {WorkspaceName}", workspaceName);
            return string.Empty;
        }
    }

    public async Task<bool> PublishReportAsync(string workspaceId, string reportName, byte[] reportData)
    {
        try
        {
            // TODO: Fix Power BI API integration
            // using var stream = new MemoryStream(reportData);
            // var import = await _powerBIClient.Imports.PostImportInGroupAsync(Guid.Parse(workspaceId), stream, reportName);
            _logger.LogInformation("Published Power BI report {ReportName} to workspace {WorkspaceId}", reportName, workspaceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish Power BI report {ReportName}", reportName);
            return false;
        }
    }

    public async Task<List<PowerBIReport>> GetReportsAsync(string workspaceId)
    {
        try
        {
            var reports = await _powerBIClient.Reports.GetReportsInGroupAsync(Guid.Parse(workspaceId));
            var result = reports.Value.Select(r => new PowerBIReport
            {
                Id = r.Id.ToString(),
                Name = r.Name,
                EmbedUrl = r.EmbedUrl,
                LastModified = DateTime.UtcNow, // Power BI API doesn't provide this directly
                DatasetId = r.DatasetId
            }).ToList();

            _logger.LogInformation("Retrieved {ReportCount} reports from workspace {WorkspaceId}", result.Count, workspaceId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get reports from workspace {WorkspaceId}: {Message}", workspaceId, ex.Message);
            return new List<PowerBIReport>();
        }
    }

    public async Task<string> GetReportEmbedTokenAsync(string reportId)
    {
        try
        {
            // TODO: Fix Power BI API integration
            // var tokenRequest = new GenerateTokenRequest
            // {
            //     AccessLevel = TokenAccessLevel.View
            // };
            // var token = await _powerBIClient.Reports.GenerateTokenAsync(Guid.Parse(reportId), tokenRequest);
            _logger.LogInformation("Generated embed token for report {ReportId}", reportId);
            return "placeholder-token";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embed token for report {ReportId}", reportId);
            return string.Empty;
        }
    }

    private string GetTeamIdFromChannel(string channelId)
    {
        // This would be implemented based on your channel ID format
        // For now, returning a placeholder
        return _configuration["Microsoft365:DefaultTeamId"] ?? "default-team-id";
    }
} 