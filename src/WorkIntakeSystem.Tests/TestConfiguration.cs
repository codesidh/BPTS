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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace WorkIntakeSystem.Tests;

public static class TestConfiguration
{
    public static void ConfigureTestHostBuilder(IWebHostBuilder builder)
    {
        // Define test JWT settings constants
        const string testJwtSecret = "test-super-secret-jwt-key-with-at-least-32-characters-for-testing";
        const string testJwtIssuer = "WorkIntakeSystem-Test";
        const string testJwtAudience = "WorkIntakeSystem-Test";
        const string testJwtExpirationHours = "24";
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test JWT settings with highest priority
            var testJwtSettings = new Dictionary<string, string>
            {
                ["JwtSettings:Secret"] = testJwtSecret,
                ["JwtSettings:Issuer"] = testJwtIssuer,
                ["JwtSettings:Audience"] = testJwtAudience,
                ["JwtSettings:ExpirationHours"] = testJwtExpirationHours,
                // Add other required configuration settings for tests
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=WorkIntakeTestDb;Trusted_Connection=true;MultipleActiveResultSets=true;",
                ["ConnectionStrings:Redis"] = "localhost:6379"
            };
            
            // Add test configuration first (highest priority)
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
            
            // Configure JWT Bearer authentication to use test settings
            // This ensures both token generation and validation use the same configuration
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var testJwtSecret = "test-super-secret-jwt-key-with-at-least-32-characters-for-testing";
                var testJwtIssuer = "WorkIntakeSystem-Test";
                var testJwtAudience = "WorkIntakeSystem-Test";
                
                // Create OpenIdConnectConfiguration to bypass metadata endpoint calls
                var config = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration()
                {
                    Issuer = testJwtIssuer
                };
                
                // Add the signing key
                var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(testJwtSecret));
                config.SigningKeys.Add(securityKey);
                
                // Set the configuration to bypass metadata download
                options.Configuration = config;
                
                // Override token validation parameters to ensure consistency
                options.TokenValidationParameters.ValidIssuer = testJwtIssuer;
                options.TokenValidationParameters.ValidAudience = testJwtAudience;
                options.TokenValidationParameters.IssuerSigningKey = securityKey;
                options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = true;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
            });
            
            // Override WindowsAuthenticationService to use mock Active Directory service
            var windowsAuthDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWindowsAuthenticationService));
            if (windowsAuthDescriptor != null)
                services.Remove(windowsAuthDescriptor);
                
            services.AddScoped<IWindowsAuthenticationService>(provider =>
            {
                var userRepository = provider.GetRequiredService<IUserRepository>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                var mockAdService = new Mock<IActiveDirectoryService>();
                return new WorkIntakeSystem.Infrastructure.Services.WindowsAuthenticationService(
                    userRepository, mockAdService.Object, configuration);
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