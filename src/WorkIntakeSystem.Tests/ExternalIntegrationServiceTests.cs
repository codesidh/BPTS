using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;
using WorkIntakeSystem.Infrastructure.Services;
using Xunit;

namespace WorkIntakeSystem.Tests
{
    public class ExternalIntegrationServiceTests
    {
        private readonly Mock<ILogger<ExternalIntegrationService>> _mockLogger;
        private readonly Mock<IConfigurationService> _mockConfigurationService;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly WorkIntakeDbContext _context;

        public ExternalIntegrationServiceTests()
        {
            _mockLogger = new Mock<ILogger<ExternalIntegrationService>>();
            _mockConfigurationService = new Mock<IConfigurationService>();
            _mockHttpClient = new Mock<HttpClient>();

            // Setup in-memory database
            var options = new DbContextOptionsBuilder<WorkIntakeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new WorkIntakeDbContext(options);

            // Setup configuration service mocks
            _mockConfigurationService.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync((string key, int? businessVerticalId, int? version) => 
                {
                    switch (key)
                    {
                        case "ExternalSystems:TestSystem:IsEnabled": return "true";
                        case "ExternalSystems:TestSystem:BaseUrl": return "https://api.test.com";
                        case "ExternalSystems:TestSystem:ApiKey": return "test-api-key";
                        case "ExternalSystems:TestSystem:Secret": return "test-secret";
                        default: return null;
                    }
                });

            _mockConfigurationService.Setup(x => x.GetValueAsync<bool>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync((string key, int? businessVerticalId, int? version) => 
                {
                    if (key == "ExternalSystems:TestSystem:IsEnabled") return true;
                    return false;
                });
        }

        [Fact]
        public async Task SyncWithExternalSystemAsync_WhenSystemEnabled_ShouldSendData()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _mockHttpClient.Object, _mockConfigurationService.Object);
            
            _mockConfigurationService.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync((string key, int? businessVerticalId, int? version) => 
                {
                    if (key == "ExternalSystems:TestSystem:IsEnabled") return "true";
                    return null;
                });

            var data = new { test = "value" };

            // Act
            var result = await service.SyncWithExternalSystemAsync("TestSystem", "/api/test", data);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SyncWithExternalSystemAsync_WhenSystemDisabled_ShouldNotSendData()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _mockHttpClient.Object, _mockConfigurationService.Object);
            
            _mockConfigurationService.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync((string key, int? businessVerticalId, int? version) => 
                {
                    if (key == "ExternalSystems:TestSystem:IsEnabled") return "false";
                    return null;
                });

            var data = new { test = "value" };

            // Act
            var result = await service.SyncWithExternalSystemAsync("TestSystem", "/api/test", data);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetExternalSystemStatusAsync_ShouldReturnSystemStatuses()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _mockHttpClient.Object, _mockConfigurationService.Object);

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
            var service = new ExternalIntegrationService(_context, _mockHttpClient.Object, _mockConfigurationService.Object);

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
            var service = new ExternalIntegrationService(_context, _mockHttpClient.Object, _mockConfigurationService.Object);

            // Act
            var result = await service.GetIntegrationLogsAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetIntegrationLogsAsync_WithFilters_ReturnsFilteredLogs()
        {
            // Arrange
            var service = new ExternalIntegrationService(_context, _mockHttpClient.Object, _mockConfigurationService.Object);

            // Act
            var result = await service.GetIntegrationLogsAsync("TestSystem", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
        }
    }
} 