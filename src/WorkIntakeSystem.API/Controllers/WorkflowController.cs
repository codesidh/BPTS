using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Authorization;
using WorkIntakeSystem.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowStageConfigurationService _stageService;
    private readonly IWorkflowTransitionService _transitionService;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        IWorkflowEngine workflowEngine,
        IWorkflowStageConfigurationService stageService,
        IWorkflowTransitionService transitionService,
        ILogger<WorkflowController> logger)
    {
        _workflowEngine = workflowEngine;
        _stageService = stageService;
        _transitionService = transitionService;
        _logger = logger;
    }

    /// <summary>
    /// Get available transitions for a work request
    /// </summary>
    [HttpGet("work-requests/{workRequestId}/transitions")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetAvailableTransitions(int workRequestId)
    {
        try
        {
            var workRequest = await GetWorkRequestAsync(workRequestId);
            if (workRequest == null)
                return NotFound($"Work request {workRequestId} not found");

            var userId = GetCurrentUserId();
            var transitions = await _workflowEngine.GetAvailableTransitionsAsync(workRequest, userId);
            
            return Ok(transitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available transitions for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while retrieving available transitions");
        }
    }

    /// <summary>
    /// Advance work request to next stage
    /// </summary>
    [HttpPost("work-requests/{workRequestId}/advance")]
    [RequirePermission("Workflow:Update")]
    public async Task<IActionResult> AdvanceWorkRequest(int workRequestId, [FromBody] AdvanceWorkRequestRequest request)
    {
        try
        {
            var workRequest = await GetWorkRequestAsync(workRequestId);
            if (workRequest == null)
                return NotFound($"Work request {workRequestId} not found");

            var userId = GetCurrentUserId();
            
            if (!await _workflowEngine.CanAdvanceAsync(workRequest, request.NextStage, userId))
                return BadRequest("Transition not allowed");

            await _workflowEngine.AdvanceAsync(workRequest, request.NextStage, userId, request.Comments);
            
            return Ok(new { Message = $"Work request advanced to {request.NextStage}", NextStage = request.NextStage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error advancing work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while advancing the work request");
        }
    }

    /// <summary>
    /// Get SLA status for a work request
    /// </summary>
    [HttpGet("work-requests/{workRequestId}/sla-status")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetSLAStatus(int workRequestId)
    {
        try
        {
            var workRequest = await GetWorkRequestAsync(workRequestId);
            if (workRequest == null)
                return NotFound($"Work request {workRequestId} not found");

            var slaStatus = await _workflowEngine.GetSLAStatusAsync(workRequest);
            return Ok(slaStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA status for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while retrieving SLA status");
        }
    }

    /// <summary>
    /// Get all SLA violations
    /// </summary>
    [HttpGet("sla-violations")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetSLAViolations([FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var violations = await _workflowEngine.GetSLAViolationsAsync(asOfDate);
            return Ok(violations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA violations");
            return StatusCode(500, "An error occurred while retrieving SLA violations");
        }
    }

    /// <summary>
    /// Process SLA notifications
    /// </summary>
    [HttpPost("process-sla-notifications")]
    [RequirePermission("Workflow:Admin")]
    public async Task<IActionResult> ProcessSLANotifications()
    {
        try
        {
            await _workflowEngine.ProcessSLANotificationsAsync();
            return Ok(new { Message = "SLA notifications processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SLA notifications");
            return StatusCode(500, "An error occurred while processing SLA notifications");
        }
    }

    /// <summary>
    /// Process auto-transitions for all work requests
    /// </summary>
    [HttpPost("process-auto-transitions")]
    [RequirePermission("Workflow:Admin")]
    public async Task<IActionResult> ProcessAutoTransitions()
    {
        try
        {
            await _workflowEngine.ProcessAutoTransitionsAsync();
            return Ok(new { Message = "Auto-transitions processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto-transitions");
            return StatusCode(500, "An error occurred while processing auto-transitions");
        }
    }

    /// <summary>
    /// Process auto-transitions for a specific work request
    /// </summary>
    [HttpPost("work-requests/{workRequestId}/process-auto-transitions")]
    [RequirePermission("Workflow:Update")]
    public async Task<IActionResult> ProcessAutoTransitionsForWorkRequest(int workRequestId)
    {
        try
        {
            await _workflowEngine.ProcessAutoTransitionsForWorkRequestAsync(workRequestId);
            return Ok(new { Message = "Auto-transitions processed for work request" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto-transitions for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while processing auto-transitions");
        }
    }

    /// <summary>
    /// Get workflow state for a work request
    /// </summary>
    [HttpGet("work-requests/{workRequestId}/state")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetWorkflowState(int workRequestId)
    {
        try
        {
            var state = await _workflowEngine.GetWorkflowStateAsync(workRequestId);
            return Ok(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow state for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while retrieving workflow state");
        }
    }

    /// <summary>
    /// Get workflow history for a work request
    /// </summary>
    [HttpGet("work-requests/{workRequestId}/history")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetWorkflowHistory(int workRequestId)
    {
        try
        {
            var history = await _workflowEngine.GetWorkflowHistoryAsync(workRequestId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow history for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while retrieving workflow history");
        }
    }

    /// <summary>
    /// Replay workflow state to a specific date
    /// </summary>
    [HttpPost("work-requests/{workRequestId}/replay-state")]
    [RequirePermission("Workflow:Admin")]
    public async Task<IActionResult> ReplayWorkflowState(int workRequestId, [FromBody] ReplayStateRequest request)
    {
        try
        {
            var success = await _workflowEngine.ReplayWorkflowStateAsync(workRequestId, request.TargetDate);
            
            if (!success)
                return BadRequest("Failed to replay workflow state");

            return Ok(new { Message = "Workflow state replayed successfully", TargetDate = request.TargetDate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replaying workflow state for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while replaying workflow state");
        }
    }

    /// <summary>
    /// Validate workflow configuration
    /// </summary>
    [HttpGet("validate-configuration")]
    [RequirePermission("Workflow:Admin")]
    public async Task<IActionResult> ValidateWorkflowConfiguration([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var result = await _workflowEngine.ValidateWorkflowConfigurationAsync(businessVerticalId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating workflow configuration");
            return StatusCode(500, "An error occurred while validating workflow configuration");
        }
    }

    /// <summary>
    /// Evaluate business rule
    /// </summary>
    [HttpPost("evaluate-business-rule")]
    [RequirePermission("Workflow:Admin")]
    public async Task<IActionResult> EvaluateBusinessRule([FromBody] EvaluateRuleRequest request)
    {
        try
        {
            var workRequest = await GetWorkRequestAsync(request.WorkRequestId);
            if (workRequest == null)
                return NotFound($"Work request {request.WorkRequestId} not found");

            var userId = GetCurrentUserId();
            var result = await _workflowEngine.EvaluateBusinessRuleAsync(request.RuleScript, workRequest, userId);
            
            return Ok(new { Result = result, RuleScript = request.RuleScript });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating business rule");
            return StatusCode(500, "An error occurred while evaluating business rule");
        }
    }

    /// <summary>
    /// Process approval workflow
    /// </summary>
    [HttpPost("work-requests/{workRequestId}/approve")]
    [RequirePermission("Workflow:Approve")]
    public async Task<IActionResult> ProcessApproval(int workRequestId, [FromBody] ApprovalRequest request)
    {
        try
        {
            var workRequest = await GetWorkRequestAsync(workRequestId);
            if (workRequest == null)
                return NotFound($"Work request {workRequestId} not found");

            var approverId = GetCurrentUserId();
            var result = await _workflowEngine.ProcessApprovalWorkflowAsync(workRequest, approverId, request.Approved, request.Comments);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approval for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while processing approval");
        }
    }

    /// <summary>
    /// Get pending approvals for current user
    /// </summary>
    [HttpGet("pending-approvals")]
    [RequirePermission("Workflow:Approve")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        try
        {
            var userId = GetCurrentUserId();
            var pendingApprovals = await _workflowEngine.GetPendingApprovalsAsync(userId);
            
            return Ok(pendingApprovals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals");
            return StatusCode(500, "An error occurred while retrieving pending approvals");
        }
    }

    /// <summary>
    /// Get workflow metrics
    /// </summary>
    [HttpGet("metrics")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetWorkflowMetrics([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var metrics = await _workflowEngine.GetWorkflowMetricsAsync(fromDate, toDate, businessVerticalId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow metrics");
            return StatusCode(500, "An error occurred while retrieving workflow metrics");
        }
    }

    /// <summary>
    /// Identify workflow bottlenecks
    /// </summary>
    [HttpGet("bottlenecks")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> IdentifyBottlenecks([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var bottlenecks = await _workflowEngine.IdentifyBottlenecksAsync(businessVerticalId);
            return Ok(bottlenecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying workflow bottlenecks");
            return StatusCode(500, "An error occurred while identifying bottlenecks");
        }
    }

    /// <summary>
    /// Get average completion time for a stage
    /// </summary>
    [HttpGet("stages/{stage}/average-completion-time")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetAverageCompletionTime(WorkflowStage stage, [FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var averageTime = await _workflowEngine.GetAverageCompletionTimeAsync(stage, businessVerticalId);
            return Ok(new { Stage = stage.ToString(), AverageCompletionTimeHours = averageTime });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average completion time for stage {Stage}", stage);
            return StatusCode(500, "An error occurred while retrieving average completion time");
        }
    }

    // Stage Configuration Endpoints
    /// <summary>
    /// Get all workflow stages
    /// </summary>
    [HttpGet("stages")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetAllStages([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var stages = await _stageService.GetAllStagesAsync(businessVerticalId);
            return Ok(stages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow stages");
            return StatusCode(500, "An error occurred while retrieving workflow stages");
        }
    }

    /// <summary>
    /// Get workflow stage by ID
    /// </summary>
    [HttpGet("stages/{stageId}")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetStage(int stageId)
    {
        try
        {
            var stage = await _stageService.GetStageAsync(stageId);
            if (stage == null)
                return NotFound($"Workflow stage {stageId} not found");

            return Ok(stage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow stage {StageId}", stageId);
            return StatusCode(500, "An error occurred while retrieving workflow stage");
        }
    }

    /// <summary>
    /// Create workflow stage
    /// </summary>
    [HttpPost("stages")]
    [RequirePermission("Workflow:Create")]
    public async Task<IActionResult> CreateStage([FromBody] WorkflowStageConfiguration stage)
    {
        try
        {
            var createdStage = await _stageService.CreateStageAsync(stage);
            return CreatedAtAction(nameof(GetStage), new { stageId = createdStage.Id }, createdStage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow stage");
            return StatusCode(500, "An error occurred while creating workflow stage");
        }
    }

    /// <summary>
    /// Update workflow stage
    /// </summary>
    [HttpPut("stages/{stageId}")]
    [RequirePermission("Workflow:Update")]
    public async Task<IActionResult> UpdateStage(int stageId, [FromBody] WorkflowStageConfiguration stage)
    {
        try
        {
            if (stageId != stage.Id)
                return BadRequest("Stage ID mismatch");

            var updatedStage = await _stageService.UpdateStageAsync(stage);
            return Ok(updatedStage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow stage {StageId}", stageId);
            return StatusCode(500, "An error occurred while updating workflow stage");
        }
    }

    /// <summary>
    /// Delete workflow stage
    /// </summary>
    [HttpDelete("stages/{stageId}")]
    [RequirePermission("Workflow:Delete")]
    public async Task<IActionResult> DeleteStage(int stageId)
    {
        try
        {
            var success = await _stageService.DeleteStageAsync(stageId);
            if (!success)
                return NotFound($"Workflow stage {stageId} not found");

            return Ok(new { Message = "Workflow stage deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow stage {StageId}", stageId);
            return StatusCode(500, "An error occurred while deleting workflow stage");
        }
    }

    // Transition Configuration Endpoints
    /// <summary>
    /// Get all workflow transitions
    /// </summary>
    [HttpGet("transitions")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetAllTransitions([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var transitions = await _transitionService.GetAllTransitionsAsync(businessVerticalId);
            return Ok(transitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow transitions");
            return StatusCode(500, "An error occurred while retrieving workflow transitions");
        }
    }

    /// <summary>
    /// Get workflow transition by ID
    /// </summary>
    [HttpGet("transitions/{transitionId}")]
    [RequirePermission("Workflow:Read")]
    public async Task<IActionResult> GetTransition(int transitionId)
    {
        try
        {
            var transition = await _transitionService.GetTransitionAsync(transitionId);
            if (transition == null)
                return NotFound($"Workflow transition {transitionId} not found");

            return Ok(transition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow transition {TransitionId}", transitionId);
            return StatusCode(500, "An error occurred while retrieving workflow transition");
        }
    }

    /// <summary>
    /// Create workflow transition
    /// </summary>
    [HttpPost("transitions")]
    [RequirePermission("Workflow:Create")]
    public async Task<IActionResult> CreateTransition([FromBody] WorkflowTransition transition)
    {
        try
        {
            var createdTransition = await _transitionService.CreateTransitionAsync(transition);
            return CreatedAtAction(nameof(GetTransition), new { transitionId = createdTransition.Id }, createdTransition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow transition");
            return StatusCode(500, "An error occurred while creating workflow transition");
        }
    }

    /// <summary>
    /// Update workflow transition
    /// </summary>
    [HttpPut("transitions/{transitionId}")]
    [RequirePermission("Workflow:Update")]
    public async Task<IActionResult> UpdateTransition(int transitionId, [FromBody] WorkflowTransition transition)
    {
        try
        {
            if (transitionId != transition.Id)
                return BadRequest("Transition ID mismatch");

            var updatedTransition = await _transitionService.UpdateTransitionAsync(transition);
            return Ok(updatedTransition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow transition {TransitionId}", transitionId);
            return StatusCode(500, "An error occurred while updating workflow transition");
        }
    }

    /// <summary>
    /// Delete workflow transition
    /// </summary>
    [HttpDelete("transitions/{transitionId}")]
    [RequirePermission("Workflow:Delete")]
    public async Task<IActionResult> DeleteTransition(int transitionId)
    {
        try
        {
            var success = await _transitionService.DeleteTransitionAsync(transitionId);
            if (!success)
                return NotFound($"Workflow transition {transitionId} not found");

            return Ok(new { Message = "Workflow transition deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow transition {TransitionId}", transitionId);
            return StatusCode(500, "An error occurred while deleting workflow transition");
        }
    }

    // Helper methods
    private async Task<WorkRequest?> GetWorkRequestAsync(int workRequestId)
    {
        // This would typically be injected as a service
        // For now, we'll access the context directly (not ideal, but for implementation)
        return null; // Would be implemented with proper service injection
    }

    private int GetCurrentUserId()
    {
        // Extract user ID from claims
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            return userId;
        
        return 0; // Default or throw exception
    }
}

// Request DTOs
public record AdvanceWorkRequestRequest(WorkflowStage NextStage, string? Comments = null);
public record ReplayStateRequest(DateTime TargetDate);
public record EvaluateRuleRequest(int WorkRequestId, string RuleScript);
public record ApprovalRequest(bool Approved, string? Comments = null); 