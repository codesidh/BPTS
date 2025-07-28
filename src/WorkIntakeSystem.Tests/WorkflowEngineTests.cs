using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Services;
using WorkIntakeSystem.Infrastructure.Data;
using Xunit;

namespace WorkIntakeSystem.Tests
{
    public class WorkflowEngineTests
    {
        private WorkIntakeDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<WorkIntakeDbContext>()
                .UseInMemoryDatabase(databaseName: "WorkflowEngineTests")
                .Options;
            return new WorkIntakeDbContext(options);
        }

        [Fact]
        public async Task CanAdvanceAsync_DeniesBackwardTransition()
        {
            var db = GetDbContext();
            var engine = new WorkflowEngine(db);
            var wr = new WorkRequest { Id = 1, CurrentStage = WorkflowStage.BusinessReview };
            db.Users.Add(new User { Id = 1, Role = UserRole.SystemAdministrator });
            db.SaveChanges();
            var canAdvance = await engine.CanAdvanceAsync(wr, WorkflowStage.Intake, 1);
            Assert.False(canAdvance);
        }

        [Fact]
        public async Task AdvanceAsync_LogsAuditAndEvent()
        {
            var db = GetDbContext();
            var engine = new WorkflowEngine(db);
            var wr = new WorkRequest { Id = 2, CurrentStage = WorkflowStage.Intake };
            db.Users.Add(new User { Id = 2, Role = UserRole.SystemAdministrator });
            db.WorkRequests.Add(wr);
            db.SaveChanges();
            await engine.AdvanceAsync(wr, WorkflowStage.BusinessReview, 2, "Test advance");
            Assert.Equal(WorkflowStage.BusinessReview, wr.CurrentStage);
            Assert.Contains(db.AuditTrails, a => a.WorkRequestId == wr.Id && a.Action.Contains("Workflow advanced"));
            Assert.Contains(db.EventStore, e => e.AggregateId == wr.Id.ToString() && e.EventType == "WorkflowStageChanged");
        }
    }
} 