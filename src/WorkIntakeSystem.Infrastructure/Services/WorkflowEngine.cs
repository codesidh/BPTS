using System;
using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            if ((int)nextStage <= (int)workRequest.CurrentStage)
                return false;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            if (nextStage > WorkflowStage.BusinessReview && user.Role < UserRole.DepartmentHead)
                return false;
            return true;
        }

        public async Task AdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId, string? comments = null)
        {
            if (!await CanAdvanceAsync(workRequest, nextStage, userId))
                throw new InvalidOperationException("Transition not allowed");
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
                Comments = comments ?? string.Empty
            };
            _context.AuditTrails.Add(audit);
            var evt = new EventStore
            {
                AggregateId = workRequest.Id.ToString(),
                EventType = "WorkflowStageChanged",
                EventData = $"{{\"from\":\"{oldStage}\",\"to\":\"{nextStage}\"}}",
                CreatedBy = userId.ToString(),
                Timestamp = DateTime.UtcNow
            };
            _context.EventStore.Add(evt);
            await _context.SaveChangesAsync();
        }
    }
} 