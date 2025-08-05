using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class DeadLetterQueueService : IDeadLetterQueueService
{
    private readonly ILogger<DeadLetterQueueService> _logger;
    private readonly Dictionary<string, List<DeadLetterMessage>> _deadLetterQueues;
    private readonly Dictionary<string, DeadLetterQueueMetrics> _metrics;
    private readonly object _queuesLock = new();
    private readonly object _metricsLock = new();

    public DeadLetterQueueService(ILogger<DeadLetterQueueService> logger)
    {
        _logger = logger;
        _deadLetterQueues = new Dictionary<string, List<DeadLetterMessage>>();
        _metrics = new Dictionary<string, DeadLetterQueueMetrics>();
    }

    public async Task<bool> AddToDeadLetterAsync(ServiceBusMessage message, string errorReason)
    {
        try
        {
            var deadLetterMessage = new DeadLetterMessage
            {
                Id = message.Id,
                OriginalMessage = message,
                ErrorReason = errorReason,
                AddedToQueueTime = DateTime.UtcNow,
                RetryCount = 0,
                LastRetryTime = null,
                IsArchived = false
            };

            lock (_queuesLock)
            {
                var serviceName = message.TargetService;
                if (!_deadLetterQueues.ContainsKey(serviceName))
                {
                    _deadLetterQueues[serviceName] = new List<DeadLetterMessage>();
                }

                _deadLetterQueues[serviceName].Add(deadLetterMessage);
            }

            UpdateMetrics(message.TargetService, "added", errorReason);
            
            _logger.LogWarning("Message {MessageId} added to dead letter queue for service {ServiceName}. Reason: {ErrorReason}", 
                message.Id, message.TargetService, errorReason);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add message {MessageId} to dead letter queue", message.Id);
            return false;
        }
    }

    public async Task<IEnumerable<ServiceBusMessage>> GetDeadLetterMessagesAsync(string serviceName)
    {
        lock (_queuesLock)
        {
            if (_deadLetterQueues.TryGetValue(serviceName, out var messages))
            {
                return messages
                    .Where(m => !m.IsArchived)
                    .Select(m => m.OriginalMessage)
                    .ToList();
            }
        }

        return new List<ServiceBusMessage>();
    }

    public async Task<bool> RetryMessageAsync(string messageId, int maxRetries = 3)
    {
        try
        {
            DeadLetterMessage? deadLetterMessage = null;
            string? serviceName = null;

            // Find the message in all queues
            lock (_queuesLock)
            {
                foreach (var kvp in _deadLetterQueues)
                {
                    var message = kvp.Value.FirstOrDefault(m => m.Id == messageId && !m.IsArchived);
                    if (message != null)
                    {
                        deadLetterMessage = message;
                        serviceName = kvp.Key;
                        break;
                    }
                }
            }

            if (deadLetterMessage == null || serviceName == null)
            {
                _logger.LogWarning("Message {MessageId} not found in dead letter queue", messageId);
                return false;
            }

            // Check if we've exceeded max retries
            if (deadLetterMessage.RetryCount >= maxRetries)
            {
                _logger.LogWarning("Message {MessageId} has exceeded max retries ({MaxRetries})", messageId, maxRetries);
                return false;
            }

            // Update retry count and timestamp
            deadLetterMessage.RetryCount++;
            deadLetterMessage.LastRetryTime = DateTime.UtcNow;

            // Simulate retry attempt
            var success = await SimulateRetryAsync(deadLetterMessage.OriginalMessage);
            
            if (success)
            {
                // Remove from dead letter queue on successful retry
                lock (_queuesLock)
                {
                    _deadLetterQueues[serviceName].Remove(deadLetterMessage);
                }

                UpdateMetrics(serviceName, "retried", deadLetterMessage.ErrorReason);
                _logger.LogInformation("Message {MessageId} successfully retried and removed from dead letter queue", messageId);
            }
            else
            {
                UpdateMetrics(serviceName, "retry_failed", deadLetterMessage.ErrorReason);
                _logger.LogWarning("Message {MessageId} retry failed, keeping in dead letter queue", messageId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry message {MessageId}", messageId);
            return false;
        }
    }

    public async Task<bool> RemoveFromDeadLetterAsync(string messageId)
    {
        try
        {
            DeadLetterMessage? deadLetterMessage = null;
            string? serviceName = null;

            lock (_queuesLock)
            {
                foreach (var kvp in _deadLetterQueues)
                {
                    var message = kvp.Value.FirstOrDefault(m => m.Id == messageId && !m.IsArchived);
                    if (message != null)
                    {
                        deadLetterMessage = message;
                        serviceName = kvp.Key;
                        break;
                    }
                }

                if (deadLetterMessage != null && serviceName != null)
                {
                    _deadLetterQueues[serviceName].Remove(deadLetterMessage);
                    UpdateMetrics(serviceName, "removed", deadLetterMessage.ErrorReason);
                    
                    _logger.LogInformation("Message {MessageId} removed from dead letter queue", messageId);
                    return true;
                }
            }

            _logger.LogWarning("Message {MessageId} not found in dead letter queue for removal", messageId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove message {MessageId} from dead letter queue", messageId);
            return false;
        }
    }

    public async Task<bool> ArchiveDeadLetterMessageAsync(string messageId)
    {
        try
        {
            DeadLetterMessage? deadLetterMessage = null;
            string? serviceName = null;

            lock (_queuesLock)
            {
                foreach (var kvp in _deadLetterQueues)
                {
                    var message = kvp.Value.FirstOrDefault(m => m.Id == messageId && !m.IsArchived);
                    if (message != null)
                    {
                        deadLetterMessage = message;
                        serviceName = kvp.Key;
                        break;
                    }
                }

                if (deadLetterMessage != null && serviceName != null)
                {
                    deadLetterMessage.IsArchived = true;
                    deadLetterMessage.ArchivedTime = DateTime.UtcNow;
                    
                    UpdateMetrics(serviceName, "archived", deadLetterMessage.ErrorReason);
                    
                    _logger.LogInformation("Message {MessageId} archived in dead letter queue", messageId);
                    return true;
                }
            }

            _logger.LogWarning("Message {MessageId} not found in dead letter queue for archiving", messageId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive message {MessageId} in dead letter queue", messageId);
            return false;
        }
    }

    public async Task<DeadLetterQueueMetrics> GetDeadLetterQueueMetricsAsync(string serviceName)
    {
        var metrics = GetOrCreateMetrics(serviceName);
        
        lock (_metricsLock)
        {
            return new DeadLetterQueueMetrics
            {
                ServiceName = serviceName,
                TotalMessages = metrics.TotalMessages,
                RetriedMessages = metrics.RetriedMessages,
                ArchivedMessages = metrics.ArchivedMessages,
                PurgedMessages = metrics.PurgedMessages,
                MessagesByErrorType = new Dictionary<string, int>(metrics.MessagesByErrorType),
                LastReset = metrics.LastReset
            };
        }
    }

    public async Task<IEnumerable<DeadLetterQueueMetrics>> GetAllDeadLetterQueueMetricsAsync()
    {
        var allMetrics = new List<DeadLetterQueueMetrics>();
        
        lock (_metricsLock)
        {
            foreach (var kvp in _metrics)
            {
                var metrics = kvp.Value;
                allMetrics.Add(new DeadLetterQueueMetrics
                {
                    ServiceName = metrics.ServiceName,
                    TotalMessages = metrics.TotalMessages,
                    RetriedMessages = metrics.RetriedMessages,
                    ArchivedMessages = metrics.ArchivedMessages,
                    PurgedMessages = metrics.PurgedMessages,
                    MessagesByErrorType = new Dictionary<string, int>(metrics.MessagesByErrorType),
                    LastReset = metrics.LastReset
                });
            }
        }

        return allMetrics;
    }

    public async Task<bool> PurgeDeadLetterQueueAsync(string serviceName, DateTime beforeDate)
    {
        try
        {
            var purgedCount = 0;
            
            lock (_queuesLock)
            {
                if (_deadLetterQueues.TryGetValue(serviceName, out var messages))
                {
                    var messagesToRemove = messages
                        .Where(m => m.AddedToQueueTime < beforeDate && !m.IsArchived)
                        .ToList();

                    foreach (var message in messagesToRemove)
                    {
                        messages.Remove(message);
                        purgedCount++;
                    }
                }
            }

            if (purgedCount > 0)
            {
                UpdateMetrics(serviceName, "purged", $"purged_{purgedCount}");
                _logger.LogInformation("Purged {PurgedCount} messages from dead letter queue for service {ServiceName}", 
                    purgedCount, serviceName);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge dead letter queue for service {ServiceName}", serviceName);
            return false;
        }
    }

    private async Task<bool> SimulateRetryAsync(ServiceBusMessage message)
    {
        // Simulate retry logic - in a real implementation, this would attempt to deliver the message
        await Task.Delay(100); // Simulate processing time
        
        // Simulate 80% success rate for retries
        var random = new Random();
        return random.Next(100) < 80;
    }

    private DeadLetterQueueMetrics GetOrCreateMetrics(string serviceName)
    {
        lock (_metricsLock)
        {
            if (!_metrics.TryGetValue(serviceName, out var metrics))
            {
                metrics = new DeadLetterQueueMetrics
                {
                    ServiceName = serviceName,
                    TotalMessages = 0,
                    RetriedMessages = 0,
                    ArchivedMessages = 0,
                    PurgedMessages = 0,
                    MessagesByErrorType = new Dictionary<string, int>(),
                    LastReset = DateTime.UtcNow
                };
                _metrics[serviceName] = metrics;
            }
            return metrics;
        }
    }

    private void UpdateMetrics(string serviceName, string action, string errorType)
    {
        lock (_metricsLock)
        {
            var metrics = GetOrCreateMetrics(serviceName);
            
            switch (action)
            {
                case "added":
                    metrics.TotalMessages++;
                    break;
                case "retried":
                    metrics.RetriedMessages++;
                    break;
                case "archived":
                    metrics.ArchivedMessages++;
                    break;
                case "purged":
                    metrics.PurgedMessages++;
                    break;
            }

            if (metrics.MessagesByErrorType.ContainsKey(errorType))
            {
                metrics.MessagesByErrorType[errorType]++;
            }
            else
            {
                metrics.MessagesByErrorType[errorType] = 1;
            }
        }
    }

    private class DeadLetterMessage
    {
        public string Id { get; set; } = string.Empty;
        public ServiceBusMessage OriginalMessage { get; set; } = new();
        public string ErrorReason { get; set; } = string.Empty;
        public DateTime AddedToQueueTime { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;
        public DateTime? LastRetryTime { get; set; }
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedTime { get; set; }
    }
} 