using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Services;
using WorkIntakeSystem.Infrastructure.Data;
using WorkIntakeSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
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
            var mockStageService = new Mock<IWorkflowStageConfigurationService>();
            var mockTransitionService = new Mock<IWorkflowTransitionService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<WorkflowEngine>>();
            
            var engine = new WorkflowEngine(db, mockStageService.Object, mockTransitionService.Object, mockEmailService.Object, mockLogger.Object);
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
            var mockStageService = new Mock<IWorkflowStageConfigurationService>();
            var mockTransitionService = new Mock<IWorkflowTransitionService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<WorkflowEngine>>();
            
            // Configure mock stage service
            var intakeStage = new WorkflowStageConfiguration { Id = 1, Order = (int)WorkflowStage.Intake, Name = "Intake" };
            var businessReviewStage = new WorkflowStageConfiguration { Id = 2, Order = (int)WorkflowStage.BusinessReview, Name = "Business Review" };
            mockStageService.Setup(x => x.GetStageByOrderAsync((int)WorkflowStage.Intake, null)).ReturnsAsync(intakeStage);
            mockStageService.Setup(x => x.GetStageByOrderAsync((int)WorkflowStage.BusinessReview, null)).ReturnsAsync(businessReviewStage);
            
            // Configure mock transition service
            var transition = new WorkflowTransition 
            { 
                Id = 1, 
                FromStage = intakeStage, 
                ToStage = businessReviewStage,
                IsActive = true
            };
            mockTransitionService.Setup(x => x.GetTransitionAsync(1, 2, null)).ReturnsAsync(transition);
            mockTransitionService.Setup(x => x.CanUserExecuteTransitionAsync(1, 2)).ReturnsAsync(true);
            mockTransitionService.Setup(x => x.EvaluateConditionAsync(1, It.IsAny<WorkRequest>(), 2)).ReturnsAsync(true);
            mockTransitionService.Setup(x => x.ValidateTransitionAsync(1, It.IsAny<WorkRequest>(), 2))
                .ReturnsAsync(new ValidationResult { IsValid = true, Errors = new List<string>() });
            
            var engine = new WorkflowEngine(db, mockStageService.Object, mockTransitionService.Object, mockEmailService.Object, mockLogger.Object);
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