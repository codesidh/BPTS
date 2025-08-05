using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Services;

// Demonstration of Enterprise Service Bus Implementation
// This script shows how to use the ESB with its three main components:
// 1. Message Transformation Engine
// 2. Circuit Breaker Service  
// 3. Dead Letter Queue Service

class ESBDemonstration
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Enterprise Service Bus Demonstration ===\n");

        // Create logger (in real app, this would be injected)
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ESBDemonstration>();

        // Initialize ESB Services
        var messageTransformationService = new MessageTransformationService(logger);
        var circuitBreakerService = new CircuitBreakerService(logger);
        var deadLetterQueueService = new DeadLetterQueueService(logger);
        var enterpriseServiceBus = new EnterpriseServiceBus(logger, messageTransformationService, circuitBreakerService, deadLetterQueueService);

        // 1. Message Transformation Demonstration
        Console.WriteLine("1. Message Transformation Engine Demo");
        Console.WriteLine("=====================================");
        
        // JSON to XML transformation
        var jsonMessage = new { name = "John Doe", age = 30, department = "IT" };
        Console.WriteLine($"Original JSON: {System.Text.Json.JsonSerializer.Serialize(jsonMessage)}");
        
        var xmlResult = await messageTransformationService.TransformMessageAsync(jsonMessage, "JSON", "XML");
        Console.WriteLine($"Transformed to XML: {xmlResult}");
        
        // XML to JSON transformation
        var xmlMessage = "<root><name>Jane Smith</name><age>25</age><department>HR</department></root>";
        Console.WriteLine($"\nOriginal XML: {xmlMessage}");
        
        var jsonResult = await messageTransformationService.TransformMessageAsync(xmlMessage, "XML", "JSON");
        Console.WriteLine($"Transformed to JSON: {jsonResult}");
        
        // Format validation
        var isValidJson = await messageTransformationService.ValidateMessageFormatAsync("{\"test\": \"data\"}", "JSON");
        Console.WriteLine($"\nValid JSON format: {isValidJson}");
        
        var isValidXml = await messageTransformationService.ValidateMessageFormatAsync("<test>data</test>", "XML");
        Console.WriteLine($"Valid XML format: {isValidXml}");

        // 2. Circuit Breaker Demonstration
        Console.WriteLine("\n\n2. Circuit Breaker Service Demo");
        Console.WriteLine("================================");
        
        // Successful operation
        Console.WriteLine("Testing successful operation...");
        var successResult = await circuitBreakerService.ExecuteWithCircuitBreakerAsync("TestService", 
            async () => 
            {
                await Task.Delay(100); // Simulate work
                return "Operation successful";
            });
        Console.WriteLine($"Result: {successResult}");
        
        // Check circuit breaker status
        var status = await circuitBreakerService.GetCircuitBreakerStatusAsync("TestService");
        Console.WriteLine($"Circuit Breaker State: {status.State}");
        Console.WriteLine($"Failure Count: {status.FailureCount}");
        
        // Failed operation (will increment failure count)
        Console.WriteLine("\nTesting failed operation...");
        try
        {
            await circuitBreakerService.ExecuteWithCircuitBreakerAsync("TestService", 
                async () => 
                {
                    await Task.Delay(100);
                    throw new Exception("Simulated failure");
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Expected failure: {ex.Message}");
        }
        
        // Check updated status
        status = await circuitBreakerService.GetCircuitBreakerStatusAsync("TestService");
        Console.WriteLine($"Updated Circuit Breaker State: {status.State}");
        Console.WriteLine($"Updated Failure Count: {status.FailureCount}");

        // 3. Dead Letter Queue Demonstration
        Console.WriteLine("\n\n3. Dead Letter Queue Service Demo");
        Console.WriteLine("===================================");
        
        // Create a test message
        var testMessage = new ServiceBusMessage
        {
            Id = "test-message-001",
            MessageType = "TestMessage",
            Payload = new { data = "test data" },
            TargetService = "TestService",
            Timestamp = DateTime.UtcNow
        };
        
        // Add message to dead letter queue
        Console.WriteLine("Adding message to dead letter queue...");
        var added = await deadLetterQueueService.AddToDeadLetterAsync(testMessage, "Service unavailable");
        Console.WriteLine($"Message added: {added}");
        
        // Get messages from dead letter queue
        var messages = await deadLetterQueueService.GetDeadLetterMessagesAsync("TestService");
        Console.WriteLine($"Messages in dead letter queue: {messages.Count()}");
        
        // Retry message
        Console.WriteLine("Attempting to retry message...");
        var retryResult = await deadLetterQueueService.RetryMessageAsync("test-message-001");
        Console.WriteLine($"Retry result: {retryResult}");
        
        // Check metrics
        var metrics = await deadLetterQueueService.GetDeadLetterQueueMetricsAsync("TestService");
        Console.WriteLine($"Total messages: {metrics.TotalMessages}");
        Console.WriteLine($"Retried messages: {metrics.RetriedMessages}");

        // 4. Enterprise Service Bus Integration Demo
        Console.WriteLine("\n\n4. Enterprise Service Bus Integration Demo");
        Console.WriteLine("===========================================");
        
        // Register a service
        var service = new ServiceRegistry
        {
            ServiceName = "DemoService",
            ServiceUrl = "https://demo.api.company.com",
            ServiceType = "REST",
            IsActive = true
        };
        
        var registered = await enterpriseServiceBus.RegisterServiceAsync(service);
        Console.WriteLine($"Service registered: {registered}");
        
        // Discover service
        var discoveredService = await enterpriseServiceBus.DiscoverServiceAsync("DemoService");
        Console.WriteLine($"Discovered service: {discoveredService.ServiceName} at {discoveredService.ServiceUrl}");
        
        // Route a message through the ESB
        var esbMessage = new ServiceBusMessage
        {
            Id = "esb-message-001",
            MessageType = "WorkRequest",
            Payload = new { requestId = "req-123", priority = "High" },
            TargetService = "DemoService",
            Headers = new Dictionary<string, object> { { "SourceFormat", "JSON" }, { "TargetFormat", "XML" } }
        };
        
        Console.WriteLine("Routing message through ESB...");
        var routed = await enterpriseServiceBus.RouteMessageAsync(esbMessage);
        Console.WriteLine($"Message routed: {routed}");

        Console.WriteLine("\n=== ESB Demonstration Complete ===");
        Console.WriteLine("The Enterprise Service Bus provides:");
        Console.WriteLine("✓ Message transformation between formats (JSON/XML/CSV)");
        Console.WriteLine("✓ Circuit breaker pattern for fault tolerance");
        Console.WriteLine("✓ Dead letter queue for failed message handling");
        Console.WriteLine("✓ Service discovery and registration");
        Console.WriteLine("✓ Comprehensive monitoring and metrics");
    }
} 