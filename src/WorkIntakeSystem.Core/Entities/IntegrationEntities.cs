using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Core.Entities;

// Additional Integration Entities (not already defined in other files)


// Microsoft 365 Integration Entities
public class Microsoft365User
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string OfficeLocation { get; set; } = string.Empty;
    public bool AccountEnabled { get; set; }
    public DateTime CreatedDateTime { get; set; }
}

public class Microsoft365Team
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
    public List<Microsoft365User> Members { get; set; } = new();
    public List<Microsoft365Channel> Channels { get; set; } = new();
}

public class Microsoft365Channel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MembershipType { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
}

public class Microsoft365CalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<Microsoft365Attendee> Attendees { get; set; } = new();
    public bool IsAllDay { get; set; }
    public string Importance { get; set; } = string.Empty;
    public string Sensitivity { get; set; } = string.Empty;
}

public class Microsoft365Attendee
{
    public string EmailAddress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // required, optional, resource
    public string Status { get; set; } = string.Empty; // none, accepted, declined, tentative
}

// Financial Systems Integration Entities
public class FinancialProject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal RemainingBudget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ProjectManager { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class FinancialTransaction
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty; // expense, revenue, transfer
    public string Category { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

// Monitoring and Observability Entities
public class ApplicationMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public string Source { get; set; } = string.Empty;
}

public class HealthCheckResult
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // healthy, unhealthy, degraded
    public DateTime CheckTime { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

public class ElasticsearchDocument
{
    public string Id { get; set; } = string.Empty;
    public string Index { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Source { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public double Score { get; set; }
}

// Enterprise Service Bus Entities
public class ESBMessage
{
    public string Id { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, object> Headers { get; set; } = new();
    public object Body { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty; // pending, processing, completed, failed
    public int RetryCount { get; set; }
    public DateTime? ExpirationTime { get; set; }
}

public class CircuitBreakerState
{
    public string ServiceName { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // closed, open, half-open
    public int FailureCount { get; set; }
    public int SuccessCount { get; set; }
    public DateTime LastFailureTime { get; set; }
    public DateTime LastSuccessTime { get; set; }
    public TimeSpan Timeout { get; set; }
    public int Threshold { get; set; }
}

public class DeadLetterMessage
{
    public string Id { get; set; } = string.Empty;
    public string OriginalMessageId { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime DeadLetterTime { get; set; }
    public int RetryCount { get; set; }
    public object OriginalMessage { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
