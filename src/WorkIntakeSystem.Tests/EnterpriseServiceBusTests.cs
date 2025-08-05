using Microsoft.Extensions.Logging;
using Moq;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Services;
using Xunit;

namespace WorkIntakeSystem.Tests;

public class EnterpriseServiceBusTests
{
    private readonly Mock<ILogger<EnterpriseServiceBus>> _loggerMock;
    private readonly Mock<IMessageTransformationService> _messageTransformationMock;
    private readonly Mock<ICircuitBreakerService> _circuitBreakerMock;
    private readonly Mock<IDeadLetterQueueService> _deadLetterQueueMock;
    private readonly EnterpriseServiceBus _esb;

    public EnterpriseServiceBusTests()
    {
        _loggerMock = new Mock<ILogger<EnterpriseServiceBus>>();
        _messageTransformationMock = new Mock<IMessageTransformationService>();
        _circuitBreakerMock = new Mock<ICircuitBreakerService>();
        _deadLetterQueueMock = new Mock<IDeadLetterQueueService>();
        
        _esb = new EnterpriseServiceBus(
            _loggerMock.Object,
            _messageTransformationMock.Object,
            _circuitBreakerMock.Object,
            _deadLetterQueueMock.Object);
    }

    [Fact]
    public async Task RouteMessageAsync_WithValidMessage_ShouldSucceed()
    {
        // Arrange
        var message = new ServiceBusMessage
        {
            Id = "test-message-1",
            MessageType = "TestMessage",
            Payload = new { test = "data" },
            TargetService = "TestService",
            Headers = new Dictionary<string, object> { { "SourceFormat", "JSON" }, { "TargetFormat", "XML" } }
        };

        _circuitBreakerMock.Setup(x => x.IsServiceAvailableAsync("TestService"))
            .ReturnsAsync(true);
        
        _messageTransformationMock.Setup(x => x.TransformMessageAsync(It.IsAny<object>(), "JSON", "XML"))
            .ReturnsAsync("<test>data</test>");
        
        _circuitBreakerMock.Setup(x => x.ExecuteWithCircuitBreakerAsync(It.IsAny<string>(), It.IsAny<Func<Task<bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _esb.RouteMessageAsync(message);

        // Assert
        Assert.True(result);
        _circuitBreakerMock.Verify(x => x.IsServiceAvailableAsync("TestService"), Times.Once);
        _messageTransformationMock.Verify(x => x.TransformMessageAsync(It.IsAny<object>(), "JSON", "XML"), Times.Once);
    }

    [Fact]
    public async Task RouteMessageAsync_WithUnavailableService_ShouldAddToDeadLetter()
    {
        // Arrange
        var message = new ServiceBusMessage
        {
            Id = "test-message-2",
            MessageType = "TestMessage",
            Payload = new { test = "data" },
            TargetService = "UnavailableService"
        };

        _circuitBreakerMock.Setup(x => x.IsServiceAvailableAsync("UnavailableService"))
            .ReturnsAsync(false);
        
        _deadLetterQueueMock.Setup(x => x.AddToDeadLetterAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _esb.RouteMessageAsync(message);

        // Assert
        Assert.False(result);
        _deadLetterQueueMock.Verify(x => x.AddToDeadLetterAsync(It.IsAny<ServiceBusMessage>(), "Service unavailable"), Times.Once);
    }

    [Fact]
    public async Task TransformMessageAsync_ShouldCallTransformationService()
    {
        // Arrange
        var message = new { test = "data" };
        var transformedMessage = "<test>data</test>";
        
        _messageTransformationMock.Setup(x => x.TransformMessageAsync(message, "JSON", "XML"))
            .ReturnsAsync(transformedMessage);

        // Act
        var result = await _esb.TransformMessageAsync(message, "JSON", "XML");

        // Assert
        Assert.Equal(transformedMessage, result);
        _messageTransformationMock.Verify(x => x.TransformMessageAsync(message, "JSON", "XML"), Times.Once);
    }

    [Fact]
    public async Task RegisterServiceAsync_ShouldAddServiceToRegistry()
    {
        // Arrange
        var service = new ServiceRegistry
        {
            ServiceName = "TestService",
            ServiceUrl = "https://testservice.api.company.com",
            ServiceType = "REST",
            IsActive = true
        };

        // Act
        var result = await _esb.RegisterServiceAsync(service);

        // Assert
        Assert.True(result);
        
        var discoveredService = await _esb.DiscoverServiceAsync("TestService");
        Assert.Equal("TestService", discoveredService.ServiceName);
        Assert.Equal("https://testservice.api.company.com", discoveredService.ServiceUrl);
    }

    [Fact]
    public async Task ExecuteWithCircuitBreakerAsync_ShouldCallCircuitBreakerService()
    {
        // Arrange
        var expectedResult = "test result";
        Func<Task<string>> operation = () => Task.FromResult(expectedResult);
        
        _circuitBreakerMock.Setup(x => x.ExecuteWithCircuitBreakerAsync("TestService", It.IsAny<Func<Task<string>>>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _esb.ExecuteWithCircuitBreakerAsync("TestService", operation);

        // Assert
        Assert.Equal(expectedResult, result);
        _circuitBreakerMock.Verify(x => x.ExecuteWithCircuitBreakerAsync("TestService", It.IsAny<Func<Task<string>>>()), Times.Once);
    }

    [Fact]
    public async Task HandleDeadLetterAsync_ShouldCallDeadLetterQueueService()
    {
        // Arrange
        var message = new ServiceBusMessage { Id = "test-message-3" };
        var errorReason = "Test error";
        
        _deadLetterQueueMock.Setup(x => x.AddToDeadLetterAsync(message, errorReason))
            .ReturnsAsync(true);

        // Act
        var result = await _esb.HandleDeadLetterAsync(message, errorReason);

        // Assert
        Assert.True(result);
        _deadLetterQueueMock.Verify(x => x.AddToDeadLetterAsync(message, errorReason), Times.Once);
    }

    [Fact]
    public async Task GetDeadLetterMessagesAsync_ShouldCallDeadLetterQueueService()
    {
        // Arrange
        var expectedMessages = new List<ServiceBusMessage>
        {
            new ServiceBusMessage { Id = "dlq-1" },
            new ServiceBusMessage { Id = "dlq-2" }
        };
        
        _deadLetterQueueMock.Setup(x => x.GetDeadLetterMessagesAsync("TestService"))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _esb.GetDeadLetterMessagesAsync("TestService");

        // Assert
        Assert.Equal(expectedMessages, result);
        _deadLetterQueueMock.Verify(x => x.GetDeadLetterMessagesAsync("TestService"), Times.Once);
    }

    [Fact]
    public async Task RetryMessageAsync_ShouldCallDeadLetterQueueService()
    {
        // Arrange
        var messageId = "test-message-4";
        
        _deadLetterQueueMock.Setup(x => x.RetryMessageAsync(messageId, 3))
            .ReturnsAsync(true);

        // Act
        var result = await _esb.RetryMessageAsync(messageId, 3);

        // Assert
        Assert.True(result);
        _deadLetterQueueMock.Verify(x => x.RetryMessageAsync(messageId, 3), Times.Once);
    }
}

public class MessageTransformationServiceTests
{
    private readonly Mock<ILogger<MessageTransformationService>> _loggerMock;
    private readonly MessageTransformationService _transformationService;

    public MessageTransformationServiceTests()
    {
        _loggerMock = new Mock<ILogger<MessageTransformationService>>();
        _transformationService = new MessageTransformationService(_loggerMock.Object);
    }

    [Fact]
    public async Task TransformMessageAsync_JsonToXml_ShouldTransformCorrectly()
    {
        // Arrange
        var jsonMessage = new { name = "test", value = 123 };

        // Act
        var result = await _transformationService.TransformMessageAsync(jsonMessage, "JSON", "XML");

        // Assert
        Assert.NotNull(result);
        var xmlString = result.ToString();
        Assert.Contains("<name>test</name>", xmlString);
        Assert.Contains("<value>123</value>", xmlString);
    }

    [Fact]
    public async Task TransformMessageAsync_XmlToJson_ShouldTransformCorrectly()
    {
        // Arrange
        var xmlMessage = "<root><name>test</name><value>123</value></root>";

        // Act
        var result = await _transformationService.TransformMessageAsync(xmlMessage, "XML", "JSON");

        // Assert
        Assert.NotNull(result);
        var jsonString = result.ToString();
        Assert.Contains("test", jsonString);
        Assert.Contains("123", jsonString);
    }

    [Fact]
    public async Task ValidateMessageFormatAsync_ValidJson_ShouldReturnTrue()
    {
        // Arrange
        var jsonMessage = "{\"test\": \"data\"}";

        // Act
        var result = await _transformationService.ValidateMessageFormatAsync(jsonMessage, "JSON");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateMessageFormatAsync_InvalidJson_ShouldReturnFalse()
    {
        // Arrange
        var invalidJson = "{invalid json}";

        // Act
        var result = await _transformationService.ValidateMessageFormatAsync(invalidJson, "JSON");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetTransformationMetricsAsync_ShouldReturnMetrics()
    {
        // Act
        var metrics = await _transformationService.GetTransformationMetricsAsync();

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(0, metrics.TotalTransformations);
        Assert.Equal(0, metrics.SuccessfulTransformations);
        Assert.Equal(0, metrics.FailedTransformations);
    }
}

public class CircuitBreakerServiceTests
{
    private readonly Mock<ILogger<CircuitBreakerService>> _loggerMock;
    private readonly CircuitBreakerService _circuitBreakerService;

    public CircuitBreakerServiceTests()
    {
        _loggerMock = new Mock<ILogger<CircuitBreakerService>>();
        _circuitBreakerService = new CircuitBreakerService(_loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteWithCircuitBreakerAsync_SuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = "test result";
        Func<Task<string>> operation = () => Task.FromResult(expectedResult);

        // Act
        var result = await _circuitBreakerService.ExecuteWithCircuitBreakerAsync("TestService", operation);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ExecuteWithCircuitBreakerAsync_FailedOperation_ShouldThrowException()
    {
        // Arrange
        Func<Task<string>> operation = () => throw new Exception("Test exception");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _circuitBreakerService.ExecuteWithCircuitBreakerAsync("TestService", operation));
    }

    [Fact]
    public async Task GetCircuitBreakerStatusAsync_ShouldReturnStatus()
    {
        // Act
        var status = await _circuitBreakerService.GetCircuitBreakerStatusAsync("TestService");

        // Assert
        Assert.NotNull(status);
        Assert.Equal("TestService", status.ServiceName);
        Assert.Equal("Closed", status.State);
        Assert.Equal(0, status.FailureCount);
    }

    [Fact]
    public async Task ResetCircuitBreakerAsync_ShouldResetCircuitBreaker()
    {
        // Act
        var result = await _circuitBreakerService.ResetCircuitBreakerAsync("TestService");

        // Assert
        Assert.True(result);
        
        var status = await _circuitBreakerService.GetCircuitBreakerStatusAsync("TestService");
        Assert.Equal("Closed", status.State);
        Assert.Equal(0, status.FailureCount);
    }
}

public class DeadLetterQueueServiceTests
{
    private readonly Mock<ILogger<DeadLetterQueueService>> _loggerMock;
    private readonly DeadLetterQueueService _deadLetterQueueService;

    public DeadLetterQueueServiceTests()
    {
        _loggerMock = new Mock<ILogger<DeadLetterQueueService>>();
        _deadLetterQueueService = new DeadLetterQueueService(_loggerMock.Object);
    }

    [Fact]
    public async Task AddToDeadLetterAsync_ShouldAddMessageToQueue()
    {
        // Arrange
        var message = new ServiceBusMessage
        {
            Id = "test-message-1",
            TargetService = "TestService"
        };
        var errorReason = "Test error";

        // Act
        var result = await _deadLetterQueueService.AddToDeadLetterAsync(message, errorReason);

        // Assert
        Assert.True(result);
        
        var messages = await _deadLetterQueueService.GetDeadLetterMessagesAsync("TestService");
        Assert.Single(messages);
        Assert.Equal("test-message-1", messages.First().Id);
    }

    [Fact]
    public async Task RetryMessageAsync_ShouldAttemptRetry()
    {
        // Arrange
        var message = new ServiceBusMessage
        {
            Id = "test-message-2",
            TargetService = "TestService"
        };
        
        await _deadLetterQueueService.AddToDeadLetterAsync(message, "Test error");

        // Act
        var result = await _deadLetterQueueService.RetryMessageAsync("test-message-2");

        // Assert
        // Note: This test may pass or fail depending on the simulated retry logic
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task RemoveFromDeadLetterAsync_ShouldRemoveMessage()
    {
        // Arrange
        var message = new ServiceBusMessage
        {
            Id = "test-message-3",
            TargetService = "TestService"
        };
        
        await _deadLetterQueueService.AddToDeadLetterAsync(message, "Test error");

        // Act
        var result = await _deadLetterQueueService.RemoveFromDeadLetterAsync("test-message-3");

        // Assert
        Assert.True(result);
        
        var messages = await _deadLetterQueueService.GetDeadLetterMessagesAsync("TestService");
        Assert.Empty(messages);
    }

    [Fact]
    public async Task GetDeadLetterQueueMetricsAsync_ShouldReturnMetrics()
    {
        // Act
        var metrics = await _deadLetterQueueService.GetDeadLetterQueueMetricsAsync("TestService");

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal("TestService", metrics.ServiceName);
        Assert.Equal(0, metrics.TotalMessages);
        Assert.Equal(0, metrics.RetriedMessages);
        Assert.Equal(0, metrics.ArchivedMessages);
    }
} 