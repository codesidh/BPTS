using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.Graph;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Tests;

public static class TestConfiguration
{
    public static void ConfigureTestServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Redis Connection as a mock to avoid external dependency during tests
        var mockRedis = new Mock<IConnectionMultiplexer>();
        // Setup commonly used members to prevent NullReferenceExceptions in services
        var mockDb = new Mock<IDatabase>();
        mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object?>() ))
                 .Returns(mockDb.Object);
        services.AddSingleton<IConnectionMultiplexer>(mockRedis.Object);

        // Override IDistributedCache with in-memory implementation and remove Redis cache registration
        services.RemoveAll<IDistributedCache>();
        services.AddDistributedMemoryCache();

        // Replace SQL Server DbContext with in-memory provider for tests
        services.RemoveAll<WorkIntakeDbContext>();
        services.RemoveAll<DbContextOptions<WorkIntakeDbContext>>();
        services.AddDbContext<WorkIntakeDbContext>(options =>
            options.UseInMemoryDatabase("WorkIntakeTestDb"));

        // Register a no-op rate limiting service to bypass Redis entirely
        services.RemoveAll<IRateLimitingService>();
        services.AddSingleton<IRateLimitingService, NoOpRateLimitingService>();

        // Register Microsoft Graph Client (with mock for testing)
        var mockGraphClient = new Mock<GraphServiceClient>();
        services.AddScoped<GraphServiceClient>(provider => mockGraphClient.Object);
        
        // Register PowerBI Client (with mock for testing)
        var mockPowerBIClient = new Mock<Microsoft.PowerBI.Api.PowerBIClient>();
        services.AddScoped<Microsoft.PowerBI.Api.PowerBIClient>(provider => mockPowerBIClient.Object);
        
        // Register Windows Authentication Service (with mock for testing)
        var mockWindowsAuthService = new Mock<WorkIntakeSystem.Core.Interfaces.IWindowsAuthenticationService>();
        services.AddSingleton<WorkIntakeSystem.Core.Interfaces.IWindowsAuthenticationService>(provider => mockWindowsAuthService.Object);
        
        // Disable Service Broker for testing to avoid SQL errors
        services.AddSingleton<WorkIntakeSystem.Core.Interfaces.IServiceBrokerService>(provider => 
        {
            var mockServiceBroker = new Mock<WorkIntakeSystem.Core.Interfaces.IServiceBrokerService>();
            mockServiceBroker.Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            mockServiceBroker.Setup(x => x.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(Enumerable.Empty<WorkIntakeSystem.Core.Interfaces.ServiceBrokerMessage>());
            return mockServiceBroker.Object;
        });
    }

    // Simple no-op implementation used in tests
    private class NoOpRateLimitingService : IRateLimitingService
    {
        public Task<bool> IsRequestAllowed(string clientId, string endpoint) => Task.FromResult(true);
        public Task<WorkIntakeSystem.Core.Interfaces.RateLimitInfo> GetRateLimitInfo(string clientId, string endpoint)
            => Task.FromResult(new WorkIntakeSystem.Core.Interfaces.RateLimitInfo { IsAllowed = true, Limit = int.MaxValue, Remaining = int.MaxValue, ResetTime = DateTime.UtcNow.AddMinutes(1) });
        public Task IncrementRequestCount(string clientId, string endpoint) => Task.CompletedTask;
        public Task ResetRateLimit(string clientId, string endpoint) => Task.CompletedTask;
    }
} 