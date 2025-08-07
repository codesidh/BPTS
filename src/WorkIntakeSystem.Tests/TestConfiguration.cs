using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.Graph;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using WorkIntakeSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;

namespace WorkIntakeSystem.Tests;

public static class TestConfiguration
{
    public static void ConfigureTestHostBuilder(IWebHostBuilder builder)
    {
        // Set environment variables BEFORE any configuration is loaded
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "test-super-secret-jwt-key-with-at-least-32-characters-for-testing");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "WorkIntakeSystem-Test");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "WorkIntakeSystem-Test");
        Environment.SetEnvironmentVariable("JwtSettings__ExpirationHours", "24");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test JWT settings
            var testJwtSettings = new Dictionary<string, string>
            {
                ["JwtSettings:Secret"] = "test-super-secret-jwt-key-with-at-least-32-characters-for-testing",
                ["JwtSettings:Issuer"] = "WorkIntakeSystem-Test",
                ["JwtSettings:Audience"] = "WorkIntakeSystem-Test",
                ["JwtSettings:ExpirationHours"] = "24"
            };
            
            config.AddInMemoryCollection(testJwtSettings);
        });
        
        builder.ConfigureServices((context, services) =>
        {
            // Remove SQL Server DbContext registration by replacing it
            var dbContextDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(WorkIntakeDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);
                
            var dbContextOptionsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<WorkIntakeDbContext>));
            if (dbContextOptionsDescriptor != null)
                services.Remove(dbContextOptionsDescriptor);
            
            // Remove Redis cache registration
            var distributedCacheDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDistributedCache));
            if (distributedCacheDescriptor != null)
                services.Remove(distributedCacheDescriptor);
                
            var redisDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);
            
            // Register in-memory database
            services.AddDbContext<WorkIntakeDbContext>(options =>
                options.UseInMemoryDatabase("WorkIntakeTestDb"));
            
            // Register in-memory distributed cache
            services.AddDistributedMemoryCache();
            
            // Register mock Redis connection
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
                     .Returns(mockDb.Object);
            services.AddSingleton<IConnectionMultiplexer>(mockRedis.Object);
            
            // Register no-op rate limiting service
            services.AddSingleton<IRateLimitingService, NoOpRateLimitingService>();
            
            // Register mock services for external dependencies
            ConfigureMockServices(services);
            
            // Configure JWT settings for tests - use the existing JWT configuration but with test settings
            var testJwtSecret = "test-super-secret-jwt-key-with-at-least-32-characters-for-testing";
            var testJwtIssuer = "WorkIntakeSystem-Test";
            var testJwtAudience = "WorkIntakeSystem-Test";
            
            // Configure the existing JWT bearer options with test settings
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(testJwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = testJwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = testJwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            
            // Override JwtAuthenticationService with test configuration
            var jwtAuthDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IJwtAuthenticationService));
            if (jwtAuthDescriptor != null)
                services.Remove(jwtAuthDescriptor);
                
            services.AddScoped<IJwtAuthenticationService>(provider =>
            {
                var userRepository = provider.GetRequiredService<IUserRepository>();
                var emailService = provider.GetRequiredService<IEmailService>();
                
                // Create a new configuration with test JWT settings
                var testConfig = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["JwtSettings:Secret"] = testJwtSecret,
                        ["JwtSettings:Issuer"] = testJwtIssuer,
                        ["JwtSettings:Audience"] = testJwtAudience,
                        ["JwtSettings:ExpirationHours"] = "24"
                    })
                    .Build();
                
                return new WorkIntakeSystem.Infrastructure.Services.JwtAuthenticationService(
                    userRepository, testConfig, emailService);
            });
            
            // Also override WindowsAuthenticationService to use the same JWT settings
            var windowsAuthDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWindowsAuthenticationService));
            if (windowsAuthDescriptor != null)
                services.Remove(windowsAuthDescriptor);
                
            services.AddScoped<IWindowsAuthenticationService>(provider =>
            {
                var userRepository = provider.GetRequiredService<IUserRepository>();
                var emailService = provider.GetRequiredService<IEmailService>();
                
                // Create a new configuration with test JWT settings
                var testConfig = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["JwtSettings:Secret"] = testJwtSecret,
                        ["JwtSettings:Issuer"] = testJwtIssuer,
                        ["JwtSettings:Audience"] = testJwtAudience,
                        ["JwtSettings:ExpirationHours"] = "24"
                    })
                    .Build();
                
                var mockAdService = new Mock<IActiveDirectoryService>();
                return new WorkIntakeSystem.Infrastructure.Services.WindowsAuthenticationService(
                    userRepository, mockAdService.Object, testConfig);
            });
        });
    }

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
        var distributedCacheDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDistributedCache));
        if (distributedCacheDescriptor != null)
            services.Remove(distributedCacheDescriptor);
        services.AddDistributedMemoryCache();

        // Replace SQL Server DbContext with in-memory provider for tests
        var dbContextDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(WorkIntakeDbContext));
        if (dbContextDescriptor != null)
            services.Remove(dbContextDescriptor);
            
        var dbContextOptionsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<WorkIntakeDbContext>));
        if (dbContextOptionsDescriptor != null)
            services.Remove(dbContextOptionsDescriptor);
        services.AddDbContext<WorkIntakeDbContext>(options =>
            options.UseInMemoryDatabase("WorkIntakeTestDb"));

        // Register a no-op rate limiting service to bypass Redis entirely
        var rateLimitingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRateLimitingService));
        if (rateLimitingDescriptor != null)
            services.Remove(rateLimitingDescriptor);
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

    private static void ConfigureMockServices(IServiceCollection services)
    {
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