using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class Microsoft365Controller : ControllerBase
{
    private readonly IMicrosoft365Service _microsoft365Service;
    private readonly ILogger<Microsoft365Controller> _logger;

    public Microsoft365Controller(
        IMicrosoft365Service microsoft365Service,
        ILogger<Microsoft365Controller> logger)
    {
        _microsoft365Service = microsoft365Service;
        _logger = logger;
    }

    // Teams Integration
    [HttpPost("teams/channels")]
    public async Task<IActionResult> CreateTeamsChannel([FromBody] CreateTeamsChannelRequest request)
    {
        try
        {
            var result = await _microsoft365Service.CreateTeamsChannelAsync(
                request.TeamId, request.ChannelName, request.Description);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Teams channel created successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to create Teams channel" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Teams channel");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("teams/notifications")]
    public async Task<IActionResult> SendTeamsNotification([FromBody] SendTeamsNotificationRequest request)
    {
        try
        {
            var result = await _microsoft365Service.SendTeamsNotificationAsync(
                request.ChannelId, request.Message, request.WorkRequestId);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Teams notification sent successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to send Teams notification" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Teams notification");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("teams/meetings")]
    public async Task<IActionResult> CreateTeamsMeeting([FromBody] CreateTeamsMeetingRequest request)
    {
        try
        {
            var meetingUrl = await _microsoft365Service.CreateTeamsMeetingAsync(
                request.Subject, request.StartTime, request.EndTime, request.Attendees);
            
            if (!string.IsNullOrEmpty(meetingUrl))
            {
                return Ok(new { Success = true, MeetingUrl = meetingUrl });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to create Teams meeting" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Teams meeting");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // SharePoint Integration
    [HttpPost("sharepoint/sites")]
    public async Task<IActionResult> CreateSharePointSite([FromBody] CreateSharePointSiteRequest request)
    {
        try
        {
            var siteUrl = await _microsoft365Service.CreateSharePointSiteAsync(
                request.SiteName, request.Description, request.WorkRequestId);
            
            if (!string.IsNullOrEmpty(siteUrl))
            {
                return Ok(new { Success = true, SiteUrl = siteUrl });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to create SharePoint site" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SharePoint site");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("sharepoint/documents")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "No file provided" });
            }

            using var stream = request.File.OpenReadStream();
            var result = await _microsoft365Service.UploadDocumentAsync(
                request.SiteUrl, request.File.FileName, stream);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Document uploaded successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to upload document" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("sharepoint/documents")]
    public async Task<IActionResult> GetDocuments([FromQuery] string siteUrl)
    {
        try
        {
            var documents = await _microsoft365Service.GetDocumentsAsync(siteUrl);
            return Ok(new { Success = true, Documents = documents });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SharePoint documents");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Power BI Integration
    [HttpPost("powerbi/workspaces")]
    public async Task<IActionResult> CreatePowerBIWorkspace([FromBody] CreatePowerBIWorkspaceRequest request)
    {
        try
        {
            var workspaceId = await _microsoft365Service.CreatePowerBIWorkspaceAsync(request.WorkspaceName);
            
            if (!string.IsNullOrEmpty(workspaceId))
            {
                return Ok(new { Success = true, WorkspaceId = workspaceId });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to create Power BI workspace" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Power BI workspace");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("powerbi/workspaces/{workspaceId}/reports")]
    public async Task<IActionResult> GetPowerBIReports(string workspaceId)
    {
        try
        {
            var reports = await _microsoft365Service.GetReportsAsync(workspaceId);
            return Ok(new { Success = true, Reports = reports });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Power BI reports");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("powerbi/reports/{reportId}/embed-token")]
    public async Task<IActionResult> GetReportEmbedToken(string reportId)
    {
        try
        {
            var token = await _microsoft365Service.GetReportEmbedTokenAsync(reportId);
            
            if (!string.IsNullOrEmpty(token))
            {
                return Ok(new { Success = true, Token = token });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to generate embed token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Power BI embed token");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }
}

// Request DTOs
public class CreateTeamsChannelRequest
{
    public string TeamId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SendTeamsNotificationRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
}

public class CreateTeamsMeetingRequest
{
    public string Subject { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<string> Attendees { get; set; } = new();
}

public class CreateSharePointSiteRequest
{
    public string SiteName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
}

public class UploadDocumentRequest
{
    public string SiteUrl { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}

public class CreatePowerBIWorkspaceRequest
{
    public string WorkspaceName { get; set; } = string.Empty;
} 