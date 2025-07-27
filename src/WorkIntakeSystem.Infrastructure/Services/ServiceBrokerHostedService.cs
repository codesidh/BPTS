using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class ServiceBrokerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceBrokerHostedService> _logger;

    public ServiceBrokerHostedService(
        IServiceProvider serviceProvider,
        ILogger<ServiceBrokerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service Broker Hosted Service starting");

        // Initialize Service Broker infrastructure
        await InitializeServiceBrokerAsync();

        // Register default message handlers
        RegisterDefaultMessageHandlers();

        // Start processing messages
        using var scope = _serviceProvider.CreateScope();
        var serviceBroker = scope.ServiceProvider.GetRequiredService<IServiceBrokerService>();
        var messageRegistry = scope.ServiceProvider.GetRequiredService<IMessageHandlerRegistry>();

        await serviceBroker.ProcessMessagesAsync(async message =>
        {
            return await messageRegistry.HandleMessageAsync(message);
        }, stoppingToken);

        _logger.LogInformation("Service Broker Hosted Service stopped");
    }

    private async Task InitializeServiceBrokerAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceBroker = scope.ServiceProvider.GetRequiredService<IServiceBrokerService>();

            // Create queue and service if they don't exist
            await serviceBroker.CreateQueueIfNotExistsAsync();
            await serviceBroker.CreateServiceIfNotExistsAsync();

            _logger.LogInformation("Service Broker infrastructure initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Service Broker infrastructure");
        }
    }

    private void RegisterDefaultMessageHandlers()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var messageRegistry = scope.ServiceProvider.GetRequiredService<IMessageHandlerRegistry>();

            // Register handlers for work request events
            messageRegistry.RegisterHandler("WorkRequestCreated", async message =>
            {
                _logger.LogInformation("Processing WorkRequestCreated message: {MessageBody}", message.MessageBody);
                // TODO: Handle work request created event
                return true;
            });

            messageRegistry.RegisterHandler("WorkRequestUpdated", async message =>
            {
                _logger.LogInformation("Processing WorkRequestUpdated message: {MessageBody}", message.MessageBody);
                // TODO: Handle work request updated event
                return true;
            });

            messageRegistry.RegisterHandler("PriorityVoteSubmitted", async message =>
            {
                _logger.LogInformation("Processing PriorityVoteSubmitted message: {MessageBody}", message.MessageBody);
                // TODO: Handle priority vote submitted event
                return true;
            });

            messageRegistry.RegisterHandler("WorkflowStateChanged", async message =>
            {
                _logger.LogInformation("Processing WorkflowStateChanged message: {MessageBody}", message.MessageBody);
                // TODO: Handle workflow state changed event
                return true;
            });

            messageRegistry.RegisterHandler("ConfigurationChanged", async message =>
            {
                _logger.LogInformation("Processing ConfigurationChanged message: {MessageBody}", message.MessageBody);
                // TODO: Handle configuration changed event - invalidate caches
                return true;
            });

            _logger.LogInformation("Default message handlers registered");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register default message handlers");
        }
    }
} 