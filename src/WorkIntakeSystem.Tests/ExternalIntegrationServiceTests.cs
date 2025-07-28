using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Services;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Collections.Generic;

namespace WorkIntakeSystem.Tests
{
    public class ExternalIntegrationServiceTests
    {
        private readonly WorkIntakeDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfigurationService> _mockConfigurationService;

        public ExternalIntegrationServiceTests()
        {
            var options = new DbContextOptionsBuilder<WorkIntakeDbContext>()
                .UseInMemoryDatabase(databaseName: $"ExternalIntegrationServiceTests_{Guid.NewGuid()}")
                .Options;
            _context = new WorkIntakeDbContext(options);
            
            // Create a mock HTTP client that returns success
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });
            
            _httpClient = new HttpClient(mockHttpHandler.Object);
            _mockConfigurationService = new Mock<IConfigurationService>();
        }

        [Fact]
        public async Task SyncWithExternalSystemAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _httpClient, _mockConfigurationService.Object);
            
            _mockConfigurationService.Setup(x => x.GetValueAsync("ExternalSystems:TestSystem:IsEnabled", null, null))
                .ReturnsAsync("true");
            _mockConfigurationService.Setup(x => x.GetValueAsync("ExternalSystems:TestSystem:BaseUrl", null, null))
                .ReturnsAsync("https://api.example.com");
            _mockConfigurationService.Setup(x => x.GetValueAsync("ExternalSystems:TestSystem:ApiKey", null, null))
                .ReturnsAsync("test-api-key");
            _mockConfigurationService.Setup(x => x.GetValueAsync("ExternalSystems:TestSystem:Secret", null, null))
                .ReturnsAsync("test-secret");

            // Act
            var result = await service.SyncWithExternalSystemAsync("TestSystem", "/api/test", new { test = "data" });

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SyncWithExternalSystemAsync_WithDisabledSystem_ReturnsFalse()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _httpClient, _mockConfigurationService.Object);
            
            _mockConfigurationService.Setup(x => x.GetValueAsync<bool>("ExternalSystems:TestSystem:IsEnabled", null, null))
                .ReturnsAsync(false);

            // Act
            var result = await service.SyncWithExternalSystemAsync("TestSystem", "/api/test", new { test = "data" });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetExternalSystemStatusAsync_ReturnsSystemStatuses()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _httpClient, _mockConfigurationService.Object);

            // Act
            var result = await service.GetExternalSystemStatusAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public async Task LogIntegrationEventAsync_ReturnsLogEntry()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _httpClient, _mockConfigurationService.Object);

            // Act
            var result = await service.LogIntegrationEventAsync("TestSystem", "test_event", new { test = "data" }, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestSystem", result.SystemName);
            Assert.Equal("test_event", result.EventType);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetIntegrationLogsAsync_ReturnsLogs()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _httpClient, _mockConfigurationService.Object);

            // Act
            var result = await service.GetIntegrationLogsAsync();

            // Assert
            Assert.NotNull(result);
        }
    }

    public class ProjectManagementIntegrationTests
    {
        private readonly Mock<IExternalIntegrationService> _mockIntegrationService;
        private readonly WorkIntakeDbContext _context;

        public ProjectManagementIntegrationTests()
        {
            _mockIntegrationService = new Mock<IExternalIntegrationService>();
            var options = new DbContextOptionsBuilder<WorkIntakeDbContext>()
                .UseInMemoryDatabase(databaseName: $"ProjectManagementIntegrationTests_{Guid.NewGuid()}")
                .Options;
            _context = new WorkIntakeDbContext(options);
        }

        [Fact]
        public async Task CreateProjectAsync_WithValidWorkRequest_ReturnsSuccess()
        {
            // Arrange
            var service = new ProjectManagementIntegration(_mockIntegrationService.Object, _context);
            
            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Project",
                Description = "Test Description",
                Category = WorkCategory.Project,
                PriorityLevel = PriorityLevel.High,
                EstimatedEffort = 40,
                TargetDate = DateTime.UtcNow.AddDays(30)
            };

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.CreateProjectAsync(workRequest);

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("AzureDevOps", "projects", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var service = new ProjectManagementIntegration(_mockIntegrationService.Object, _context);
            
            var projectData = new { name = "Updated Project", description = "Updated Description" };

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.UpdateProjectAsync(1, projectData);

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("AzureDevOps", "projects/1", projectData), Times.Once);
        }

        [Fact]
        public async Task SyncProjectStatusAsync_WithValidWorkRequest_ReturnsSuccess()
        {
            // Arrange
            var service = new ProjectManagementIntegration(_mockIntegrationService.Object, _context);
            
            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Project",
                Status = WorkStatus.InProgress,
                CurrentStage = WorkflowStage.Development,
                ActualDate = DateTime.UtcNow,
                ActualEffort = 20
            };

            _context.WorkRequests.Add(workRequest);
            await _context.SaveChangesAsync();
            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.SyncProjectStatusAsync(1);

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("AzureDevOps", "projects/1/status", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetProjectTasksAsync_ReturnsTasks()
        {
            // Arrange
            var service = new ProjectManagementIntegration(_mockIntegrationService.Object, _context);

            // Act
            var result = await service.GetProjectTasksAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }
    }

    public class CalendarIntegrationTests
    {
        private readonly Mock<IExternalIntegrationService> _mockIntegrationService;

        public CalendarIntegrationTests()
        {
            _mockIntegrationService = new Mock<IExternalIntegrationService>();
        }

        [Fact]
        public async Task CreateCalendarEventAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var service = new CalendarIntegration(_mockIntegrationService.Object);
            
            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Event",
                Description = "Test Description",
                Category = WorkCategory.WorkRequest,
                PriorityLevel = PriorityLevel.Medium
            };

            var startDate = DateTime.UtcNow.AddDays(1);
            var endDate = DateTime.UtcNow.AddDays(1).AddHours(2);

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.CreateCalendarEventAsync(workRequest, startDate, endDate);

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("Teams", "calendar/events", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCalendarEventAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var service = new CalendarIntegration(_mockIntegrationService.Object);
            
            var startDate = DateTime.UtcNow.AddDays(1);
            var endDate = DateTime.UtcNow.AddDays(1).AddHours(2);

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.UpdateCalendarEventAsync(1, startDate, endDate);

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("Teams", "calendar/events/1", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCalendarEventAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var service = new CalendarIntegration(_mockIntegrationService.Object);

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.DeleteCalendarEventAsync(1);

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("Teams", "calendar/events/1/delete", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_ReturnsEvents()
        {
            // Arrange
            var service = new CalendarIntegration(_mockIntegrationService.Object);
            
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(30);

            // Act
            var result = await service.GetCalendarEventsAsync(fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }
    }

    public class NotificationIntegrationTests
    {
        private readonly Mock<IExternalIntegrationService> _mockIntegrationService;

        public NotificationIntegrationTests()
        {
            _mockIntegrationService = new Mock<IExternalIntegrationService>();
        }

        [Fact]
        public async Task SendNotificationAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var service = new NotificationIntegration(_mockIntegrationService.Object);

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.SendNotificationAsync(
                "test@example.com",
                "Test Subject",
                "Test Message",
                NotificationType.Email
            );

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("EmailService", "notifications/send", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task SendBulkNotificationAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var service = new NotificationIntegration(_mockIntegrationService.Object);
            
            var recipients = new List<string> { "user1@example.com", "user2@example.com" };

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await service.SendBulkNotificationAsync(
                recipients,
                "Bulk Test Subject",
                "Bulk Test Message",
                NotificationType.Email
            );

            // Assert
            Assert.True(result);
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync("EmailService", "notifications/bulk-send", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetNotificationStatusAsync_ReturnsStatus()
        {
            // Arrange
            var service = new NotificationIntegration(_mockIntegrationService.Object);

            // Act
            var result = await service.GetNotificationStatusAsync("notification-123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("notification-123", result.Id);
            Assert.Equal("Delivered", result.Status);
        }

        [Theory]
        [InlineData(NotificationType.Email, "EmailService")]
        [InlineData(NotificationType.Teams, "Teams")]
        [InlineData(NotificationType.Slack, "Slack")]
        [InlineData(NotificationType.SMS, "SMSService")]
        [InlineData(NotificationType.Push, "PushNotificationService")]
        public async Task SendNotificationAsync_WithDifferentTypes_UsesCorrectSystem(NotificationType type, string expectedSystem)
        {
            // Arrange
            var service = new NotificationIntegration(_mockIntegrationService.Object);

            _mockIntegrationService.Setup(x => x.SyncWithExternalSystemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            await service.SendNotificationAsync("test@example.com", "Test", "Test", type);

            // Assert
            _mockIntegrationService.Verify(x => x.SyncWithExternalSystemAsync(expectedSystem, "notifications/send", It.IsAny<object>()), Times.Once);
        }
    }
} 