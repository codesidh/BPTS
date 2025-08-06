using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.Graph;
using Moq;

namespace WorkIntakeSystem.Tests;

public static class TestConfiguration
{
    public static void ConfigureTestServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Redis Connection
        var redisConnection = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379");
        services.AddSingleton<IConnectionMultiplexer>(redisConnection);

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
} 