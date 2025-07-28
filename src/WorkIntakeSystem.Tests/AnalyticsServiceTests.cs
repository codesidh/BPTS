using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Services;
using WorkIntakeSystem.Infrastructure.Data;
using Xunit;

namespace WorkIntakeSystem.Tests
{
    public class AnalyticsServiceTests
    {
        private WorkIntakeDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<WorkIntakeDbContext>()
                .UseInMemoryDatabase(databaseName: $"AnalyticsServiceTests_{Guid.NewGuid()}")
                .Options;
            return new WorkIntakeDbContext(options);
        }

        [Fact]
        public async Task GetDashboardAnalyticsAsync_ReturnsCorrectMetrics()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            // Add test data
            var department = new Department { Id = 1, Name = "IT", ResourceCapacity = 10, CurrentUtilization = 75 };
            var businessVertical = new BusinessVertical { Id = 1, Name = "Medicaid" };
            var user = new User { Id = 1, Name = "Test User", DepartmentId = 1 };

            db.Departments.Add(department);
            db.BusinessVerticals.Add(businessVertical);
            db.Users.Add(user);

            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                BusinessVerticalId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            db.WorkRequests.Add(workRequest);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetDashboardAnalyticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalActiveRequests);
            Assert.Equal(0, result.TotalCompletedRequests);
            Assert.True(result.AverageCompletionTime >= 0);
            Assert.True(result.SLAComplianceRate >= 0);
            Assert.True(result.ResourceUtilization >= 0);
        }

        [Fact]
        public async Task GetDepartmentAnalyticsAsync_ReturnsDepartmentSpecificData()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            var department = new Department { Id = 1, Name = "IT", ResourceCapacity = 10, CurrentUtilization = 75 };
            var user = new User { Id = 1, Name = "Test User", DepartmentId = 1 };

            db.Departments.Add(department);
            db.Users.Add(user);

            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            db.WorkRequests.Add(workRequest);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetDepartmentAnalyticsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.DepartmentId);
            Assert.Equal("IT", result.DepartmentName);
            Assert.Equal(1, result.ActiveRequests);
            Assert.Equal(0, result.CompletedRequests);
        }

        [Fact]
        public async Task GetWorkflowAnalyticsAsync_ReturnsWorkflowMetrics()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            db.WorkRequests.Add(workRequest);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetWorkflowAnalyticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.StageMetrics.Count > 0);
            Assert.True(result.AverageTimeInStage >= 0);
            Assert.True(result.TotalTransitions >= 0);
        }

        [Fact]
        public async Task GetPriorityAnalyticsAsync_ReturnsPriorityDistribution()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            var workRequest1 = new WorkRequest
            {
                Id = 1,
                Title = "High Priority Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            var workRequest2 = new WorkRequest
            {
                Id = 2,
                Title = "Low Priority Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.Low,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                CurrentStage = WorkflowStage.Intake,
                Priority = 0.3m,
                EstimatedEffort = 20
            };

            db.WorkRequests.AddRange(workRequest1, workRequest2);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetPriorityAnalyticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Distribution.Count > 0);
            Assert.True(result.AveragePriorityScore > 0);
        }

        [Fact]
        public async Task GetResourceUtilizationAsync_ReturnsUtilizationData()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            var department = new Department { Id = 1, Name = "IT", ResourceCapacity = 10, CurrentUtilization = 75 };
            db.Departments.Add(department);

            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            db.WorkRequests.Add(workRequest);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetResourceUtilizationAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OverallUtilization >= 0);
            Assert.True(result.DepartmentUtilization.Count > 0);
            Assert.True(result.Allocations.Count > 0);
        }

        [Fact]
        public async Task GetSLAComplianceAsync_ReturnsComplianceData()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.Closed,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                ActualDate = DateTime.UtcNow.AddDays(-1),
                CurrentStage = WorkflowStage.Closure,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            db.WorkRequests.Add(workRequest);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetSLAComplianceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OverallComplianceRate >= 0);
            Assert.True(result.ComplianceByCategory.Count >= 0);
            Assert.True(result.ComplianceByDepartment.Count >= 0);
        }

        [Fact]
        public async Task GetTrendDataAsync_ReturnsTrendData()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            db.WorkRequests.Add(workRequest);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetTrendDataAsync("requests", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 0);
        }

        [Fact]
        public async Task GetDashboardAnalyticsAsync_WithDateRange_FiltersCorrectly()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            var workRequest1 = new WorkRequest
            {
                Id = 1,
                Title = "Old Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-60),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            var workRequest2 = new WorkRequest
            {
                Id = 2,
                Title = "Recent Request",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.High,
                Status = WorkStatus.InProgress,
                DepartmentId = 1,
                SubmitterId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CurrentStage = WorkflowStage.Development,
                Priority = 0.8m,
                EstimatedEffort = 40
            };

            db.WorkRequests.AddRange(workRequest1, workRequest2);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetDashboardAnalyticsAsync(
                fromDate: DateTime.UtcNow.AddDays(-30),
                toDate: DateTime.UtcNow
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalActiveRequests); // Only the recent request should be included
        }

        [Fact]
        public async Task GetDepartmentAnalyticsAsync_WithInvalidDepartment_ReturnsEmptyData()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AnalyticsService(db);

            // Act
            var result = await service.GetDepartmentAnalyticsAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999, result.DepartmentId);
            Assert.Equal("Unknown", result.DepartmentName);
            Assert.Equal(0, result.ActiveRequests);
            Assert.Equal(0, result.CompletedRequests);
        }
    }
} 