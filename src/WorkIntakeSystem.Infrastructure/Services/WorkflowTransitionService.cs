using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public interface IWorkflowTransitionService
    {
        // CRUD Operations
        Task<WorkflowTransition> CreateTransitionAsync(WorkflowTransition transition);
        Task<WorkflowTransition?> GetTransitionAsync(int transitionId);
        Task<WorkflowTransition?> GetTransitionAsync(int fromStageId, int toStageId, int? businessVerticalId = null);
        Task<IEnumerable<WorkflowTransition>> GetAllTransitionsAsync(int? businessVerticalId = null);
        Task<IEnumerable<WorkflowTransition>> GetTransitionsFromStageAsync(int stageId);
        Task<IEnumerable<WorkflowTransition>> GetTransitionsToStageAsync(int stageId);
        Task<WorkflowTransition> UpdateTransitionAsync(WorkflowTransition transition);
        Task<bool> DeleteTransitionAsync(int transitionId);

        // Conditional Logic
        Task<bool> EvaluateConditionAsync(int transitionId, WorkRequest workRequest, int userId);
        Task<bool> SetConditionScriptAsync(int transitionId, string conditionScript);
        Task<string?> GetConditionScriptAsync(int transitionId);
        Task<bool> ValidateConditionScriptAsync(string conditionScript);

        // Auto-transition Capabilities
        Task<IEnumerable<WorkflowTransition>> GetAutoTransitionsAsync(int? businessVerticalId = null);
        Task<bool> SetAutoTransitionAsync(int transitionId, bool enabled, int? delayMinutes = null);
        Task<bool> ShouldAutoTransitionAsync(int transitionId, WorkRequest workRequest);
        Task ProcessScheduledTransitionsAsync();

        // Validation Rules
        Task<bool> SetValidationRulesAsync(int transitionId, TransitionValidationRules rules);
        Task<TransitionValidationRules?> GetValidationRulesAsync(int transitionId);
        Task<ValidationResult> ValidateTransitionAsync(int transitionId, WorkRequest workRequest, int userId);

        // Notification Management
        Task<bool> SetNotificationTemplateAsync(int transitionId, NotificationTemplate template);
        Task<NotificationTemplate?> GetNotificationTemplateAsync(int transitionId);
        Task<bool> ShouldSendNotificationAsync(int transitionId);

        // Business Logic
        Task<IEnumerable<WorkflowTransition>> GetAvailableTransitionsAsync(int fromStageId, int userId, int? businessVerticalId = null);
        Task<bool> CanUserExecuteTransitionAsync(int transitionId, int userId);
        Task<TransitionMetrics> GetTransitionMetricsAsync(int transitionId, DateTime fromDate, DateTime toDate);
    }

    public class WorkflowTransitionService : IWorkflowTransitionService
    {
        private readonly WorkIntakeDbContext _context;
        private readonly ILogger<WorkflowTransitionService> _logger;

        public WorkflowTransitionService(
            WorkIntakeDbContext context,
            ILogger<WorkflowTransitionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CRUD Operations
        public async Task<WorkflowTransition> CreateTransitionAsync(WorkflowTransition transition)
        {
            try
            {
                transition.CreatedDate = DateTime.UtcNow;
                transition.ModifiedDate = DateTime.UtcNow;
                transition.IsActive = true;

                _context.WorkflowTransitions.Add(transition);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created workflow transition {TransitionName} with ID {TransitionId}", 
                    transition.TransitionName, transition.Id);
                return transition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow transition {TransitionName}", transition.TransitionName);
                throw;
            }
        }

        public async Task<WorkflowTransition?> GetTransitionAsync(int transitionId)
        {
            try
            {
                return await _context.WorkflowTransitions
                    .Include(t => t.FromStage)
                    .Include(t => t.ToStage)
                    .Include(t => t.BusinessVertical)
                    .FirstOrDefaultAsync(t => t.Id == transitionId && t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow transition {TransitionId}", transitionId);
                return null;
            }
        }

        public async Task<WorkflowTransition?> GetTransitionAsync(int fromStageId, int toStageId, int? businessVerticalId = null)
        {
            try
            {
                var query = _context.WorkflowTransitions
                    .Include(t => t.FromStage)
                    .Include(t => t.ToStage)
                    .Include(t => t.BusinessVertical)
                    .Where(t => t.FromStageId == fromStageId && t.ToStageId == toStageId && t.IsActive);

                if (businessVerticalId.HasValue)
                    query = query.Where(t => t.BusinessVerticalId == businessVerticalId);
                else
                    query = query.Where(t => t.BusinessVerticalId == null);

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow transition from {FromStageId} to {ToStageId}", 
                    fromStageId, toStageId);
                return null;
            }
        }

        public async Task<IEnumerable<WorkflowTransition>> GetAllTransitionsAsync(int? businessVerticalId = null)
        {
            try
            {
                var query = _context.WorkflowTransitions
                    .Include(t => t.FromStage)
                    .Include(t => t.ToStage)
                    .Include(t => t.BusinessVertical)
                    .Where(t => t.IsActive);

                if (businessVerticalId.HasValue)
                    query = query.Where(t => t.BusinessVerticalId == businessVerticalId || t.BusinessVerticalId == null);

                return await query
                    .OrderBy(t => t.FromStage.Order)
                    .ThenBy(t => t.ToStage.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow transitions");
                return Enumerable.Empty<WorkflowTransition>();
            }
        }

        public async Task<IEnumerable<WorkflowTransition>> GetTransitionsFromStageAsync(int stageId)
        {
            try
            {
                return await _context.WorkflowTransitions
                    .Include(t => t.FromStage)
                    .Include(t => t.ToStage)
                    .Include(t => t.BusinessVertical)
                    .Where(t => t.FromStageId == stageId && t.IsActive)
                    .OrderBy(t => t.ToStage.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transitions from stage {StageId}", stageId);
                return Enumerable.Empty<WorkflowTransition>();
            }
        }

        public async Task<IEnumerable<WorkflowTransition>> GetTransitionsToStageAsync(int stageId)
        {
            try
            {
                return await _context.WorkflowTransitions
                    .Include(t => t.FromStage)
                    .Include(t => t.ToStage)
                    .Include(t => t.BusinessVertical)
                    .Where(t => t.ToStageId == stageId && t.IsActive)
                    .OrderBy(t => t.FromStage.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transitions to stage {StageId}", stageId);
                return Enumerable.Empty<WorkflowTransition>();
            }
        }

        public async Task<WorkflowTransition> UpdateTransitionAsync(WorkflowTransition transition)
        {
            try
            {
                transition.ModifiedDate = DateTime.UtcNow;
                _context.WorkflowTransitions.Update(transition);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated workflow transition {TransitionId}", transition.Id);
                return transition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow transition {TransitionId}", transition.Id);
                throw;
            }
        }

        public async Task<bool> DeleteTransitionAsync(int transitionId)
        {
            try
            {
                var transition = await _context.WorkflowTransitions.FindAsync(transitionId);
                if (transition == null) return false;

                // Soft delete
                transition.IsActive = false;
                transition.ModifiedDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted workflow transition {TransitionId}", transitionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow transition {TransitionId}", transitionId);
                return false;
            }
        }

        // Conditional Logic
        public async Task<bool> EvaluateConditionAsync(int transitionId, WorkRequest workRequest, int userId)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null) return false;

                var conditionScript = await GetConditionScriptAsync(transitionId);
                if (string.IsNullOrWhiteSpace(conditionScript)) return true; // No conditions = always allowed

                return await EvaluateConditionScriptAsync(conditionScript, workRequest, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating condition for transition {TransitionId}", transitionId);
                return false;
            }
        }

        public async Task<bool> SetConditionScriptAsync(int transitionId, string conditionScript)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null) return false;

                if (!await ValidateConditionScriptAsync(conditionScript))
                    return false;

                transition.ConditionScript = conditionScript;
                await UpdateTransitionAsync(transition);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting condition script for transition {TransitionId}", transitionId);
                return false;
            }
        }

        public async Task<string?> GetConditionScriptAsync(int transitionId)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                return transition?.ConditionScript;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting condition script for transition {TransitionId}", transitionId);
                return null;
            }
        }

        public async Task<bool> ValidateConditionScriptAsync(string conditionScript)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(conditionScript)) return await Task.FromResult(true);

                // Basic validation - check if it's valid JSON and contains expected structure
                var conditionObj = JsonSerializer.Deserialize<ConditionScript>(conditionScript);
                return await Task.FromResult(conditionObj != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating condition script");
                return await Task.FromResult(false);
            }
        }

        // Auto-transition Capabilities
        public async Task<IEnumerable<WorkflowTransition>> GetAutoTransitionsAsync(int? businessVerticalId = null)
        {
            try
            {
                var query = _context.WorkflowTransitions
                    .Include(t => t.FromStage)
                    .Include(t => t.ToStage)
                    .Where(t => t.IsActive && t.AutoTransitionDelayMinutes.HasValue);

                if (businessVerticalId.HasValue)
                    query = query.Where(t => t.BusinessVerticalId == businessVerticalId);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving auto-transitions");
                return Enumerable.Empty<WorkflowTransition>();
            }
        }

        public async Task<bool> SetAutoTransitionAsync(int transitionId, bool enabled, int? delayMinutes = null)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null) return false;

                transition.AutoTransitionDelayMinutes = enabled ? delayMinutes : null;
                await UpdateTransitionAsync(transition);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting auto-transition for transition {TransitionId}", transitionId);
                return false;
            }
        }

        public async Task<bool> ShouldAutoTransitionAsync(int transitionId, WorkRequest workRequest)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null || !transition.AutoTransitionDelayMinutes.HasValue) return false;

                // Check if enough time has passed since entering the current stage
                var timeInStage = DateTime.UtcNow - workRequest.ModifiedDate;
                var requiredDelay = TimeSpan.FromMinutes(transition.AutoTransitionDelayMinutes.Value);

                return timeInStage >= requiredDelay;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking auto-transition for transition {TransitionId}", transitionId);
                return false;
            }
        }

        public async Task ProcessScheduledTransitionsAsync()
        {
            try
            {
                var autoTransitions = await GetAutoTransitionsAsync();
                var processedCount = 0;

                foreach (var transition in autoTransitions)
                {
                    var workRequests = await _context.WorkRequests
                        .Where(wr => (int)wr.CurrentStage == transition.FromStage.Order)
                        .ToListAsync();

                    foreach (var workRequest in workRequests)
                    {
                        if (await ShouldAutoTransitionAsync(transition.Id, workRequest))
                        {
                            // Execute auto-transition
                            var targetStage = (WorkflowStage)transition.ToStage.Order;
                            // Note: This would require access to the workflow engine
                            // For now, we'll log the intent
                            _logger.LogInformation("Auto-transition ready for WorkRequest {WorkRequestId} from {FromStage} to {ToStage}",
                                workRequest.Id, transition.FromStage.Name, transition.ToStage.Name);
                            processedCount++;
                        }
                    }
                }

                _logger.LogInformation("Processed {Count} auto-transitions", processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled transitions");
            }
        }

        // Validation Rules
        public async Task<bool> SetValidationRulesAsync(int transitionId, TransitionValidationRules rules)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null) return false;

                transition.ValidationRules = JsonSerializer.Serialize(rules);
                await UpdateTransitionAsync(transition);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting validation rules for transition {TransitionId}", transitionId);
                return false;
            }
        }

        public async Task<TransitionValidationRules?> GetValidationRulesAsync(int transitionId)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null || string.IsNullOrEmpty(transition.ValidationRules)) return null;

                return JsonSerializer.Deserialize<TransitionValidationRules>(transition.ValidationRules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting validation rules for transition {TransitionId}", transitionId);
                return null;
            }
        }

        public async Task<ValidationResult> ValidateTransitionAsync(int transitionId, WorkRequest workRequest, int userId)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                var rules = await GetValidationRulesAsync(transitionId);
                if (rules == null) return result;

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("User not found");
                    return result;
                }

                // Validate required fields
                foreach (var fieldRule in rules.RequiredFields)
                {
                    if (!ValidateRequiredField(workRequest, fieldRule))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Field {fieldRule.FieldName} is required");
                    }
                }

                // Validate business rules
                foreach (var businessRule in rules.BusinessRules)
                {
                    if (!await EvaluateBusinessRuleAsync(businessRule, workRequest, user))
                    {
                        if (businessRule.IsWarning)
                            result.Warnings.Add(businessRule.ErrorMessage);
                        else
                        {
                            result.IsValid = false;
                            result.Errors.Add(businessRule.ErrorMessage);
                        }
                    }
                }

                // Validate role requirements
                if (!string.IsNullOrEmpty(rules.RequiredRole))
                {
                    if (Enum.TryParse<UserRole>(rules.RequiredRole, out var requiredRole))
                    {
                        if (user.Role < requiredRole)
                        {
                            result.IsValid = false;
                            result.Errors.Add($"User role {user.Role} is insufficient. Required: {requiredRole}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transition {TransitionId}", transitionId);
                result.IsValid = false;
                result.Errors.Add("Validation error occurred");
            }

            return result;
        }

        // Notification Management
        public async Task<bool> SetNotificationTemplateAsync(int transitionId, NotificationTemplate template)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null) return false;

                transition.NotificationTemplate = JsonSerializer.Serialize(template);
                transition.NotificationRequired = true;
                await UpdateTransitionAsync(transition);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting notification template for transition {TransitionId}", transitionId);
                return false;
            }
        }

        public async Task<NotificationTemplate?> GetNotificationTemplateAsync(int transitionId)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null || string.IsNullOrEmpty(transition.NotificationTemplate)) return null;

                return JsonSerializer.Deserialize<NotificationTemplate>(transition.NotificationTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template for transition {TransitionId}", transitionId);
                return null;
            }
        }

        public async Task<bool> ShouldSendNotificationAsync(int transitionId)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                return transition?.NotificationRequired ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking notification requirement for transition {TransitionId}", transitionId);
                return false;
            }
        }

        // Business Logic
        public async Task<IEnumerable<WorkflowTransition>> GetAvailableTransitionsAsync(int fromStageId, int userId, int? businessVerticalId = null)
        {
            try
            {
                var transitions = await GetTransitionsFromStageAsync(fromStageId);
                var availableTransitions = new List<WorkflowTransition>();

                foreach (var transition in transitions)
                {
                    if (await CanUserExecuteTransitionAsync(transition.Id, userId))
                        availableTransitions.Add(transition);
                }

                return availableTransitions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available transitions from stage {StageId}", fromStageId);
                return Enumerable.Empty<WorkflowTransition>();
            }
        }

        public async Task<bool> CanUserExecuteTransitionAsync(int transitionId, int userId)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null) return false;

                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Check role requirements
                if (!string.IsNullOrEmpty(transition.RequiredRole))
                {
                    if (Enum.TryParse<UserRole>(transition.RequiredRole, out var requiredRole))
                    {
                        if (user.Role < requiredRole) return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user permission for transition {TransitionId}", transitionId);
                return false;
            }
        }

        public async Task<TransitionMetrics> GetTransitionMetricsAsync(int transitionId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var transition = await GetTransitionAsync(transitionId);
                if (transition == null) return new TransitionMetrics();

                var auditTrails = await _context.AuditTrails
                    .Where(at => at.ChangedDate >= fromDate && at.ChangedDate <= toDate)
                    .Where(at => at.Action.Contains($"{transition.FromStage.Name} -> {transition.ToStage.Name}"))
                    .ToListAsync();

                return new TransitionMetrics
                {
                    TransitionId = transitionId,
                    TransitionName = transition.TransitionName,
                    ExecutionCount = auditTrails.Count,
                    AverageExecutionTime = CalculateAverageExecutionTime(auditTrails),
                    FromDate = fromDate,
                    ToDate = toDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transition metrics for {TransitionId}", transitionId);
                return new TransitionMetrics();
            }
        }

        // Helper methods
        private async Task<bool> EvaluateConditionScriptAsync(string conditionScript, WorkRequest workRequest, int userId)
        {
            try
            {
                var condition = JsonSerializer.Deserialize<ConditionScript>(conditionScript);
                if (condition == null) return true;

                // Evaluate each condition
                foreach (var rule in condition.Rules)
                {
                    if (!await EvaluateConditionRuleAsync(rule, workRequest, userId))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating condition script");
                return false;
            }
        }

        private async Task<bool> EvaluateConditionRuleAsync(ConditionRule rule, WorkRequest workRequest, int userId)
        {
            try
            {
                switch (rule.Type.ToLower())
                {
                    case "priority":
                        return EvaluatePriorityCondition(rule, workRequest);
                    case "role":
                        return await EvaluateRoleCondition(rule, userId);
                    case "businessvertical":
                        return EvaluateBusinessVerticalCondition(rule, workRequest);
                    case "timeelapsed":
                        return EvaluateTimeElapsedCondition(rule, workRequest);
                    default:
                        return true; // Unknown conditions pass by default
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating condition rule {RuleType}", rule.Type);
                return false;
            }
        }

        private bool EvaluatePriorityCondition(ConditionRule rule, WorkRequest workRequest)
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

        private async Task<bool> EvaluateRoleCondition(ConditionRule rule, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

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

        private bool EvaluateBusinessVerticalCondition(ConditionRule rule, WorkRequest workRequest)
        {
            if (int.TryParse(rule.Value, out var requiredVerticalId))
            {
                return workRequest.BusinessVerticalId == requiredVerticalId;
            }
            return true;
        }

        private bool EvaluateTimeElapsedCondition(ConditionRule rule, WorkRequest workRequest)
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

        private bool ValidateRequiredField(WorkRequest workRequest, RequiredField field)
        {
            return field.FieldName.ToLower() switch
            {
                "title" => !string.IsNullOrWhiteSpace(workRequest.Title),
                "description" => !string.IsNullOrWhiteSpace(workRequest.Description),
                "businessverticalid" => workRequest.BusinessVerticalId > 0,
                _ => true // Unknown fields pass validation
            };
        }

        private async Task<bool> EvaluateBusinessRuleAsync(BusinessRule rule, WorkRequest workRequest, User user)
        {
            // Simple rule evaluation - can be extended with a proper rules engine
            try
            {
                return rule.Name.ToLower() switch
                {
                    "requireapproval" => await CheckApprovalRequirement(workRequest, user),
                    "checkbudget" => await CheckBudgetRequirement(workRequest),
                    "validatecompliance" => await CheckComplianceRequirement(workRequest),
                    _ => true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating business rule {RuleName}", rule.Name);
                return false;
            }
        }

        private async Task<bool> CheckApprovalRequirement(WorkRequest workRequest, User user)
        {
            // Check if approval is required based on business logic
            return user.Role >= UserRole.DepartmentHead || workRequest.PriorityLevel <= PriorityLevel.Medium;
        }

        private async Task<bool> CheckBudgetRequirement(WorkRequest workRequest)
        {
            // Placeholder for budget validation
            return true;
        }

        private async Task<bool> CheckComplianceRequirement(WorkRequest workRequest)
        {
            // Placeholder for compliance validation
            return true;
        }

        private double CalculateAverageExecutionTime(List<AuditTrail> auditTrails)
        {
            if (!auditTrails.Any()) return 0;

            var executionTimes = auditTrails
                .Where(at => !string.IsNullOrEmpty(at.Metadata))
                .Select(at => ExtractExecutionTime(at.Metadata))
                .Where(time => time > 0);

            return executionTimes.Any() ? executionTimes.Average() : 0;
        }

        private double ExtractExecutionTime(string metadata)
        {
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
                _logger.LogError(ex, "Error extracting execution time from metadata");
            }
            return 0;
        }
    }

    // Supporting classes
    public class TransitionValidationRules
    {
        public List<RequiredField> RequiredFields { get; set; } = new();
        public List<BusinessRule> BusinessRules { get; set; } = new();
        public string? RequiredRole { get; set; }
        public bool RequireComments { get; set; } = false;
        public int? MinCommentLength { get; set; }
    }

    public class RequiredField
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
    }

    public class ConditionScript
    {
        public List<ConditionRule> Rules { get; set; } = new();
        public string Logic { get; set; } = "AND"; // AND, OR
    }

    public class ConditionRule
    {
        public string Type { get; set; } = string.Empty; // Priority, Role, BusinessVertical, TimeElapsed
        public string Operator { get; set; } = string.Empty; // Equals, GreaterThan, LessThan, etc.
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TransitionMetrics
    {
        public int TransitionId { get; set; }
        public string TransitionName { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public double AverageExecutionTime { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
} 