using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevOpsIntegrationController : ControllerBase
{
    private readonly IDevOpsIntegrationService _devOpsIntegrationService;
    private readonly ILogger<DevOpsIntegrationController> _logger;

    public DevOpsIntegrationController(
        IDevOpsIntegrationService devOpsIntegrationService,
        ILogger<DevOpsIntegrationController> logger)
    {
        _devOpsIntegrationService = devOpsIntegrationService;
        _logger = logger;
    }

    // Azure DevOps Integration
    [HttpPost("azure-devops/work-items")]
    public async Task<IActionResult> CreateAzureDevOpsWorkItem([FromBody] CreateAzureDevOpsWorkItemRequest request)
    {
        try
        {
            var workItemId = await _devOpsIntegrationService.CreateAzureDevOpsWorkItemAsync(
                request.Project, request.WorkItemType, request.Title, request.Description, request.WorkRequestId);
            
            if (!string.IsNullOrEmpty(workItemId))
            {
                return Ok(new { Success = true, WorkItemId = workItemId });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to create Azure DevOps work item" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Azure DevOps work item");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPut("azure-devops/work-items/{workItemId}")]
    public async Task<IActionResult> UpdateAzureDevOpsWorkItem(string workItemId, [FromBody] Dictionary<string, object> fields)
    {
        try
        {
            var result = await _devOpsIntegrationService.UpdateAzureDevOpsWorkItemAsync(workItemId, fields);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Azure DevOps work item updated successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to update Azure DevOps work item" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Azure DevOps work item");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("azure-devops/work-items/{workItemId}")]
    public async Task<IActionResult> GetAzureDevOpsWorkItem(string workItemId)
    {
        try
        {
            var workItem = await _devOpsIntegrationService.GetAzureDevOpsWorkItemAsync(workItemId);
            
            if (!string.IsNullOrEmpty(workItem.Id))
            {
                return Ok(new { Success = true, WorkItem = workItem });
            }
            
            return NotFound(new { Success = false, Message = "Azure DevOps work item not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure DevOps work item");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("azure-devops/work-requests/{workRequestId}/work-items")]
    public async Task<IActionResult> GetAzureDevOpsWorkItemsByWorkRequest(int workRequestId)
    {
        try
        {
            var workItems = await _devOpsIntegrationService.GetWorkItemsByWorkRequestAsync(workRequestId);
            return Ok(new { Success = true, WorkItems = workItems });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure DevOps work items for work request");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Jira Integration
    [HttpPost("jira/issues")]
    public async Task<IActionResult> CreateJiraIssue([FromBody] CreateJiraIssueRequest request)
    {
        try
        {
            var issueKey = await _devOpsIntegrationService.CreateJiraIssueAsync(
                request.Project, request.IssueType, request.Summary, request.Description, request.WorkRequestId);
            
            if (!string.IsNullOrEmpty(issueKey))
            {
                return Ok(new { Success = true, IssueKey = issueKey });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to create Jira issue" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Jira issue");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPut("jira/issues/{issueKey}")]
    public async Task<IActionResult> UpdateJiraIssue(string issueKey, [FromBody] Dictionary<string, object> fields)
    {
        try
        {
            var result = await _devOpsIntegrationService.UpdateJiraIssueAsync(issueKey, fields);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Jira issue updated successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to update Jira issue" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Jira issue");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("jira/issues/{issueKey}")]
    public async Task<IActionResult> GetJiraIssue(string issueKey)
    {
        try
        {
            var issue = await _devOpsIntegrationService.GetJiraIssueAsync(issueKey);
            
            if (!string.IsNullOrEmpty(issue.Key))
            {
                return Ok(new { Success = true, Issue = issue });
            }
            
            return NotFound(new { Success = false, Message = "Jira issue not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jira issue");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("jira/work-requests/{workRequestId}/issues")]
    public async Task<IActionResult> GetJiraIssuesByWorkRequest(int workRequestId)
    {
        try
        {
            var issues = await _devOpsIntegrationService.GetIssuesByWorkRequestAsync(workRequestId);
            return Ok(new { Success = true, Issues = issues });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jira issues for work request");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Synchronization
    [HttpPost("sync/work-requests/{workRequestId}")]
    public async Task<IActionResult> SyncWorkRequestStatus(int workRequestId)
    {
        try
        {
            var result = await _devOpsIntegrationService.SyncWorkRequestStatusAsync(workRequestId);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Work request status synchronized successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to synchronize work request status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing work request status");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("sync/work-requests/{workRequestId}/history")]
    public async Task<IActionResult> GetSyncHistory(int workRequestId)
    {
        try
        {
            var history = await _devOpsIntegrationService.GetSyncHistoryAsync(workRequestId);
            return Ok(new { Success = true, History = history });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync history");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }
}

// Request DTOs
public class CreateAzureDevOpsWorkItemRequest
{
    public string Project { get; set; } = string.Empty;
    public string WorkItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
}

public class CreateJiraIssueRequest
{
    public string Project { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
} 