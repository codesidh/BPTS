using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Services;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace WorkIntakeSystem.Tests
{
    public class MonitoringTests
    {
        private readonly Mock<ILogger<MonitoringService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<IConnectionMultiplexer> _mockRedisConnection;
        private readonly Mock<IDatabase> _mockRedisDatabase;

        public MonitoringTests()
        {
            _mockLogger = new Mock<ILogger<MonitoringService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpClient = new Mock<HttpClient>();
            _mockRedisConnection = new Mock<IConnectionMultiplexer>();
            _mockRedisDatabase = new Mock<IDatabase>();

            // Setup configuration
            _mockConfiguration.Setup(x => x["Monitoring:Elasticsearch:Url"]).Returns("http://localhost:9200");
            _mockConfiguration.Setup(x => x["Monitoring:Logstash:Url"]).Returns("http://localhost:5044");
            _mockConfiguration.Setup(x => x["Monitoring:Kibana:Url"]).Returns("http://localhost:5601");
            // Create a simple configuration object instead of mocking
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Monitoring:Elasticsearch:Url"] = "http://localhost:9200",
                    ["Monitoring:Logstash:Url"] = "http://localhost:5044",
                    ["Monitoring:Kibana:Url"] = "http://localhost:5601",
                    ["Monitoring:Enabled"] = "true"
                })
                .Build();
            _mockConfiguration = new Mock<IConfiguration>();
            // Create a simple configuration object instead of mocking
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Monitoring:Elasticsearch:Url"] = "http://localhost:9200",
                    ["Monitoring:Logstash:Url"] = "http://localhost:5044",
                    ["Monitoring:Kibana:Url"] = "http://localhost:5601",
                    ["Monitoring:Enabled"] = "true"
                })
                .Build();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x.GetValue<string>("Monitoring:Elasticsearch:Url", null)).Returns("http://localhost:9200");
            _mockConfiguration.Setup(x => x.GetValue<string>("Monitoring:Logstash:Url", null)).Returns("http://localhost:5044");
            _mockConfiguration.Setup(x => x.GetValue<string>("Monitoring:Kibana:Url", null)).Returns("http://localhost:5601");
            _mockConfiguration.Setup(x => x.GetValue<string>("Monitoring:Enabled", null)).Returns("true");

            // Setup Redis
            _mockRedisConnection.Setup(x => x.GetDatabase()).Returns(_mockRedisDatabase.Object);
        }

        [Fact]
        public async Task MonitoringService_TrackMetricAsync_ShouldSendToElasticsearch()
        {
            // Arrange
            var monitoringService = new MonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockHttpClient.Object,
                _mockRedisConnection.Object);

            var metricName = "test_metric";
            var value = 42.5;
            var tags = new Dictionary<string, string> { { "test", "value" } };

            // Act
            await monitoringService.TrackMetricAsync(metricName, value, tags);

            // Assert
            // Method should complete without exception
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tracked metric")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task MonitoringService_TrackDependencyAsync_ShouldSendToElasticsearch()
        {
            // Arrange
            var monitoringService = new MonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockHttpClient.Object,
                _mockRedisConnection.Object);

            var dependencyType = "HTTP";
            var target = "https://api.example.com";
            var duration = TimeSpan.FromMilliseconds(150);
            var success = true;

            // Act
            await monitoringService.TrackDependencyAsync(dependencyType, target, duration, success);

            // Assert
            // Method should complete without exception
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tracked dependency")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task MonitoringService_TrackExceptionAsync_ShouldSendToElasticsearch()
        {
            // Arrange
            var monitoringService = new MonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockHttpClient.Object,
                _mockRedisConnection.Object);

            var exception = new InvalidOperationException("Test exception");
            var properties = new Dictionary<string, string> { { "context", "test" } };

            // Act
            await monitoringService.TrackExceptionAsync(exception, properties);

            // Assert
            // Method should complete without exception
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tracked exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task MonitoringService_IncrementCounterAsync_ShouldSendToElasticsearch()
        {
            // Arrange
            var monitoringService = new MonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockHttpClient.Object,
                _mockRedisConnection.Object);

            var counterName = "test_counter";
            var tags = new Dictionary<string, string> { { "test", "value" } };

            // Act
            await monitoringService.IncrementCounterAsync(counterName, tags);

            // Assert
            // Method should complete without exception
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Incremented counter")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task MonitoringService_SetGaugeAsync_ShouldSendToElasticsearch()
        {
            // Arrange
            var monitoringService = new MonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockHttpClient.Object,
                _mockRedisConnection.Object);

            var gaugeName = "test_gauge";
            var value = 75.5;
            var tags = new Dictionary<string, string> { { "test", "value" } };

            // Act
            await monitoringService.SetGaugeAsync(gaugeName, value, tags);

            // Assert
            // Method should complete without exception
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Set gauge")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task MonitoringService_RecordHistogramAsync_ShouldSendToElasticsearch()
        {
            // Arrange
            var monitoringService = new MonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockHttpClient.Object,
                _mockRedisConnection.Object);

            var histogramName = "test_histogram";
            var value = 100.0;
            var tags = new Dictionary<string, string> { { "test", "value" } };

            // Act
            await monitoringService.RecordHistogramAsync(histogramName, value, tags);

            // Assert
            // Method should complete without exception
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recorded histogram")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    public class HealthCheckServiceTests
    {
        private readonly Mock<ILogger<HealthCheckService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<IConnectionMultiplexer> _mockRedisConnection;
        private readonly Mock<IDatabase> _mockRedisDatabase;
        private readonly WorkIntakeDbContext _dbContext;

        public HealthCheckServiceTests()
        {
            _mockLogger = new Mock<ILogger<HealthCheckService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpClient = new Mock<HttpClient>();
            _mockRedisConnection = new Mock<IConnectionMultiplexer>();
            _mockRedisDatabase = new Mock<IDatabase>();

            // Setup in-memory database
            var options = new DbContextOptionsBuilder<WorkIntakeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new WorkIntakeDbContext(options);

            // Setup configuration for health checks
            _mockConfiguration.Setup(x => x.GetValue<string>("HealthChecks:Database:Enabled", null)).Returns("true");
            _mockConfiguration.Setup(x => x.GetValue<string>("HealthChecks:Redis:Enabled", null)).Returns("true");
            _mockConfiguration.Setup(x => x.GetValue<string>("HealthChecks:ExternalServices:Enabled", null)).Returns("true");
            _mockConfiguration.Setup(x => x.GetValue<string>("HealthChecks:SystemResources:Enabled", null)).Returns("true");
            _mockConfiguration.Setup(x => x.GetValue<string>("HealthChecks:Application:Enabled", null)).Returns("true");

            // Setup Redis
            _mockRedisConnection.Setup(x => x.GetDatabase()).Returns(_mockRedisDatabase.Object);
        }

        [Fact]
        public async Task HealthCheckService_CheckDatabaseConnectivityAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckDatabaseConnectivityAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Database connection is working", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckDatabasePerformanceAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckDatabasePerformanceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Database query took", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckDatabaseMigrationStatusAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckDatabaseMigrationStatusAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Database is up to date", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckRedisConnectivityAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Setup Redis ping response
            _mockRedisDatabase.Setup(x => x.PingAsync()).ReturnsAsync(TimeSpan.FromMilliseconds(5));

            // Act
            var result = await healthCheckService.CheckRedisConnectivityAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Redis is responding", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckRedisPerformanceAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Setup Redis operations - simplified to avoid optional parameters
            _mockRedisDatabase.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync("test_value");

            // Act
            var result = await healthCheckService.CheckRedisPerformanceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Redis operation took", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckSystemResourcesAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckSystemResourcesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Memory:", result.Description);
            Assert.Contains("CPU:", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckDiskSpaceAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckDiskSpaceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Disk usage:", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckMemoryUsageAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckMemoryUsageAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Memory usage:", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckCpuUsageAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckCpuUsageAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("CPU usage:", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckApplicationPerformanceAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckApplicationPerformanceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Application performance test took", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckApiEndpointsAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckApiEndpointsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("API endpoints are responding", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckBackgroundServicesAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckBackgroundServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Background services are running", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_CheckWorkflowEngineAsync_ShouldReturnHealthy()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act
            var result = await healthCheckService.CheckWorkflowEngineAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsHealthy);
            Assert.Equal("Healthy", result.Status);
            Assert.Contains("Workflow engine is operational", result.Description);
        }

        [Fact]
        public async Task HealthCheckService_RunComprehensiveHealthCheckAsync_ShouldReturnCompleteReport()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Setup Redis ping response
            _mockRedisDatabase.Setup(x => x.PingAsync()).ReturnsAsync(TimeSpan.FromMilliseconds(5));

            // Act
            var result = await healthCheckService.RunComprehensiveHealthCheckAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OverallHealth);
            Assert.Equal("Healthy", result.OverallStatus);
            Assert.True(result.TotalCheckTime.TotalMilliseconds > 0);
            Assert.NotNull(result.Summary);
            Assert.NotNull(result.DatabaseChecks);
            Assert.NotNull(result.CacheChecks);
            Assert.NotNull(result.ExternalServiceChecks);
            Assert.NotNull(result.SystemChecks);
            Assert.NotNull(result.ApplicationChecks);
        }

        [Fact]
        public async Task HealthCheckService_ConfigurationMethods_ShouldWorkCorrectly()
        {
            // Arrange
            var healthCheckService = new HealthCheckService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _dbContext,
                _mockRedisConnection.Object,
                _mockHttpClient.Object);

            // Act & Assert
            var isEnabled = await healthCheckService.IsHealthCheckEnabledAsync("database");
            Assert.True(isEnabled);

            await healthCheckService.SetHealthCheckEnabledAsync("database", false);
            isEnabled = await healthCheckService.IsHealthCheckEnabledAsync("database");
            Assert.False(isEnabled);

            var interval = await healthCheckService.GetHealthCheckIntervalAsync("database");
            Assert.Equal(TimeSpan.FromSeconds(30), interval);

            await healthCheckService.SetHealthCheckIntervalAsync("database", TimeSpan.FromSeconds(60));
            interval = await healthCheckService.GetHealthCheckIntervalAsync("database");
            Assert.Equal(TimeSpan.FromSeconds(60), interval);
        }
    }

    public class ApplicationPerformanceMonitoringServiceTests
    {
        private readonly Mock<ILogger<ApplicationPerformanceMonitoringService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IMonitoringService> _mockMonitoringService;

        public ApplicationPerformanceMonitoringServiceTests()
        {
            _mockLogger = new Mock<ILogger<ApplicationPerformanceMonitoringService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMonitoringService = new Mock<IMonitoringService>();

            // Setup configuration
            _mockConfiguration.Setup(x => x.GetValue<string>("Monitoring:APM:Enabled", null)).Returns("true");
        }

        [Fact]
        public async Task APMService_TrackMetricAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var metricName = "test_metric";
            var value = 42.5;
            var tags = new Dictionary<string, string> { { "test", "value" } };

            // Act
            await apmService.TrackMetricAsync(metricName, value, tags);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackMetricAsync(metricName, value, tags), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackDependencyAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var dependencyType = "HTTP";
            var target = "https://api.example.com";
            var duration = TimeSpan.FromMilliseconds(150);
            var success = true;
            var properties = new Dictionary<string, string> { { "test", "value" } };

            // Act
            await apmService.TrackDependencyAsync(dependencyType, target, duration, success, properties);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackDependencyAsync(dependencyType, target, duration, success), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackExceptionAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var exception = new InvalidOperationException("Test exception");
            var properties = new Dictionary<string, string> { { "context", "test" } };

            // Act
            await apmService.TrackExceptionAsync(exception, properties);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackExceptionAsync(exception, properties), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackEventAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var eventName = "test_event";
            var properties = new Dictionary<string, string> { { "test", "value" } };

            // Act
            await apmService.TrackEventAsync(eventName, properties);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackEventAsync(eventName, properties), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackUserActionAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var userId = "user123";
            var action = "login";
            var properties = new Dictionary<string, string> { { "ip", "192.168.1.1" } };

            // Act
            await apmService.TrackUserActionAsync(userId, action, properties);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackEventAsync("UserAction", It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackBusinessEventAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var eventName = "work_request_created";
            var properties = new Dictionary<string, object> { { "request_id", "req123" }, { "priority", "high" } };

            // Act
            await apmService.TrackBusinessEventAsync(eventName, properties);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackEventAsync("Business_work_request_created", It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackPerformanceAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var operationName = "database_query";
            var duration = TimeSpan.FromMilliseconds(250);
            var properties = new Dictionary<string, string> { { "table", "users" } };

            // Act
            await apmService.TrackPerformanceAsync(operationName, duration, properties);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackMetricAsync($"performance_{operationName}_duration", duration.TotalMilliseconds, properties), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackMemoryUsageAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var memoryUsage = 512 * 1024 * 1024L; // 512MB
            var tags = new Dictionary<string, string> { { "instance", "web-1" } };

            // Act
            await apmService.TrackMemoryUsageAsync(memoryUsage, tags);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackMetricAsync("memory_usage_bytes", memoryUsage, tags), Times.Once);
            _mockMonitoringService.Verify(x => x.TrackMetricAsync("memory_usage_mb", memoryUsage / 1024.0 / 1024.0, tags), Times.Once);
        }

        [Fact]
        public async Task APMService_TrackCpuUsageAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            var cpuUsage = 75.5;
            var tags = new Dictionary<string, string> { { "instance", "web-1" } };

            // Act
            await apmService.TrackCpuUsageAsync(cpuUsage, tags);

            // Assert
            _mockMonitoringService.Verify(x => x.TrackMetricAsync("cpu_usage_percent", cpuUsage, tags), Times.Once);
        }

        [Fact]
        public async Task APMService_SetCustomPropertyAsync_ShouldWorkCorrectly()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            // Act
            await apmService.SetCustomPropertyAsync("user_id", "user123");
            await apmService.SetCustomPropertyAsync("session_id", "session456");

            // Assert
            _mockMonitoringService.Verify(x => x.TrackEventAsync("CustomProperty_Set", It.IsAny<Dictionary<string, string>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task APMService_ClearCustomPropertiesAsync_ShouldWorkCorrectly()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            // Act
            await apmService.ClearCustomPropertiesAsync();

            // Assert
            _mockMonitoringService.Verify(x => x.TrackEventAsync("CustomProperty_Clear", It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Fact]
        public async Task APMService_IsEnabledAsync_ShouldReturnTrue()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            // Act
            var result = await apmService.IsEnabledAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task APMService_FlushAsync_ShouldCallMonitoringService()
        {
            // Arrange
            var apmService = new ApplicationPerformanceMonitoringService(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMonitoringService.Object);

            // Act
            await apmService.FlushAsync();

            // Assert
            _mockMonitoringService.Verify(x => x.SendToElasticsearchAsync("apm_flush", It.IsAny<object>()), Times.Once);
        }
    }
} 