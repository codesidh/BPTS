using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class ServiceBrokerService : IServiceBrokerService
{
    private readonly ILogger<ServiceBrokerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly string _queueName;
    private readonly string _serviceName;
    private readonly string _contractName;
    private readonly int _messageTimeout;
    private readonly bool _isEnabled;

    public ServiceBrokerService(
        ILogger<ServiceBrokerService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Database connection string not configured");
        
        var serviceBrokerConfig = configuration.GetSection("ServiceBroker");
        _queueName = serviceBrokerConfig["QueueName"] ?? "WorkIntakeQueue";
        _serviceName = serviceBrokerConfig["ServiceName"] ?? "WorkIntakeService";
        _contractName = serviceBrokerConfig["ContractName"] ?? "WorkIntakeContract";
        _messageTimeout = serviceBrokerConfig.GetValue<int>("MessageTimeout", 30000);
        _isEnabled = serviceBrokerConfig.GetValue<bool>("Enabled", true);
    }

    public async Task<bool> SendMessageAsync(string messageType, object messageBody, string? targetService = null)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("Service Broker is disabled");
            return false;
        }

        try
        {
            var serializedBody = JsonSerializer.Serialize(messageBody);
            var target = targetService ?? _serviceName;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                DECLARE @conversation_handle uniqueidentifier;
                
                BEGIN DIALOG CONVERSATION @conversation_handle
                FROM SERVICE @source_service
                TO SERVICE @target_service
                ON CONTRACT [@contract_name]
                WITH ENCRYPTION = OFF;
                
                SEND ON CONVERSATION @conversation_handle
                MESSAGE TYPE [@message_type] (@message_body);
                
                SELECT @conversation_handle;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@source_service", _serviceName);
            command.Parameters.AddWithValue("@target_service", target);
            command.Parameters.AddWithValue("@contract_name", _contractName);
            command.Parameters.AddWithValue("@message_type", messageType);
            command.Parameters.AddWithValue("@message_body", serializedBody);

            var conversationHandle = await command.ExecuteScalarAsync();
            
            _logger.LogInformation("Sent Service Broker message: Type={MessageType}, Conversation={ConversationHandle}",
                messageType, conversationHandle);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Service Broker message: Type={MessageType}", messageType);
            return false;
        }
    }

    public async Task<ServiceBrokerMessage?> ReceiveMessageAsync(TimeSpan? timeout = null)
    {
        if (!_isEnabled)
        {
            return null;
        }

        var messages = await ReceiveMessagesAsync(1, timeout);
        return messages.FirstOrDefault();
    }

    public async Task<IEnumerable<ServiceBrokerMessage>> ReceiveMessagesAsync(int maxMessages = 10, TimeSpan? timeout = null)
    {
        if (!_isEnabled)
        {
            return Enumerable.Empty<ServiceBrokerMessage>();
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var timeoutMs = (int)(timeout?.TotalMilliseconds ?? _messageTimeout);
            var sql = $@"
                RECEIVE TOP ({maxMessages})
                    conversation_handle,
                    message_type_name,
                    message_body,
                    message_sequence_number
                FROM [{_queueName}]
                TIMEOUT {timeoutMs};";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            var messages = new List<ServiceBrokerMessage>();
            while (await reader.ReadAsync())
            {
                var message = new ServiceBrokerMessage
                {
                    ConversationHandle = reader.GetGuid("conversation_handle"),
                    MessageType = reader.GetString("message_type_name"),
                    MessageBody = reader.IsDBNull("message_body") ? string.Empty : 
                        System.Text.Encoding.Unicode.GetString((byte[])reader["message_body"]),
                    CreatedDate = DateTime.UtcNow,
                    SourceService = _serviceName
                };

                messages.Add(message);
            }

            if (messages.Any())
            {
                _logger.LogInformation("Received {MessageCount} Service Broker messages", messages.Count);
            }

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive Service Broker messages");
            return Enumerable.Empty<ServiceBrokerMessage>();
        }
    }

    public async Task ProcessMessagesAsync(Func<ServiceBrokerMessage, Task<bool>> messageHandler, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("Service Broker is disabled - message processing skipped");
            return;
        }

        _logger.LogInformation("Starting Service Broker message processing");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var messages = await ReceiveMessagesAsync(10, TimeSpan.FromSeconds(30));
                
                foreach (var message in messages)
                {
                    try
                    {
                        var success = await messageHandler(message);
                        if (success)
                        {
                            _logger.LogDebug("Successfully processed message: {MessageType}", message.MessageType);
                        }
                        else
                        {
                            _logger.LogWarning("Message handler returned false for: {MessageType}", message.MessageType);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message: {MessageType}", message.MessageType);
                    }
                }

                if (!messages.Any())
                {
                    // No messages received, wait a bit before trying again
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Service Broker message processing cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Service Broker message processing loop");
        }
    }

    public async Task<bool> BeginConversationAsync(string targetService, string contract)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                DECLARE @conversation_handle uniqueidentifier;
                
                BEGIN DIALOG CONVERSATION @conversation_handle
                FROM SERVICE @source_service
                TO SERVICE @target_service
                ON CONTRACT [@contract_name]
                WITH ENCRYPTION = OFF;
                
                SELECT @conversation_handle;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@source_service", _serviceName);
            command.Parameters.AddWithValue("@target_service", targetService);
            command.Parameters.AddWithValue("@contract_name", contract);

            var conversationHandle = await command.ExecuteScalarAsync();
            _logger.LogInformation("Started conversation: {ConversationHandle}", conversationHandle);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to begin conversation with service: {TargetService}", targetService);
            return false;
        }
    }

    public async Task<bool> EndConversationAsync(Guid conversationHandle)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "END CONVERSATION @conversation_handle;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@conversation_handle", conversationHandle);

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("Ended conversation: {ConversationHandle}", conversationHandle);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end conversation: {ConversationHandle}", conversationHandle);
            return false;
        }
    }

    public async Task<bool> CreateQueueIfNotExistsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $@"
                IF NOT EXISTS (SELECT * FROM sys.service_queues WHERE name = '{_queueName}')
                BEGIN
                    CREATE QUEUE [{_queueName}];
                END";

            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
            
            _logger.LogInformation("Service Broker queue ensured: {QueueName}", _queueName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Service Broker queue: {QueueName}", _queueName);
            return false;
        }
    }

    public async Task<bool> CreateServiceIfNotExistsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $@"
                IF NOT EXISTS (SELECT * FROM sys.services WHERE name = '{_serviceName}')
                BEGIN
                    CREATE SERVICE [{_serviceName}] ON QUEUE [{_queueName}];
                END";

            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
            
            _logger.LogInformation("Service Broker service ensured: {ServiceName}", _serviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Service Broker service: {ServiceName}", _serviceName);
            return false;
        }
    }

    public async Task<ServiceBrokerStats> GetServiceStatsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $@"
                SELECT 
                    (SELECT COUNT(*) FROM [{_queueName}]) as QueueLength,
                    (SELECT COUNT(*) FROM sys.conversation_endpoints WHERE state_desc = 'CONVERSING') as ActiveConversations;";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ServiceBrokerStats
                {
                    QueueLength = reader.GetInt32("QueueLength"),
                    ActiveConversations = reader.GetInt32("ActiveConversations"),
                    LastMessageProcessed = DateTime.UtcNow,
                    // TODO: Implement additional stats tracking
                };
            }

            return new ServiceBrokerStats();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Service Broker stats");
            return new ServiceBrokerStats();
        }
    }
}

public class MessageHandlerRegistry : IMessageHandlerRegistry
{
    private readonly Dictionary<string, Func<ServiceBrokerMessage, Task<bool>>> _handlers = new();
    private readonly ILogger<MessageHandlerRegistry> _logger;

    public MessageHandlerRegistry(ILogger<MessageHandlerRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterHandler<T>(string messageType, Func<T, Task<bool>> handler) where T : class
    {
        _handlers[messageType] = async (message) =>
        {
            try
            {
                var typedMessage = JsonSerializer.Deserialize<T>(message.MessageBody);
                if (typedMessage != null)
                {
                    return await handler(typedMessage);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing message for type: {MessageType}", messageType);
                return false;
            }
        };

        _logger.LogInformation("Registered typed handler for message type: {MessageType}", messageType);
    }

    public void RegisterHandler(string messageType, Func<ServiceBrokerMessage, Task<bool>> handler)
    {
        _handlers[messageType] = handler;
        _logger.LogInformation("Registered handler for message type: {MessageType}", messageType);
    }

    public async Task<bool> HandleMessageAsync(ServiceBrokerMessage message)
    {
        if (_handlers.TryGetValue(message.MessageType, out var handler))
        {
            try
            {
                return await handler(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message type: {MessageType}", message.MessageType);
                return false;
            }
        }

        _logger.LogWarning("No handler registered for message type: {MessageType}", message.MessageType);
        return false;
    }

    public IEnumerable<string> GetRegisteredMessageTypes()
    {
        return _handlers.Keys;
    }
} 