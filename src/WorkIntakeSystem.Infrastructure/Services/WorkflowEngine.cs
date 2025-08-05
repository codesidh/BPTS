using System;
using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly WorkIntakeDbContext _context;
        private readonly IWorkflowStageConfigurationService _stageService;
        private readonly IWorkflowTransitionService _transitionService;
        private readonly IEmailService _emailService;
        private readonly ILogger<WorkflowEngine> _logger;

        public WorkflowEngine(
            WorkIntakeDbContext context,
            IWorkflowStageConfigurationService stageService,
            IWorkflowTransitionService transitionService,
            IEmailService emailService,
            ILogger<WorkflowEngine> logger)
        {
            _context = context;
            _stageService = stageService;
            _transitionService = transitionService;
            _emailService = emailService;
            _logger = logger;
        }

        // Existing methods (enhanced)
        public async Task<bool> CanAdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Get the transition
                var transition = await GetTransitionAsync(workRequest, nextStage, workRequest.BusinessVerticalId);
                if (transition == null) return false;

                // Validate transition conditions
                return await ValidateTransitionConditionsAsync(workRequest, transition, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if work request {WorkRequestId} can advance to {NextStage}", 
                    workRequest.Id, nextStage);
                return false;
            }
        }

        public async Task AdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId, string? comments = null)
        {
            try
            {
                if (!await CanAdvanceAsync(workRequest, nextStage, userId))
                    throw new InvalidOperationException("Transition not allowed");

                var transition = await GetTransitionAsync(workRequest, nextStage, workRequest.BusinessVerticalId);
                if (transition == null)
                    throw new InvalidOperationException("Transition configuration not found");

                // Calculate time in current stage for SLA tracking
                var timeInStage = DateTime.UtcNow - (workRequest.ModifiedDate != DateTime.MinValue ? workRequest.ModifiedDate : workRequest.CreatedDate);
                var oldStage = workRequest.CurrentStage;

                // Update work request
                workRequest.CurrentStage = nextStage;
                workRequest.ModifiedDate = DateTime.UtcNow;

                // Create audit trail with enhanced metadata
                var auditMetadata = new
                {
                    TimeInPreviousStageHours = timeInStage.TotalHours,
                    TransitionId = transition.Id,
                    TransitionName = transition.TransitionName,
                    UserId = userId,
                    Comments = comments ?? string.Empty,
                    Timestamp = DateTime.UtcNow
                };

                var audit = new AuditTrail
                {
                    WorkRequestId = workRequest.Id,
                    Action = $"Workflow advanced: {oldStage} -> {nextStage}",
                    OldValue = oldStage.ToString(),
                    NewValue = nextStage.ToString(),
                    ChangedById = userId,
                    ChangedDate = DateTime.UtcNow,
                    Comments = comments ?? string.Empty,
                    Metadata = JsonSerializer.Serialize(auditMetadata)
                };

                _context.AuditTrails.Add(audit);

                // Create event store entry
                var eventData = new
                {
                    workRequestId = workRequest.Id,
                    from = oldStage.ToString(),
                    to = nextStage.ToString(),
                    transitionId = transition.Id,
                    userId = userId,
                    comments = comments,
                    timeInPreviousStage = timeInStage.TotalHours
                };

                var evt = new EventStore
                {
                    AggregateId = workRequest.Id.ToString(),
                    EventType = "WorkflowStageChanged",
                    EventData = JsonSerializer.Serialize(eventData),
                    CreatedBy = userId.ToString(),
                    Timestamp = DateTime.UtcNow
                };

                _context.EventStore.Add(evt);
                await _context.SaveChangesAsync();

                // Send notifications if required
                if (transition.NotificationRequired)
                {
                    await NotifyStakeholdersAsync(workRequest, transition, "StageChanged");
                }

                _logger.LogInformation("Work request {WorkRequestId} advanced from {OldStage} to {NewStage} by user {UserId}",
                    workRequest.Id, oldStage, nextStage, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error advancing work request {WorkRequestId} to {NextStage}", 
                    workRequest.Id, nextStage);
                throw;
            }
        }

        // Enhanced workflow management
        public async Task<IEnumerable<WorkflowStage>> GetAvailableTransitionsAsync(WorkRequest workRequest, int userId)
        {
            try
            {
                var currentStageOrder = (int)workRequest.CurrentStage;
                var currentStageConfig = await _stageService.GetStageByOrderAsync(currentStageOrder, workRequest.BusinessVerticalId);
                
                if (currentStageConfig == null) return Enumerable.Empty<WorkflowStage>();

                var availableTransitions = await _transitionService.GetAvailableTransitionsAsync(currentStageConfig.Id, userId, workRequest.BusinessVerticalId);
                
                return availableTransitions.Select(t => (WorkflowStage)t.ToStage.Order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available transitions for work request {WorkRequestId}", workRequest.Id);
                return Enumerable.Empty<WorkflowStage>();
            }
        }

        public async Task<WorkflowTransition?> GetTransitionAsync(WorkRequest workRequest, WorkflowStage nextStage, int? businessVerticalId = null)
        {
            try
            {
                var currentStageOrder = (int)workRequest.CurrentStage;
                var nextStageOrder = (int)nextStage;

                var currentStageConfig = await _stageService.GetStageByOrderAsync(currentStageOrder, businessVerticalId);
                var nextStageConfig = await _stageService.GetStageByOrderAsync(nextStageOrder, businessVerticalId);

                if (currentStageConfig == null || nextStageConfig == null) return null;

                return await _transitionService.GetTransitionAsync(currentStageConfig.Id, nextStageConfig.Id, businessVerticalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transition from {CurrentStage} to {NextStage}", 
                    workRequest.CurrentStage, nextStage);
                return null;
            }
        }

        public async Task<bool> ValidateTransitionConditionsAsync(WorkRequest workRequest, WorkflowTransition transition, int userId)
        {
            try
            {
                // Check basic role requirements
                if (!await _transitionService.CanUserExecuteTransitionAsync(transition.Id, userId))
                    return false;

                // Evaluate conditional logic
                if (!await _transitionService.EvaluateConditionAsync(transition.Id, workRequest, userId))
                    return false;

                // Validate transition rules
                var validationResult = await _transitionService.ValidateTransitionAsync(transition.Id, workRequest, userId);
                return validationResult.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transition conditions for transition {TransitionId}", transition.Id);
                return false;
            }
        }

        // Auto-transition capabilities
        public async Task ProcessAutoTransitionsAsync()
        {
            try
            {
                var autoTransitions = await _transitionService.GetAutoTransitionsAsync();
                var processedCount = 0;

                foreach (var transition in autoTransitions)
                {
                    var workRequests = await _context.WorkRequests
                        .Where(wr => (int)wr.CurrentStage == transition.FromStage.Order)
                        .ToListAsync();

                    foreach (var workRequest in workRequests)
                    {
                        if (await ShouldAutoTransitionAsync(workRequest, transition))
                        {
                            var nextStage = (WorkflowStage)transition.ToStage.Order;
                            await AdvanceAsync(workRequest, nextStage, 0, "Auto-transition");
                            processedCount++;
                            
                            _logger.LogInformation("Auto-transitioned work request {WorkRequestId} from {FromStage} to {ToStage}",
                                workRequest.Id, transition.FromStage.Name, transition.ToStage.Name);
                        }
                    }
                }

                _logger.LogInformation("Processed {Count} auto-transitions", processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing auto-transitions");
            }
        }

        public async Task ProcessAutoTransitionsForWorkRequestAsync(int workRequestId)
        {
            try
            {
                var workRequest = await _context.WorkRequests.FindAsync(workRequestId);
                if (workRequest == null) return;

                var currentStageOrder = (int)workRequest.CurrentStage;
                var currentStageConfig = await _stageService.GetStageByOrderAsync(currentStageOrder, workRequest.BusinessVerticalId);
                
                if (currentStageConfig == null) return;

                var transitions = await _transitionService.GetTransitionsFromStageAsync(currentStageConfig.Id);
                var autoTransitions = transitions.Where(t => t.AutoTransitionDelayMinutes.HasValue);

                foreach (var transition in autoTransitions)
                {
                    if (await ShouldAutoTransitionAsync(workRequest, transition))
                    {
                        var nextStage = (WorkflowStage)transition.ToStage.Order;
                        await AdvanceAsync(workRequest, nextStage, 0, "Auto-transition");
                        break; // Only process one auto-transition at a time
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing auto-transitions for work request {WorkRequestId}", workRequestId);
            }
        }

        public async Task<bool> ShouldAutoTransitionAsync(WorkRequest workRequest, WorkflowTransition transition)
        {
            try
            {
                if (!transition.AutoTransitionDelayMinutes.HasValue) return false;

                // Check if enough time has passed
                var timeInStage = DateTime.UtcNow - workRequest.ModifiedDate;
                var requiredDelay = TimeSpan.FromMinutes(transition.AutoTransitionDelayMinutes.Value);

                if (timeInStage < requiredDelay) return false;

                // Evaluate conditions
                return await _transitionService.EvaluateConditionAsync(transition.Id, workRequest, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if work request {WorkRequestId} should auto-transition", workRequest.Id);
                return false;
            }
        }

        // SLA tracking and notifications
        public async Task<SLAStatus> GetSLAStatusAsync(WorkRequest workRequest)
        {
            try
            {
                var currentStageOrder = (int)workRequest.CurrentStage;
                var stageConfig = await _stageService.GetStageByOrderAsync(currentStageOrder, workRequest.BusinessVerticalId);
                
                var status = new SLAStatus
                {
                    WorkRequestId = workRequest.Id,
                    CurrentStage = workRequest.CurrentStage,
                    StageEntryTime = workRequest.ModifiedDate,
                    SLAHours = stageConfig?.SLAHours
                };

                if (status.SLAHours.HasValue)
                {
                    status.SLADeadline = status.StageEntryTime.AddHours(status.SLAHours.Value);
                    status.TimeRemaining = status.SLADeadline.Value - DateTime.UtcNow;
                    status.IsViolated = DateTime.UtcNow > status.SLADeadline.Value;
                    status.IsAtRisk = status.TimeRemaining.TotalHours <= (status.SLAHours.Value * 0.25); // 25% threshold

                    status.Status = status.IsViolated ? "Violated" : 
                                   status.IsAtRisk ? "At Risk" : "On Track";
                }
                else
                {
                    status.Status = "No SLA";
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SLA status for work request {WorkRequestId}", workRequest.Id);
                return new SLAStatus { WorkRequestId = workRequest.Id, Status = "Error" };
            }
        }

        public async Task<IEnumerable<WorkRequest>> GetSLAViolationsAsync(DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.UtcNow;
                var violations = new List<WorkRequest>();

                var workRequests = await _context.WorkRequests
                    .Where(wr => wr.Status != WorkStatus.Closed && wr.Status != WorkStatus.Rejected)
                    .ToListAsync();

                foreach (var workRequest in workRequests)
                {
                    var slaStatus = await GetSLAStatusAsync(workRequest);
                    if (slaStatus.IsViolated)
                    {
                        violations.Add(workRequest);
                    }
                }

                return violations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SLA violations");
                return Enumerable.Empty<WorkRequest>();
            }
        }

        public async Task ProcessSLANotificationsAsync()
        {
            try
            {
                var workRequests = await _context.WorkRequests
                    .Where(wr => wr.Status != WorkStatus.Closed && wr.Status != WorkStatus.Rejected)
                    .ToListAsync();

                var notificationsSent = 0;

                foreach (var workRequest in workRequests)
                {
                    var slaStatus = await GetSLAStatusAsync(workRequest);
                    
                    if (slaStatus.IsViolated || slaStatus.IsAtRisk)
                    {
                        await SendSLANotificationAsync(workRequest, slaStatus);
                        notificationsSent++;
                    }
                }

                _logger.LogInformation("Sent {Count} SLA notifications", notificationsSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SLA notifications");
            }
        }

        public async Task NotifyStakeholdersAsync(WorkRequest workRequest, WorkflowTransition transition, string eventType)
        {
            try
            {
                var template = await _transitionService.GetNotificationTemplateAsync(transition.Id);
                if (template == null) return;

                var subject = RenderTemplate(template.Subject, workRequest, transition, eventType);
                var body = RenderTemplate(template.Body, workRequest, transition, eventType);

                // Send notifications to configured recipients
                foreach (var recipient in template.Recipients)
                {
                    if (template.SendEmail)
                    {
                        await _emailService.SendNotificationEmailAsync(recipient, subject, body);
                    }
                    
                    // In-app notifications would be handled here
                    if (template.SendInApp)
                    {
                        // Create in-app notification record
                        _logger.LogInformation("In-app notification sent to {Recipient} for work request {WorkRequestId}",
                            recipient, workRequest.Id);
                    }
                }

                _logger.LogInformation("Sent notifications for work request {WorkRequestId} transition {TransitionName}",
                    workRequest.Id, transition.TransitionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications for work request {WorkRequestId}", workRequest.Id);
            }
        }

        // Workflow state management
        public async Task<WorkflowState> GetWorkflowStateAsync(int workRequestId)
        {
            try
            {
                var workRequest = await _context.WorkRequests.FindAsync(workRequestId);
                if (workRequest == null) 
                    throw new ArgumentException($"Work request {workRequestId} not found");

                var latestAudit = await _context.AuditTrails
                    .Where(at => at.WorkRequestId == workRequestId && at.Action.Contains("Workflow advanced"))
                    .OrderByDescending(at => at.ChangedDate)
                    .FirstOrDefaultAsync();

                var state = new WorkflowState
                {
                    WorkRequestId = workRequestId,
                    Stage = workRequest.CurrentStage,
                    EntryTime = workRequest.ModifiedDate,
                    UserId = latestAudit?.ChangedById ?? 0,
                    Comments = latestAudit?.Comments
                };

                // Extract metadata if available
                if (!string.IsNullOrEmpty(latestAudit?.Metadata))
                {
                    try
                    {
                        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(latestAudit.Metadata);
                        state.Metadata = metadata ?? new Dictionary<string, object>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deserializing audit metadata for work request {WorkRequestId}", workRequestId);
                    }
                }

                return state;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow state for work request {WorkRequestId}", workRequestId);
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowState>> GetWorkflowHistoryAsync(int workRequestId)
        {
            try
            {
                var auditTrails = await _context.AuditTrails
                    .Where(at => at.WorkRequestId == workRequestId && at.Action.Contains("Workflow advanced"))
                    .OrderBy(at => at.ChangedDate)
                    .ToListAsync();

                var states = new List<WorkflowState>();
                DateTime? previousDate = null;

                foreach (var audit in auditTrails)
                {
                    var state = new WorkflowState
                    {
                        WorkRequestId = workRequestId,
                        Stage = Enum.Parse<WorkflowStage>(audit.NewValue ?? "Draft"),
                        EntryTime = audit.ChangedDate,
                        UserId = audit.ChangedById,
                        Comments = audit.Comments
                    };

                    if (previousDate.HasValue)
                    {
                        state.Duration = audit.ChangedDate - previousDate.Value;
                    }

                    // Extract metadata
                    if (!string.IsNullOrEmpty(audit.Metadata))
                    {
                        try
                        {
                            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.Metadata);
                            state.Metadata = metadata ?? new Dictionary<string, object>();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error deserializing audit metadata");
                        }
                    }

                    states.Add(state);
                    previousDate = audit.ChangedDate;
                }

                // Set exit time for all states except the last one
                for (int i = 0; i < states.Count - 1; i++)
                {
                    states[i].ExitTime = states[i + 1].EntryTime;
                }

                return states;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow history for work request {WorkRequestId}", workRequestId);
                return Enumerable.Empty<WorkflowState>();
            }
        }

        public async Task<bool> ReplayWorkflowStateAsync(int workRequestId, DateTime targetDate)
        {
            try
            {
                var history = await GetWorkflowHistoryAsync(workRequestId);
                var targetState = history
                    .Where(s => s.EntryTime <= targetDate)
                    .OrderByDescending(s => s.EntryTime)
                    .FirstOrDefault();

                if (targetState == null) return false;

                var workRequest = await _context.WorkRequests.FindAsync(workRequestId);
                if (workRequest == null) return false;

                // Create a snapshot of the state at the target date
                var snapshot = new EventSnapshot
                {
                    AggregateId = workRequestId.ToString(),
                    SnapshotType = "WorkRequest",
                    SnapshotData = JsonSerializer.Serialize(new
                    {
                        workRequestId = workRequestId,
                        stage = targetState.Stage.ToString(),
                        entryTime = targetState.EntryTime,
                        userId = targetState.UserId,
                        comments = targetState.Comments,
                        metadata = targetState.Metadata
                    }),
                    Version = history.Count(s => s.EntryTime <= targetDate),
                    Timestamp = DateTime.UtcNow,
                    CreatedById = 0, // System user
                    CreatedDate = DateTime.UtcNow
                };

                // Note: EventSnapshots would need to be added to DbContext
                // _context.EventSnapshots.Add(snapshot);
                // await _context.SaveChangesAsync();

                _logger.LogInformation("Created workflow state snapshot for work request {WorkRequestId} at {TargetDate}",
                    workRequestId, targetDate);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replaying workflow state for work request {WorkRequestId}", workRequestId);
                return false;
            }
        }

        public async Task<WorkflowValidationResult> ValidateWorkflowConfigurationAsync(int? businessVerticalId = null)
        {
            var result = new WorkflowValidationResult { IsValid = true };

            try
            {
                var stages = await _stageService.GetAllStagesAsync(businessVerticalId);
                var transitions = await _transitionService.GetAllTransitionsAsync(businessVerticalId);

                // Validate stage configuration
                foreach (var stage in stages)
                {
                    var stageErrors = await _stageService.GetValidationErrorsAsync(stage);
                    result.ValidationErrors.AddRange(stageErrors);
                }

                // Validate transitions
                var stageIds = stages.Select(s => s.Id).ToHashSet();
                foreach (var transition in transitions)
                {
                    if (!stageIds.Contains(transition.FromStageId))
                    {
                        result.ValidationErrors.Add($"Transition {transition.TransitionName} references invalid from stage {transition.FromStageId}");
                    }
                    
                    if (!stageIds.Contains(transition.ToStageId))
                    {
                        result.ValidationErrors.Add($"Transition {transition.TransitionName} references invalid to stage {transition.ToStageId}");
                    }
                }

                // Check for orphaned stages (no incoming or outgoing transitions)
                foreach (var stage in stages)
                {
                    var hasIncoming = transitions.Any(t => t.ToStageId == stage.Id);
                    var hasOutgoing = transitions.Any(t => t.FromStageId == stage.Id);
                    
                    if (!hasIncoming && stage.Order > 0)
                    {
                        result.Warnings.Add($"Stage {stage.Name} has no incoming transitions");
                    }
                    
                    if (!hasOutgoing && stage.Order < stages.Max(s => s.Order))
                    {
                        result.Warnings.Add($"Stage {stage.Name} has no outgoing transitions");
                    }
                }

                result.IsValid = !result.ValidationErrors.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating workflow configuration");
                result.IsValid = false;
                result.ValidationErrors.Add("Error occurred during validation");
            }

            return result;
        }

        // Business rule evaluation
        public async Task<bool> EvaluateBusinessRuleAsync(string ruleScript, WorkRequest workRequest, int userId)
        {
            try
            {
                // Simple rule evaluation - can be extended with a proper rules engine
                if (string.IsNullOrWhiteSpace(ruleScript)) return true;

                var rule = JsonSerializer.Deserialize<BusinessRuleScript>(ruleScript);
                if (rule == null) return true;

                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                return rule.Type.ToLower() switch
                {
                    "priority" => EvaluatePriorityRule(rule, workRequest),
                    "role" => EvaluateRoleRule(rule, user),
                    "timeelapsed" => EvaluateTimeElapsedRule(rule, workRequest),
                    "businessvertical" => EvaluateBusinessVerticalRule(rule, workRequest),
                    _ => true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating business rule");
                return false;
            }
        }

        public async Task<ApprovalResult> ProcessApprovalWorkflowAsync(WorkRequest workRequest, int approverId, bool approved, string? comments = null)
        {
            try
            {
                var approver = await _context.Users.FindAsync(approverId);
                if (approver == null)
                {
                    return new ApprovalResult
                    {
                        Success = false,
                        Message = "Approver not found"
                    };
                }

                // Check if user can approve at current stage
                var currentStageOrder = (int)workRequest.CurrentStage;
                var stageConfig = await _stageService.GetStageByOrderAsync(currentStageOrder, workRequest.BusinessVerticalId);
                
                if (stageConfig == null || !stageConfig.ApprovalRequired)
                {
                    return new ApprovalResult
                    {
                        Success = false,
                        Message = "Current stage does not require approval"
                    };
                }

                if (!await _stageService.CanUserAccessStageAsync(stageConfig.Id, approverId))
                {
                    return new ApprovalResult
                    {
                        Success = false,
                        Message = "User does not have permission to approve at this stage"
                    };
                }

                // Record approval decision
                var auditAction = approved ? "Approved" : "Rejected";
                var audit = new AuditTrail
                {
                    WorkRequestId = workRequest.Id,
                    Action = $"Workflow {auditAction} at stage {workRequest.CurrentStage}",
                    OldValue = "Pending Approval",
                    NewValue = auditAction,
                    ChangedById = approverId,
                    ChangedDate = DateTime.UtcNow,
                    Comments = comments ?? string.Empty,
                    Metadata = JsonSerializer.Serialize(new { ApprovalDecision = approved, ApproverId = approverId })
                };

                _context.AuditTrails.Add(audit);

                WorkflowStage? nextStage = null;
                if (approved)
                {
                    // Find next stage in workflow
                    var availableTransitions = await GetAvailableTransitionsAsync(workRequest, approverId);
                    nextStage = availableTransitions.FirstOrDefault();
                    
                    if (nextStage.HasValue)
                    {
                        await AdvanceAsync(workRequest, nextStage.Value, approverId, $"Approved: {comments}");
                    }
                }
                else
                {
                    // Handle rejection - could move to a rejection stage or back to previous stage
                    // For now, we'll just record the rejection
                    workRequest.ModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return new ApprovalResult
                {
                    Success = true,
                    Message = approved ? "Request approved successfully" : "Request rejected",
                    NextStage = nextStage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing approval for work request {WorkRequestId}", workRequest.Id);
                return new ApprovalResult
                {
                    Success = false,
                    Message = "Error processing approval"
                };
            }
        }

        public async Task<IEnumerable<WorkRequest>> GetPendingApprovalsAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return Enumerable.Empty<WorkRequest>();

                var workRequests = await _context.WorkRequests
                    .Where(wr => wr.Status != WorkStatus.Closed && wr.Status != WorkStatus.Rejected)
                    .ToListAsync();

                var pendingApprovals = new List<WorkRequest>();

                foreach (var workRequest in workRequests)
                {
                    var currentStageOrder = (int)workRequest.CurrentStage;
                    var stageConfig = await _stageService.GetStageByOrderAsync(currentStageOrder, workRequest.BusinessVerticalId);
                    
                    if (stageConfig?.ApprovalRequired == true && 
                        await _stageService.CanUserAccessStageAsync(stageConfig.Id, userId))
                    {
                        pendingApprovals.Add(workRequest);
                    }
                }

                return pendingApprovals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals for user {UserId}", userId);
                return Enumerable.Empty<WorkRequest>();
            }
        }

        // Workflow metrics and analytics
        public async Task<WorkflowMetrics> GetWorkflowMetricsAsync(DateTime fromDate, DateTime toDate, int? businessVerticalId = null)
        {
            try
            {
                var query = _context.WorkRequests.AsQueryable();
                
                if (businessVerticalId.HasValue)
                    query = query.Where(wr => wr.BusinessVerticalId == businessVerticalId);

                var workRequests = await query
                    .Where(wr => wr.CreatedDate >= fromDate && wr.CreatedDate <= toDate)
                    .ToListAsync();

                var completed = workRequests.Where(wr => wr.Status == WorkStatus.Closed).ToList();
                var slaViolations = 0;

                // Calculate SLA violations
                foreach (var wr in workRequests)
                {
                    var slaStatus = await GetSLAStatusAsync(wr);
                    if (slaStatus.IsViolated) slaViolations++;
                }

                var metrics = new WorkflowMetrics
                {
                    TotalWorkRequests = workRequests.Count,
                    CompletedWorkRequests = completed.Count,
                    AverageCompletionDays = completed.Any() ? 
                        completed.Average(wr => (wr.ModifiedDate - wr.CreatedDate).TotalDays) : 0,
                    SLAViolations = slaViolations,
                    SLAComplianceRate = workRequests.Any() ? 
                        (double)(workRequests.Count - slaViolations) / workRequests.Count * 100 : 100,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                // Calculate stage distribution
                foreach (WorkflowStage stage in Enum.GetValues<WorkflowStage>())
                {
                    var count = workRequests.Count(wr => wr.CurrentStage == stage);
                    if (count > 0)
                        metrics.StageDistribution[stage] = count;
                }

                // Calculate average stage times from audit trails
                var auditTrails = await _context.AuditTrails
                    .Where(at => workRequests.Select(wr => wr.Id).Contains(at.WorkRequestId))
                    .Where(at => at.Action.Contains("Workflow advanced"))
                    .Where(at => at.ChangedDate >= fromDate && at.ChangedDate <= toDate)
                    .ToListAsync();

                foreach (WorkflowStage stage in Enum.GetValues<WorkflowStage>())
                {
                    var stageTimes = auditTrails
                        .Where(at => at.OldValue == stage.ToString())
                        .Select(at => ExtractTimeInStage(at.Metadata))
                        .Where(time => time > 0);

                    if (stageTimes.Any())
                        metrics.AverageStageTime[stage] = stageTimes.Average();
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating workflow metrics");
                return new WorkflowMetrics { FromDate = fromDate, ToDate = toDate };
            }
        }

        public async Task<IEnumerable<WorkflowBottleneckAnalysis>> IdentifyBottlenecksAsync(int? businessVerticalId = null)
        {
            try
            {
                var bottlenecks = new List<WorkflowBottleneckAnalysis>();
                var stages = await _stageService.GetAllStagesAsync(businessVerticalId);

                foreach (var stage in stages)
                {
                    var workflowStage = (WorkflowStage)stage.Order;
                    var pendingCount = await _context.WorkRequests
                        .CountAsync(wr => wr.CurrentStage == workflowStage && 
                                         wr.Status != WorkStatus.Closed && 
                                         wr.Status != WorkStatus.Rejected);

                    if (pendingCount > 0)
                    {
                        var averageWaitTime = await GetAverageCompletionTimeAsync(workflowStage, businessVerticalId);
                        
                        var bottleneck = new WorkflowBottleneckAnalysis
                        {
                            Stage = workflowStage,
                            PendingCount = pendingCount,
                            AverageWaitTime = averageWaitTime
                        };

                        // Determine bottleneck type and recommendation
                        if (averageWaitTime > 72) // More than 3 days
                        {
                            bottleneck.BottleneckType = "Process";
                            bottleneck.Recommendation = "Review stage requirements and consider automation";
                        }
                        else if (pendingCount > 10)
                        {
                            bottleneck.BottleneckType = "Volume";
                            bottleneck.Recommendation = "Consider adding more resources or parallel processing";
                        }
                        else if (stage.ApprovalRequired)
                        {
                            bottleneck.BottleneckType = "Approval";
                            bottleneck.Recommendation = "Review approval requirements and delegate authority";
                        }

                        if (pendingCount > 5 || averageWaitTime > 48) // Threshold for reporting
                            bottlenecks.Add(bottleneck);
                    }
                }

                return bottlenecks.OrderByDescending(b => b.PendingCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error identifying workflow bottlenecks");
                return Enumerable.Empty<WorkflowBottleneckAnalysis>();
            }
        }

        public async Task<double> GetAverageCompletionTimeAsync(WorkflowStage stage, int? businessVerticalId = null)
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                
                var auditTrails = await _context.AuditTrails
                    .Where(at => at.OldValue == stage.ToString())
                    .Where(at => at.Action.Contains("Workflow advanced"))
                    .Where(at => at.ChangedDate >= thirtyDaysAgo)
                    .ToListAsync();

                if (businessVerticalId.HasValue)
                {
                    var workRequestIds = await _context.WorkRequests
                        .Where(wr => wr.BusinessVerticalId == businessVerticalId)
                        .Select(wr => wr.Id)
                        .ToListAsync();
                    
                    auditTrails = auditTrails.Where(at => workRequestIds.Contains(at.WorkRequestId)).ToList();
                }

                var stageTimes = auditTrails
                    .Select(at => ExtractTimeInStage(at.Metadata))
                    .Where(time => time > 0);

                return stageTimes.Any() ? stageTimes.Average() : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average completion time for stage {Stage}", stage);
                return 0;
            }
        }

        // Helper methods
        private async Task SendSLANotificationAsync(WorkRequest workRequest, SLAStatus slaStatus)
        {
            try
            {
                var subject = $"SLA {slaStatus.Status}: Work Request #{workRequest.Id}";
                var body = $@"
                    Work Request: {workRequest.Title}
                    Current Stage: {slaStatus.CurrentStage}
                    SLA Status: {slaStatus.Status}
                    Time Remaining: {slaStatus.TimeRemaining:dd\:hh\:mm}
                    Deadline: {slaStatus.SLADeadline:yyyy-MM-dd HH:mm}
                ";

                // Send to assigned user and stakeholders
                // This would need to be enhanced with proper recipient logic
                _logger.LogInformation("SLA notification sent for work request {WorkRequestId}: {Status}",
                    workRequest.Id, slaStatus.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SLA notification for work request {WorkRequestId}", workRequest.Id);
            }
        }

        private string RenderTemplate(string template, WorkRequest workRequest, WorkflowTransition transition, string eventType)
        {
            return template
                .Replace("{WorkRequestId}", workRequest.Id.ToString())
                .Replace("{Title}", workRequest.Title)
                .Replace("{Description}", workRequest.Description)
                .Replace("{Priority}", workRequest.Priority.ToString())
                .Replace("{Status}", workRequest.Status.ToString())
                .Replace("{CurrentStage}", workRequest.CurrentStage.ToString())
                .Replace("{TransitionName}", transition.TransitionName)
                .Replace("{EventType}", eventType)
                .Replace("{CreatedDate}", workRequest.CreatedDate.ToString("yyyy-MM-dd HH:mm"))
                .Replace("{ModifiedDate}", workRequest.ModifiedDate.ToString("yyyy-MM-dd HH:mm"));
        }

        private bool EvaluatePriorityRule(BusinessRuleScript rule, WorkRequest workRequest)
        {
            if (Enum.TryParse<PriorityLevel>(rule.Value, out var requiredPriority))
            {
                return rule.Operator.ToLower() switch
                {
                    "equals" => workRequest.PriorityLevel == requiredPriority,
                    "greaterthan" => workRequest.PriorityLevel > requiredPriority,
                    "lessthan" => workRequest.PriorityLevel < requiredPriority,
                    _ => true
                };
            }
            return true;
        }

        private bool EvaluateRoleRule(BusinessRuleScript rule, User user)
        {
            if (Enum.TryParse<UserRole>(rule.Value, out var requiredRole))
            {
                return rule.Operator.ToLower() switch
                {
                    "equals" => user.Role == requiredRole,
                    "greaterthanorequal" => user.Role >= requiredRole,
                    "lessthanorequal" => user.Role <= requiredRole,
                    _ => true
                };
            }
            return true;
        }

        private bool EvaluateTimeElapsedRule(BusinessRuleScript rule, WorkRequest workRequest)
        {
            if (int.TryParse(rule.Value, out var requiredHours))
            {
                var elapsed = DateTime.UtcNow - workRequest.ModifiedDate;
                return rule.Operator.ToLower() switch
                {
                    "greaterthan" => elapsed.TotalHours > requiredHours,
                    "lessthan" => elapsed.TotalHours < requiredHours,
                    _ => true
                };
            }
            return true;
        }

        private bool EvaluateBusinessVerticalRule(BusinessRuleScript rule, WorkRequest workRequest)
        {
            if (int.TryParse(rule.Value, out var requiredVerticalId))
            {
                return workRequest.BusinessVerticalId == requiredVerticalId;
            }
            return true;
        }

        private double ExtractTimeInStage(string? metadata)
        {
            if (string.IsNullOrEmpty(metadata)) return 0;

            try
            {
                var metadataObj = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
                if (metadataObj != null && metadataObj.ContainsKey("TimeInPreviousStageHours"))
                {
                    if (double.TryParse(metadataObj["TimeInPreviousStageHours"].ToString(), out var time))
                        return time;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting time in stage from metadata");
            }
            return 0;
        }
    }

    // Supporting classes
    public class BusinessRuleScript
    {
        public string Type { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
} 