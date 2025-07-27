namespace WorkIntakeSystem.Core.Interfaces;

public interface IServiceBrokerService
{
    Task<bool> SendMessageAsync(string messageType, object messageBody, string? targetService = null);
    Task<ServiceBrokerMessage?> ReceiveMessageAsync(TimeSpan? timeout = null);
    Task<IEnumerable<ServiceBrokerMessage>> ReceiveMessagesAsync(int maxMessages = 10, TimeSpan? timeout = null);
    Task ProcessMessagesAsync(Func<ServiceBrokerMessage, Task<bool>> messageHandler, CancellationToken cancellationToken = default);
    Task<bool> BeginConversationAsync(string targetService, string contract);
    Task<bool> EndConversationAsync(Guid conversationHandle);
    Task<bool> CreateQueueIfNotExistsAsync();
    Task<bool> CreateServiceIfNotExistsAsync();
    Task<ServiceBrokerStats> GetServiceStatsAsync();
}

public interface IMessageHandlerRegistry
{
    void RegisterHandler<T>(string messageType, Func<T, Task<bool>> handler) where T : class;
    void RegisterHandler(string messageType, Func<ServiceBrokerMessage, Task<bool>> handler);
    Task<bool> HandleMessageAsync(ServiceBrokerMessage message);
    IEnumerable<string> GetRegisteredMessageTypes();
}

public class ServiceBrokerMessage
{
    public Guid ConversationHandle { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int Priority { get; set; } = 5;
    public string SourceService { get; set; } = string.Empty;
    public string TargetService { get; set; } = string.Empty;
    public Dictionary<string, object> Headers { get; set; } = new();
}

public class ServiceBrokerStats
{
    public int QueueLength { get; set; }
    public int ActiveConversations { get; set; }
    public int MessagesProcessedToday { get; set; }
    public int MessagesSentToday { get; set; }
    public int ErrorCount { get; set; }
    public DateTime LastMessageProcessed { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
} 