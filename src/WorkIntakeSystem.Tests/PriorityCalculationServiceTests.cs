using System.Threading.Tasks;
using Moq;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Services;
using Xunit;

namespace WorkIntakeSystem.Tests
{
    public class PriorityCalculationServiceTests
    {
        [Fact]
        public async Task UsesConfigurableTimeDecay()
        {
            var workRequest = new WorkRequest { Id = 1, CreatedDate = System.DateTime.UtcNow.AddDays(-10) };
            var config = new Mock<IConfigurationService>();
            config.Setup(c => c.GetValueAsync<bool>("PriorityCalculation:TimeDecayEnabled", null, null)).ReturnsAsync(true);
            config.Setup(c => c.GetValueAsync<decimal>("PriorityCalculation:MaxTimeDecayMultiplier", null, null)).ReturnsAsync(1.5m);
            var svc = new PriorityCalculationService(null, null, null, config.Object);
            var factor = await svc.CalculateTimeDecayFactorAsync(workRequest.CreatedDate);
            Assert.True(factor <= 1.5m);
        }
    }
} 