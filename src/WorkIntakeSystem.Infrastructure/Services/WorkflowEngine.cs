using System;
using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly WorkIntakeDbContext _context;

        public WorkflowEngine(WorkIntakeDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanAdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId)
        {
            // Load user
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Prevent backward transitions by default
            if ((int)nextStage <= (int)workRequest.CurrentStage)
                return false;

            // Load stage configurations using enum order mapping
            var fromStageConfig = await _context.WorkflowStages.FirstOrDefaultAsync(s => s.Order == (int)workRequest.CurrentStage && s.IsActive);
            var toStageConfig = await _context.WorkflowStages.FirstOrDefaultAsync(s => s.Order == (int)nextStage && s.IsActive);
            if (fromStageConfig == null || toStageConfig == null)
            {
                // No advanced workflow configuration defined – fallback to basic checks
                return nextStage > workRequest.CurrentStage;
            }

            // Ensure there is an active transition configured
            var transition = await _context.WorkflowTransitions
                .FirstOrDefaultAsync(t => t.FromStageId == fromStageConfig.Id && t.ToStageId == toStageConfig.Id && t.IsActive);
            if (transition == null)
            {
                // If no explicit transition, allow by default (config optional)
                return true;
            }

            // Role based check – if transition requires a role, ensure user role meets it (>=)
            if (!string.IsNullOrWhiteSpace(transition.RequiredRole))
            {
                if (!Enum.TryParse<UserRole>(transition.RequiredRole, out var requiredRole))
                    return false;
                if (user.Role < requiredRole)
                    return false;
            }

            // Additional simple conditional check (placeholder)
            // If ConditionScript contains "RequiresApproval" then ensure ApprovalRequired false or user role high
            if (!string.IsNullOrWhiteSpace(transition.ConditionScript) && transition.ConditionScript.Contains("RequiresApproval", StringComparison.OrdinalIgnoreCase))
            {
                // For now, allow only DepartmentHead+ to satisfy approval
                if (user.Role < UserRole.DepartmentHead)
                    return false;
            }

            return true;
        }

        public async Task AdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId, string? comments = null)
        {
            if (!await CanAdvanceAsync(workRequest, nextStage, userId))
                throw new InvalidOperationException("Transition not allowed");

            // SLA tracking – compute time spent in stage and store in AuditTrail metadata (simple implementation)
            var timeInStage = DateTime.UtcNow - (workRequest.ModifiedDate ?? workRequest.CreatedDate);

            var oldStage = workRequest.CurrentStage;
            workRequest.CurrentStage = nextStage;
            workRequest.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var audit = new AuditTrail
            {
                WorkRequestId = workRequest.Id,
                Action = $"Workflow advanced: {oldStage} -> {nextStage}",
                OldValue = oldStage.ToString(),
                NewValue = nextStage.ToString(),
                ChangedById = userId,
                ChangedDate = DateTime.UtcNow,
                Comments = comments ?? string.Empty,
                Metadata = JsonSerializer.Serialize(new { TimeInPreviousStageHours = timeInStage.TotalHours })
            };
            _context.AuditTrails.Add(audit);

            var evt = new EventStore
            {
                AggregateId = workRequest.Id.ToString(),
                EventType = "WorkflowStageChanged",
                EventData = JsonSerializer.Serialize(new { from = oldStage.ToString(), to = nextStage.ToString() }),
                CreatedBy = userId.ToString(),
                Timestamp = DateTime.UtcNow
            };
            _context.EventStore.Add(evt);
            await _context.SaveChangesAsync();
        }
    }
} 