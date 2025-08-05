using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class EnterpriseServiceBus : IEnterpriseServiceBus
{
    private readonly ILogger<EnterpriseServiceBus> _logger;
    private readonly IMessageTransformationService _messageTransformationService;
    private readonly ICircuitBreakerService _circuitBreakerService;
    private readonly IDeadLetterQueueService _deadLetterQueueService;
    private readonly Dictionary<string, ServiceRegistry> _serviceRegistry;
    private readonly object _registryLock = new();

    public EnterpriseServiceBus(
        ILogger<EnterpriseServiceBus> logger,
        IMessageTransformationService messageTransformationService,
        ICircuitBreakerService circuitBreakerService,
        IDeadLetterQueueService deadLetterQueueService)
    {
        _logger = logger;
        _messageTransformationService = messageTransformationService;
        _circuitBreakerService = circuitBreakerService;
        _deadLetterQueueService = deadLetterQueueService;
        _serviceRegistry = new Dictionary<string, ServiceRegistry>();
    }

    public async Task<bool> RouteMessageAsync(string messageType, object message, string targetService)
    {
        try
        {
            var serviceBusMessage = new ServiceBusMessage
            {
                MessageType = messageType,
                Payload = message,
                TargetService = targetService,
                Timestamp = DateTime.UtcNow
            };

            return await RouteMessageAsync(serviceBusMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to route message: Type={MessageType}, Target={TargetService}", messageType, targetService);
            return false;
        }
    }

    public async Task<bool> RouteMessageAsync(ServiceBusMessage message)
    {
        try
        {
            // Check if target service is available
            if (!await IsServiceAvailableAsync(message.TargetService))
            {
                _logger.LogWarning("Target service {TargetService} is not available", message.TargetService);
                await _deadLetterQueueService.AddToDeadLetterAsync(message, "Service unavailable");
                return false;
            }

            // Transform message if needed
            var transformedPayload = await _messageTransformationService.TransformMessageAsync(
                message.Payload, 
                message.Headers.GetValueOrDefault("SourceFormat", "JSON").ToString() ?? "JSON",
                message.Headers.GetValueOrDefault("TargetFormat", "JSON").ToString() ?? "JSON");

            message.Payload = transformedPayload;

            // Execute with circuit breaker
            var result = await _circuitBreakerService.ExecuteWithCircuitBreakerAsync(
                message.TargetService,
                async () =>
                {
                    // Simulate actual message delivery
                    await Task.Delay(100); // Simulate network delay
                    _logger.LogInformation("Message routed successfully: {MessageId} -> {TargetService}", 
                        message.Id, message.TargetService);
                    return true;
                });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to route message: {MessageId}", message.Id);
            await _deadLetterQueueService.AddToDeadLetterAsync(message, ex.Message);
            return false;
        }
    }

    public async Task<IEnumerable<ServiceBusMessage>> RouteMessagesAsync(IEnumerable<ServiceBusMessage> messages)
    {
        var results = new List<ServiceBusMessage>();
        var tasks = messages.Select(async message =>
        {
            var success = await RouteMessageAsync(message);
            if (success)
            {
                results.Add(message);
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    public async Task<object> TransformMessageAsync(object message, string sourceFormat, string targetFormat)
    {
        return await _messageTransformationService.TransformMessageAsync(message, sourceFormat, targetFormat);
    }

    public async Task<bool> ValidateMessageFormatAsync(object message, string expectedFormat)
    {
        return await _messageTransformationService.ValidateMessageFormatAsync(message, expectedFormat);
    }

    public async Task<ServiceRegistry> DiscoverServiceAsync(string serviceName)
    {
        lock (_registryLock)
        {
            if (_serviceRegistry.TryGetValue(serviceName, out var service))
            {
                return service;
            }
        }

        // Simulate service discovery from external registry
        await Task.Delay(50);
        return new ServiceRegistry
        {
            ServiceName = serviceName,
            ServiceUrl = $"https://{serviceName.ToLower()}.api.company.com",
            ServiceType = "REST",
            IsActive = true,
            LastHeartbeat = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<ServiceRegistry>> GetAllServicesAsync()
    {
        lock (_registryLock)
        {
            return _serviceRegistry.Values.ToList();
        }
    }

    public async Task<bool> RegisterServiceAsync(ServiceRegistry service)
    {
        try
        {
            lock (_registryLock)
            {
                _serviceRegistry[service.ServiceName] = service;
            }

            _logger.LogInformation("Service registered: {ServiceName} at {ServiceUrl}", 
                service.ServiceName, service.ServiceUrl);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register service: {ServiceName}", service.ServiceName);
            return false;
        }
    }

    public async Task<bool> UnregisterServiceAsync(string serviceName)
    {
        try
        {
            lock (_registryLock)
            {
                var removed = _serviceRegistry.Remove(serviceName);
                if (removed)
                {
                    _logger.LogInformation("Service unregistered: {ServiceName}", serviceName);
                }
                return removed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister service: {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<T> ExecuteWithCircuitBreakerAsync<T>(string serviceName, Func<Task<T>> operation)
    {
        return await _circuitBreakerService.ExecuteWithCircuitBreakerAsync(serviceName, operation);
    }

    public async Task<bool> IsServiceAvailableAsync(string serviceName)
    {
        var status = await _circuitBreakerService.GetCircuitBreakerStatusAsync(serviceName);
        return status.State == "Closed";
    }

    public async Task<CircuitBreakerStatus> GetCircuitBreakerStatusAsync(string serviceName)
    {
        return await _circuitBreakerService.GetCircuitBreakerStatusAsync(serviceName);
    }

    public async Task<object> TransformRequestAsync(object request, string sourceFormat, string targetFormat)
    {
        return await _messageTransformationService.TransformRequestAsync(request, sourceFormat, targetFormat);
    }

    public async Task<object> TransformResponseAsync(object response, string sourceFormat, string targetFormat)
    {
        return await _messageTransformationService.TransformResponseAsync(response, sourceFormat, targetFormat);
    }

    public async Task<bool> HandleDeadLetterAsync(ServiceBusMessage message, string errorReason)
    {
        return await _deadLetterQueueService.AddToDeadLetterAsync(message, errorReason);
    }

    public async Task<IEnumerable<ServiceBusMessage>> GetDeadLetterMessagesAsync(string serviceName)
    {
        return await _deadLetterQueueService.GetDeadLetterMessagesAsync(serviceName);
    }

    public async Task<bool> RetryMessageAsync(string messageId, int maxRetries = 3)
    {
        return await _deadLetterQueueService.RetryMessageAsync(messageId, maxRetries);
    }
} 