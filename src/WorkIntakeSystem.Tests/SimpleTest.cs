using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace WorkIntakeSystem.Tests
{
    public class SimpleTest
    {
        [Fact]
        public void BasicTest_ShouldPass()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SimpleTest>>();
            
            // Act
            var result = true;
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void MockTest_ShouldWork()
        {
            // Arrange
            var mockService = new Mock<ITestService>();
            mockService.Setup(x => x.GetValue()).Returns("test");
            
            // Act
            var result = mockService.Object.GetValue();
            
            // Assert
            Assert.Equal("test", result);
        }
    }

    public interface ITestService
    {
        string GetValue();
    }
} 