using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Services;
using WorkIntakeSystem.Infrastructure.Repositories;
using Xunit;

namespace WorkIntakeSystem.Tests
{
    public class ConfigurationServiceTests
    {
        [Fact]
        public async Task ReturnsValueFromDatabaseIfPresent()
        {
            var repo = new Mock<ISystemConfigurationRepository>();
            repo.Setup(r => r.GetLatestActiveAsync("TestKey", null)).ReturnsAsync(new WorkIntakeSystem.Core.Entities.SystemConfiguration { ConfigurationValue = "db-value", IsActive = true });
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var mockLogger = new Mock<ILogger<ConfigurationService>>();
            var service = new ConfigurationService(repo.Object, config, mockLogger.Object);
            var value = await service.GetValueAsync("TestKey");
            Assert.Equal("db-value", value);
        }

        [Fact]
        public async Task FallsBackToAppSettingsIfNotInDatabase()
        {
            var repo = new Mock<ISystemConfigurationRepository>();
            repo.Setup(r => r.GetLatestActiveAsync("TestKey", null)).ReturnsAsync((WorkIntakeSystem.Core.Entities.SystemConfiguration?)null);
            var config = new ConfigurationBuilder().AddInMemoryCollection(new[] { new System.Collections.Generic.KeyValuePair<string, string>("TestKey", "appsettings-value") }).Build();
            var mockLogger = new Mock<ILogger<ConfigurationService>>();
            var service = new ConfigurationService(repo.Object, config, mockLogger.Object);
            var value = await service.GetValueAsync("TestKey");
            Assert.Equal("appsettings-value", value);
        }
    }
} 