namespace WorkIntakeSystem.Core.Interfaces;

public interface IEnterpriseServiceBus
{
    // Message Routing
    Task<bool> RouteMessageAsync(string messageType, object message, string targetService);
    Task<bool> RouteMessageAsync(ServiceBusMessage message);
    Task<IEnumerable<ServiceBusMessage>> RouteMessagesAsync(IEnumerable<ServiceBusMessage> messages);
    
    // Protocol Translation
    Task<object> TransformMessageAsync(object message, string sourceFormat, string targetFormat);
    Task<bool> ValidateMessageFormatAsync(object message, string expectedFormat);
    
    // Service Discovery
    Task<ServiceRegistry> DiscoverServiceAsync(string serviceName);
    Task<IEnumerable<ServiceRegistry>> GetAllServicesAsync();
    Task<bool> RegisterServiceAsync(ServiceRegistry service);
    Task<bool> UnregisterServiceAsync(string serviceName);
    
    // Circuit Breaker
    Task<T> ExecuteWithCircuitBreakerAsync<T>(string serviceName, Func<Task<T>> operation);
    Task<bool> IsServiceAvailableAsync(string serviceName);
    Task<CircuitBreakerStatus> GetCircuitBreakerStatusAsync(string serviceName);
    
    // Message Transformation
    Task<object> TransformRequestAsync(object request, string sourceFormat, string targetFormat);
    Task<object> TransformResponseAsync(object response, string sourceFormat, string targetFormat);
    
    // Error Handling
    Task<bool> HandleDeadLetterAsync(ServiceBusMessage message, string errorReason);
    Task<IEnumerable<ServiceBusMessage>> GetDeadLetterMessagesAsync(string serviceName);
    Task<bool> RetryMessageAsync(string messageId, int maxRetries = 3);
}

public class ServiceBusMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MessageType { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public string SourceService { get; set; } = string.Empty;
    public string TargetService { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string CausationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Headers { get; set; } = new();
}

public class ServiceRegistry
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class CircuitBreakerStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public string State { get; set; } = "Closed"; // Closed, Open, HalfOpen
    public int FailureCount { get; set; } = 0;
    public DateTime LastFailureTime { get; set; } = DateTime.UtcNow;
    public DateTime NextAttemptTime { get; set; } = DateTime.UtcNow;
    public int Threshold { get; set; } = 5;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
} 