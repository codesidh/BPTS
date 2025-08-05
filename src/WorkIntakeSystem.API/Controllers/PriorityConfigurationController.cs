using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Authorization;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PriorityConfigurationController : ControllerBase
{
    private readonly IPriorityConfigurationService _priorityConfigurationService;
    private readonly ILogger<PriorityConfigurationController> _logger;

    public PriorityConfigurationController(
        IPriorityConfigurationService priorityConfigurationService,
        ILogger<PriorityConfigurationController> logger)
    {
        _priorityConfigurationService = priorityConfigurationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all priority configurations
    /// </summary>
    [HttpGet]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetAllConfigurations()
    {
        try
        {
            var configurations = await _priorityConfigurationService.GetAllConfigurationsAsync();
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all priority configurations");
            return StatusCode(500, "An error occurred while retrieving priority configurations");
        }
    }

    /// <summary>
    /// Get priority configurations by business vertical
    /// </summary>
    [HttpGet("business-vertical/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetConfigurationsByBusinessVertical(int businessVerticalId)
    {
        try
        {
            var configurations = await _priorityConfigurationService.GetConfigurationsByBusinessVerticalAsync(businessVerticalId);
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority configurations for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving priority configurations");
        }
    }

    /// <summary>
    /// Get priority configuration by ID
    /// </summary>
    [HttpGet("{configurationId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetConfiguration(int configurationId)
    {
        try
        {
            var configuration = await _priorityConfigurationService.GetConfigurationAsync(configurationId);
            if (configuration == null)
                return NotFound($"Priority configuration {configurationId} not found");

            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority configuration {ConfigurationId}", configurationId);
            return StatusCode(500, "An error occurred while retrieving priority configuration");
        }
    }

    /// <summary>
    /// Create new priority configuration
    /// </summary>
    [HttpPost]
    [RequirePermission("PriorityConfiguration:Create")]
    public async Task<IActionResult> CreateConfiguration([FromBody] PriorityConfiguration configuration)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = await _priorityConfigurationService.ValidateConfigurationAsync(configuration);
            if (!validationResult.IsValid)
                return BadRequest(new { Errors = validationResult.Errors, Warnings = validationResult.Warnings });

            var createdConfiguration = await _priorityConfigurationService.CreateConfigurationAsync(configuration);
            return CreatedAtAction(nameof(GetConfiguration), new { configurationId = createdConfiguration.Id }, createdConfiguration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating priority configuration");
            return StatusCode(500, "An error occurred while creating priority configuration");
        }
    }

    /// <summary>
    /// Update priority configuration
    /// </summary>
    [HttpPut("{configurationId}")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> UpdateConfiguration(int configurationId, [FromBody] PriorityConfiguration configuration)
    {
        try
        {
            if (configurationId != configuration.Id)
                return BadRequest("Configuration ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = await _priorityConfigurationService.ValidateConfigurationAsync(configuration);
            if (!validationResult.IsValid)
                return BadRequest(new { Errors = validationResult.Errors, Warnings = validationResult.Warnings });

            var updatedConfiguration = await _priorityConfigurationService.UpdateConfigurationAsync(configuration);
            return Ok(updatedConfiguration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating priority configuration {ConfigurationId}", configurationId);
            return StatusCode(500, "An error occurred while updating priority configuration");
        }
    }

    /// <summary>
    /// Delete priority configuration
    /// </summary>
    [HttpDelete("{configurationId}")]
    [RequirePermission("PriorityConfiguration:Delete")]
    public async Task<IActionResult> DeleteConfiguration(int configurationId)
    {
        try
        {
            var success = await _priorityConfigurationService.DeleteConfigurationAsync(configurationId);
            if (!success)
                return NotFound($"Priority configuration {configurationId} not found");

            return Ok(new { Message = "Priority configuration deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting priority configuration {ConfigurationId}", configurationId);
            return StatusCode(500, "An error occurred while deleting priority configuration");
        }
    }

    /// <summary>
    /// Get priority algorithm configuration
    /// </summary>
    [HttpGet("algorithm/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetAlgorithmConfig(int businessVerticalId)
    {
        try
        {
            var config = await _priorityConfigurationService.GetAlgorithmConfigAsync(businessVerticalId);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting algorithm config for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving algorithm configuration");
        }
    }

    /// <summary>
    /// Set priority algorithm configuration
    /// </summary>
    [HttpPost("algorithm/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> SetAlgorithmConfig(int businessVerticalId, [FromBody] PriorityAlgorithmConfig config)
    {
        try
        {
            var success = await _priorityConfigurationService.SetAlgorithmConfigAsync(businessVerticalId, config);
            if (!success)
                return BadRequest("Failed to set algorithm configuration");

            return Ok(new { Message = "Algorithm configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting algorithm config for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while setting algorithm configuration");
        }
    }

    /// <summary>
    /// Test priority calculation with given parameters
    /// </summary>
    [HttpPost("test-calculation/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> TestPriorityCalculation(int businessVerticalId, [FromBody] WorkRequest testRequest)
    {
        try
        {
            var result = await _priorityConfigurationService.TestPriorityCalculationAsync(businessVerticalId, testRequest);
            return Ok(new { PriorityScore = result, TestRequest = testRequest });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing priority calculation for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while testing priority calculation");
        }
    }

    /// <summary>
    /// Preview priority changes with new configuration
    /// </summary>
    [HttpPost("preview/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> PreviewPriorityChanges(int businessVerticalId, [FromBody] PriorityAlgorithmConfig newConfig)
    {
        try
        {
            var result = await _priorityConfigurationService.PreviewPriorityChangesAsync(businessVerticalId, newConfig);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing priority changes for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while previewing priority changes");
        }
    }

    /// <summary>
    /// Get time decay configuration
    /// </summary>
    [HttpGet("time-decay/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetTimeDecayConfig(int businessVerticalId)
    {
        try
        {
            var config = await _priorityConfigurationService.GetTimeDecayConfigAsync(businessVerticalId);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time decay config for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving time decay configuration");
        }
    }

    /// <summary>
    /// Set time decay configuration
    /// </summary>
    [HttpPost("time-decay/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> SetTimeDecayConfig(int businessVerticalId, [FromBody] TimeDecayConfig config)
    {
        try
        {
            var success = await _priorityConfigurationService.SetTimeDecayConfigAsync(businessVerticalId, config);
            if (!success)
                return BadRequest("Failed to set time decay configuration");

            return Ok(new { Message = "Time decay configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting time decay config for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while setting time decay configuration");
        }
    }

    /// <summary>
    /// Calculate time decay factor for a specific date
    /// </summary>
    [HttpPost("time-decay/{businessVerticalId}/calculate")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> CalculateTimeDecayFactor(int businessVerticalId, [FromBody] DateTime createdDate)
    {
        try
        {
            var config = await _priorityConfigurationService.GetTimeDecayConfigAsync(businessVerticalId);
            var factor = await _priorityConfigurationService.CalculateTimeDecayFactorAsync(createdDate, config);
            
            return Ok(new { 
                CreatedDate = createdDate,
                DaysOld = (DateTime.UtcNow - createdDate).Days,
                TimeDecayFactor = factor,
                Configuration = config
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating time decay factor for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while calculating time decay factor");
        }
    }

    /// <summary>
    /// Get business value weight configuration
    /// </summary>
    [HttpGet("business-value-weights/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetBusinessValueWeights(int businessVerticalId)
    {
        try
        {
            var config = await _priorityConfigurationService.GetBusinessValueWeightsAsync(businessVerticalId);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business value weights for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving business value weights");
        }
    }

    /// <summary>
    /// Set business value weight configuration
    /// </summary>
    [HttpPost("business-value-weights/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> SetBusinessValueWeights(int businessVerticalId, [FromBody] BusinessValueWeightConfig config)
    {
        try
        {
            var success = await _priorityConfigurationService.SetBusinessValueWeightsAsync(businessVerticalId, config);
            if (!success)
                return BadRequest("Failed to set business value weights configuration");

            return Ok(new { Message = "Business value weights updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting business value weights for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while setting business value weights");
        }
    }

    /// <summary>
    /// Get capacity adjustment configuration
    /// </summary>
    [HttpGet("capacity-adjustment/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetCapacityAdjustmentConfig(int businessVerticalId)
    {
        try
        {
            var config = await _priorityConfigurationService.GetCapacityAdjustmentConfigAsync(businessVerticalId);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting capacity adjustment config for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving capacity adjustment configuration");
        }
    }

    /// <summary>
    /// Set capacity adjustment configuration
    /// </summary>
    [HttpPost("capacity-adjustment/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> SetCapacityAdjustmentConfig(int businessVerticalId, [FromBody] CapacityAdjustmentConfig config)
    {
        try
        {
            var success = await _priorityConfigurationService.SetCapacityAdjustmentConfigAsync(businessVerticalId, config);
            if (!success)
                return BadRequest("Failed to set capacity adjustment configuration");

            return Ok(new { Message = "Capacity adjustment configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting capacity adjustment config for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while setting capacity adjustment configuration");
        }
    }

    /// <summary>
    /// Get auto-adjustment rules configuration
    /// </summary>
    [HttpGet("auto-adjustment/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetAutoAdjustmentRules(int businessVerticalId)
    {
        try
        {
            var config = await _priorityConfigurationService.GetAutoAdjustmentRulesAsync(businessVerticalId);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auto adjustment rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving auto adjustment rules");
        }
    }

    /// <summary>
    /// Set auto-adjustment rules configuration
    /// </summary>
    [HttpPost("auto-adjustment/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> SetAutoAdjustmentRules(int businessVerticalId, [FromBody] AutoAdjustmentRulesConfig config)
    {
        try
        {
            var success = await _priorityConfigurationService.SetAutoAdjustmentRulesAsync(businessVerticalId, config);
            if (!success)
                return BadRequest("Failed to set auto adjustment rules");

            return Ok(new { Message = "Auto adjustment rules updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting auto adjustment rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while setting auto adjustment rules");
        }
    }

    /// <summary>
    /// Process auto-adjustments
    /// </summary>
    [HttpPost("auto-adjustment/process")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> ProcessAutoAdjustments([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var success = await _priorityConfigurationService.ProcessAutoAdjustmentsAsync(businessVerticalId);
            if (!success)
                return BadRequest("Failed to process auto adjustments");

            return Ok(new { Message = "Auto adjustments processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto adjustments");
            return StatusCode(500, "An error occurred while processing auto adjustments");
        }
    }

    /// <summary>
    /// Get escalation rules configuration
    /// </summary>
    [HttpGet("escalation/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetEscalationRules(int businessVerticalId)
    {
        try
        {
            var config = await _priorityConfigurationService.GetEscalationRulesAsync(businessVerticalId);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving escalation rules");
        }
    }

    /// <summary>
    /// Set escalation rules configuration
    /// </summary>
    [HttpPost("escalation/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> SetEscalationRules(int businessVerticalId, [FromBody] EscalationRulesConfig config)
    {
        try
        {
            var success = await _priorityConfigurationService.SetEscalationRulesAsync(businessVerticalId, config);
            if (!success)
                return BadRequest("Failed to set escalation rules");

            return Ok(new { Message = "Escalation rules updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting escalation rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while setting escalation rules");
        }
    }

    /// <summary>
    /// Get pending escalations
    /// </summary>
    [HttpGet("escalation/pending")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetPendingEscalations([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var escalations = await _priorityConfigurationService.GetPendingEscalationsAsync(businessVerticalId);
            return Ok(escalations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending escalations");
            return StatusCode(500, "An error occurred while retrieving pending escalations");
        }
    }

    /// <summary>
    /// Process escalations
    /// </summary>
    [HttpPost("escalation/process")]
    [RequirePermission("PriorityConfiguration:Update")]
    public async Task<IActionResult> ProcessEscalations([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var success = await _priorityConfigurationService.ProcessEscalationsAsync(businessVerticalId);
            if (!success)
                return BadRequest("Failed to process escalations");

            return Ok(new { Message = "Escalations processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing escalations");
            return StatusCode(500, "An error occurred while processing escalations");
        }
    }

    /// <summary>
    /// Validate priority configuration
    /// </summary>
    [HttpPost("validate")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> ValidateConfiguration([FromBody] PriorityConfiguration configuration)
    {
        try
        {
            var result = await _priorityConfigurationService.ValidateConfigurationAsync(configuration);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating priority configuration");
            return StatusCode(500, "An error occurred while validating configuration");
        }
    }

    /// <summary>
    /// Get priority trends analysis
    /// </summary>
    [HttpGet("analytics/trends/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetPriorityTrends(int businessVerticalId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var analysis = await _priorityConfigurationService.GetPriorityTrendsAsync(businessVerticalId, fromDate, toDate);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority trends for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving priority trends");
        }
    }

    /// <summary>
    /// Get effectiveness metrics
    /// </summary>
    [HttpGet("analytics/effectiveness/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetEffectivenessMetrics(int businessVerticalId)
    {
        try
        {
            var metrics = await _priorityConfigurationService.GetEffectivenessMetricsAsync(businessVerticalId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effectiveness metrics for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving effectiveness metrics");
        }
    }

    /// <summary>
    /// Get configuration recommendations
    /// </summary>
    [HttpGet("analytics/recommendations/{businessVerticalId}")]
    [RequirePermission("PriorityConfiguration:Read")]
    public async Task<IActionResult> GetConfigurationRecommendations(int businessVerticalId)
    {
        try
        {
            var recommendation = await _priorityConfigurationService.GetConfigurationRecommendationsAsync(businessVerticalId);
            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration recommendations for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving configuration recommendations");
        }
    }

    // Helper method to get current user ID
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
    }

    // Helper method to get current user name
    private string GetCurrentUserName()
    {
        return User.FindFirst("name")?.Value ?? "Unknown";
    }
} 