using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public interface IWorkflowStageConfigurationService
    {
        // CRUD Operations
        Task<WorkflowStageConfiguration> CreateStageAsync(WorkflowStageConfiguration stage);
        Task<WorkflowStageConfiguration?> GetStageAsync(int stageId);
        Task<WorkflowStageConfiguration?> GetStageByOrderAsync(int order, int? businessVerticalId = null);
        Task<IEnumerable<WorkflowStageConfiguration>> GetAllStagesAsync(int? businessVerticalId = null);
        Task<WorkflowStageConfiguration> UpdateStageAsync(WorkflowStageConfiguration stage);
        Task<bool> DeleteStageAsync(int stageId);

        // Role-based configuration
        Task<bool> AddRequiredRoleAsync(int stageId, UserRole role);
        Task<bool> RemoveRequiredRoleAsync(int stageId, UserRole role);
        Task<IEnumerable<UserRole>> GetRequiredRolesAsync(int stageId);
        Task<bool> CanUserAccessStageAsync(int stageId, int userId);

        // SLA Configuration
        Task<bool> SetSLAAsync(int stageId, int slaHours);
        Task<int?> GetSLAAsync(int stageId);
        Task<IEnumerable<WorkflowStageConfiguration>> GetStagesWithSLAAsync(int? businessVerticalId = null);

        // Notification Templates
        Task<bool> SetNotificationTemplateAsync(int stageId, NotificationTemplate template);
        Task<NotificationTemplate?> GetNotificationTemplateAsync(int stageId);
        Task<bool> TestNotificationTemplateAsync(int stageId, WorkRequest sampleWorkRequest);

        // Validation and Business Logic
        Task<bool> ValidateStageConfigurationAsync(WorkflowStageConfiguration stage);
        Task<IEnumerable<string>> GetValidationErrorsAsync(WorkflowStageConfiguration stage);
        Task<bool> IsStageOrderValidAsync(int order, int? businessVerticalId = null, int? excludeStageId = null);

        // Advanced Configuration
        Task<bool> SetAutoTransitionAsync(int stageId, bool enabled, int? delayMinutes = null);
        Task<bool> SetValidationRulesAsync(int stageId, ValidationRules rules);
        Task<ValidationRules?> GetValidationRulesAsync(int stageId);
    }

    public class WorkflowStageConfigurationService : IWorkflowStageConfigurationService
    {
        private readonly WorkIntakeDbContext _context;
        private readonly ILogger<WorkflowStageConfigurationService> _logger;

        public WorkflowStageConfigurationService(
            WorkIntakeDbContext context,
            ILogger<WorkflowStageConfigurationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CRUD Operations
        public async Task<WorkflowStageConfiguration> CreateStageAsync(WorkflowStageConfiguration stage)
        {
            try
            {
                if (!await ValidateStageConfigurationAsync(stage))
                    throw new InvalidOperationException("Stage configuration is invalid");

                stage.CreatedDate = DateTime.UtcNow;
                stage.ModifiedDate = DateTime.UtcNow;
                stage.IsActive = true;

                _context.WorkflowStages.Add(stage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created workflow stage configuration {StageName} with ID {StageId}", stage.Name, stage.Id);
                return stage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow stage configuration {StageName}", stage.Name);
                throw;
            }
        }

        public async Task<WorkflowStageConfiguration?> GetStageAsync(int stageId)
        {
            try
            {
                return await _context.WorkflowStages
                    .Include(s => s.BusinessVertical)
                    .Include(s => s.FromTransitions)
                    .Include(s => s.ToTransitions)
                    .FirstOrDefaultAsync(s => s.Id == stageId && s.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow stage {StageId}", stageId);
                return null;
            }
        }

        public async Task<WorkflowStageConfiguration?> GetStageByOrderAsync(int order, int? businessVerticalId = null)
        {
            try
            {
                var query = _context.WorkflowStages
                    .Include(s => s.BusinessVertical)
                    .Where(s => s.Order == order && s.IsActive);

                if (businessVerticalId.HasValue)
                    query = query.Where(s => s.BusinessVerticalId == businessVerticalId);
                else
                    query = query.Where(s => s.BusinessVerticalId == null);

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow stage by order {Order}", order);
                return null;
            }
        }

        public async Task<IEnumerable<WorkflowStageConfiguration>> GetAllStagesAsync(int? businessVerticalId = null)
        {
            try
            {
                var query = _context.WorkflowStages
                    .Include(s => s.BusinessVertical)
                    .Where(s => s.IsActive);

                if (businessVerticalId.HasValue)
                    query = query.Where(s => s.BusinessVerticalId == businessVerticalId || s.BusinessVerticalId == null);

                return await query
                    .OrderBy(s => s.Order)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow stages for business vertical {BusinessVerticalId}", businessVerticalId);
                return Enumerable.Empty<WorkflowStageConfiguration>();
            }
        }

        public async Task<WorkflowStageConfiguration> UpdateStageAsync(WorkflowStageConfiguration stage)
        {
            try
            {
                if (!await ValidateStageConfigurationAsync(stage))
                    throw new InvalidOperationException("Stage configuration is invalid");

                stage.ModifiedDate = DateTime.UtcNow;
                
                // Add to change history
                var changeHistory = JsonSerializer.Deserialize<List<StageChangeRecord>>(stage.ChangeHistory) ?? new List<StageChangeRecord>();
                changeHistory.Add(new StageChangeRecord
                {
                    ChangeDate = DateTime.UtcNow,
                    ChangedBy = "System", // Should be passed from context
                    ChangeType = "Update",
                    Description = "Stage configuration updated"
                });
                stage.ChangeHistory = JsonSerializer.Serialize(changeHistory);

                _context.WorkflowStages.Update(stage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated workflow stage configuration {StageName} with ID {StageId}", stage.Name, stage.Id);
                return stage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow stage configuration {StageId}", stage.Id);
                throw;
            }
        }

        public async Task<bool> DeleteStageAsync(int stageId)
        {
            try
            {
                var stage = await _context.WorkflowStages.FindAsync(stageId);
                if (stage == null) return false;

                // Soft delete
                stage.IsActive = false;
                stage.ModifiedDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted workflow stage configuration {StageId}", stageId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow stage configuration {StageId}", stageId);
                return false;
            }
        }

        // Role-based configuration
        public async Task<bool> AddRequiredRoleAsync(int stageId, UserRole role)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null) return false;

                var roles = JsonSerializer.Deserialize<List<string>>(stage.RequiredRoles) ?? new List<string>();
                var roleString = role.ToString();
                
                if (!roles.Contains(roleString))
                {
                    roles.Add(roleString);
                    stage.RequiredRoles = JsonSerializer.Serialize(roles);
                    await UpdateStageAsync(stage);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding required role {Role} to stage {StageId}", role, stageId);
                return false;
            }
        }

        public async Task<bool> RemoveRequiredRoleAsync(int stageId, UserRole role)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null) return false;

                var roles = JsonSerializer.Deserialize<List<string>>(stage.RequiredRoles) ?? new List<string>();
                var roleString = role.ToString();
                
                if (roles.Remove(roleString))
                {
                    stage.RequiredRoles = JsonSerializer.Serialize(roles);
                    await UpdateStageAsync(stage);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing required role {Role} from stage {StageId}", role, stageId);
                return false;
            }
        }

        public async Task<IEnumerable<UserRole>> GetRequiredRolesAsync(int stageId)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null) return Enumerable.Empty<UserRole>();

                var roleStrings = JsonSerializer.Deserialize<List<string>>(stage.RequiredRoles) ?? new List<string>();
                var roles = new List<UserRole>();

                foreach (var roleString in roleStrings)
                {
                    if (Enum.TryParse<UserRole>(roleString, out var role))
                        roles.Add(role);
                }

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting required roles for stage {StageId}", stageId);
                return Enumerable.Empty<UserRole>();
            }
        }

        public async Task<bool> CanUserAccessStageAsync(int stageId, int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                var requiredRoles = await GetRequiredRolesAsync(stageId);
                if (!requiredRoles.Any()) return true; // No role restrictions

                return requiredRoles.Any(role => user.Role >= role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user access for stage {StageId} and user {UserId}", stageId, userId);
                return false;
            }
        }

        // SLA Configuration
        public async Task<bool> SetSLAAsync(int stageId, int slaHours)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null) return false;

                stage.SLAHours = slaHours;
                await UpdateStageAsync(stage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting SLA for stage {StageId}", stageId);
                return false;
            }
        }

        public async Task<int?> GetSLAAsync(int stageId)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                return stage?.SLAHours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SLA for stage {StageId}", stageId);
                return null;
            }
        }

        public async Task<IEnumerable<WorkflowStageConfiguration>> GetStagesWithSLAAsync(int? businessVerticalId = null)
        {
            try
            {
                var stages = await GetAllStagesAsync(businessVerticalId);
                return stages.Where(s => s.SLAHours.HasValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stages with SLA");
                return Enumerable.Empty<WorkflowStageConfiguration>();
            }
        }

        // Notification Templates
        public async Task<bool> SetNotificationTemplateAsync(int stageId, NotificationTemplate template)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null) return false;

                stage.NotificationTemplate = JsonSerializer.Serialize(template);
                await UpdateStageAsync(stage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting notification template for stage {StageId}", stageId);
                return false;
            }
        }

        public async Task<NotificationTemplate?> GetNotificationTemplateAsync(int stageId)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null || string.IsNullOrEmpty(stage.NotificationTemplate)) return null;

                return JsonSerializer.Deserialize<NotificationTemplate>(stage.NotificationTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template for stage {StageId}", stageId);
                return null;
            }
        }

        public async Task<bool> TestNotificationTemplateAsync(int stageId, WorkRequest sampleWorkRequest)
        {
            try
            {
                var template = await GetNotificationTemplateAsync(stageId);
                if (template == null) return false;

                // Test template rendering with sample data
                var renderedSubject = RenderTemplate(template.Subject, sampleWorkRequest);
                var renderedBody = RenderTemplate(template.Body, sampleWorkRequest);

                return !string.IsNullOrEmpty(renderedSubject) && !string.IsNullOrEmpty(renderedBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing notification template for stage {StageId}", stageId);
                return false;
            }
        }

        // Validation and Business Logic
        public async Task<bool> ValidateStageConfigurationAsync(WorkflowStageConfiguration stage)
        {
            var errors = await GetValidationErrorsAsync(stage);
            return !errors.Any();
        }

        public async Task<IEnumerable<string>> GetValidationErrorsAsync(WorkflowStageConfiguration stage)
        {
            var errors = new List<string>();

            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(stage.Name))
                    errors.Add("Stage name is required");

                if (stage.Order < 0)
                    errors.Add("Stage order must be non-negative");

                // Check for duplicate order
                if (!await IsStageOrderValidAsync(stage.Order, stage.BusinessVerticalId, stage.Id))
                    errors.Add($"Stage order {stage.Order} is already in use");

                // Validate SLA
                if (stage.SLAHours.HasValue && stage.SLAHours.Value <= 0)
                    errors.Add("SLA hours must be positive");

                // Validate JSON fields
                if (!IsValidJson(stage.RequiredRoles))
                    errors.Add("Required roles must be valid JSON");

                if (!IsValidJson(stage.NotificationTemplate))
                    errors.Add("Notification template must be valid JSON");

                if (!IsValidJson(stage.AllowedTransitions))
                    errors.Add("Allowed transitions must be valid JSON");

                if (!IsValidJson(stage.ValidationRules))
                    errors.Add("Validation rules must be valid JSON");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating stage configuration");
                errors.Add("Validation error occurred");
            }

            return errors;
        }

        public async Task<bool> IsStageOrderValidAsync(int order, int? businessVerticalId = null, int? excludeStageId = null)
        {
            try
            {
                var query = _context.WorkflowStages
                    .Where(s => s.Order == order && s.IsActive);

                if (businessVerticalId.HasValue)
                    query = query.Where(s => s.BusinessVerticalId == businessVerticalId);
                else
                    query = query.Where(s => s.BusinessVerticalId == null);

                if (excludeStageId.HasValue)
                    query = query.Where(s => s.Id != excludeStageId.Value);

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating stage order {Order}", order);
                return false;
            }
        }

        // Advanced Configuration
        public async Task<bool> SetAutoTransitionAsync(int stageId, bool enabled, int? delayMinutes = null)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null) return false;

                stage.AutoTransition = enabled;
                // Note: Auto-transition delay is handled at the transition level
                await UpdateStageAsync(stage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting auto-transition for stage {StageId}", stageId);
                return false;
            }
        }

        public async Task<bool> SetValidationRulesAsync(int stageId, ValidationRules rules)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null) return false;

                stage.ValidationRules = JsonSerializer.Serialize(rules);
                await UpdateStageAsync(stage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting validation rules for stage {StageId}", stageId);
                return false;
            }
        }

        public async Task<ValidationRules?> GetValidationRulesAsync(int stageId)
        {
            try
            {
                var stage = await GetStageAsync(stageId);
                if (stage == null || string.IsNullOrEmpty(stage.ValidationRules)) return null;

                return JsonSerializer.Deserialize<ValidationRules>(stage.ValidationRules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting validation rules for stage {StageId}", stageId);
                return null;
            }
        }

        // Helper methods
        private bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return true;
            
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string RenderTemplate(string template, WorkRequest workRequest)
        {
            // Simple template rendering - replace placeholders
            return template
                .Replace("{WorkRequestId}", workRequest.Id.ToString())
                .Replace("{Title}", workRequest.Title)
                .Replace("{Description}", workRequest.Description)
                .Replace("{Priority}", workRequest.Priority.ToString())
                .Replace("{Status}", workRequest.Status.ToString())
                .Replace("{CurrentStage}", workRequest.CurrentStage.ToString())
                .Replace("{CreatedDate}", workRequest.CreatedDate.ToString("yyyy-MM-dd HH:mm"))
                .Replace("{ModifiedDate}", workRequest.ModifiedDate.ToString("yyyy-MM-dd HH:mm"));
        }
    }

    // Supporting classes
    public class NotificationTemplate
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new();
        public string Priority { get; set; } = "Normal"; // Low, Normal, High
        public bool SendEmail { get; set; } = true;
        public bool SendInApp { get; set; } = true;
    }

    public class ValidationRules
    {
        public List<FieldValidation> FieldValidations { get; set; } = new();
        public List<BusinessRule> BusinessRules { get; set; } = new();
        public bool RequireComments { get; set; } = false;
        public int? MinCommentLength { get; set; }
    }

    public class FieldValidation
    {
        public string FieldName { get; set; } = string.Empty;
        public bool Required { get; set; } = false;
        public string DataType { get; set; } = "String";
        public string? Pattern { get; set; }
        public object? MinValue { get; set; }
        public object? MaxValue { get; set; }
    }

    public class BusinessRule
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsWarning { get; set; } = false;
    }

    public class StageChangeRecord
    {
        public DateTime ChangeDate { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
} 