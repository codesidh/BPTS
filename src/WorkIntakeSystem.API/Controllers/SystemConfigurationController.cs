using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Infrastructure.Authorization;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<SystemConfigurationController> _logger;

    public SystemConfigurationController(
        IConfigurationService configurationService,
        ILogger<SystemConfigurationController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all system configurations
    /// </summary>
    [HttpGet]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetAllConfigurations()
    {
        try
        {
            var configurations = await _configurationService.GetAllConfigurationsAsync();
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all configurations");
            return StatusCode(500, "An error occurred while retrieving configurations");
        }
    }

    /// <summary>
    /// Get configuration value by key
    /// </summary>
    [HttpGet("{key}")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetConfiguration(string key, [FromQuery] int? businessVerticalId = null, [FromQuery] int? version = null)
    {
        try
        {
            var value = await _configurationService.GetValueAsync(key, businessVerticalId, version);
            if (value == null)
                return NotFound($"Configuration with key '{key}' not found");

            return Ok(new { Key = key, Value = value, BusinessVerticalId = businessVerticalId, Version = version });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration {Key}", key);
            return StatusCode(500, "An error occurred while retrieving the configuration");
        }
    }

    /// <summary>
    /// Get typed configuration value
    /// </summary>
    [HttpGet("{key}/typed")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetTypedConfiguration<T>(string key, [FromQuery] int? businessVerticalId = null, [FromQuery] int? version = null)
    {
        try
        {
            var value = await _configurationService.GetValueAsync<T>(key, businessVerticalId, version);
            return Ok(new { Key = key, Value = value, Type = typeof(T).Name, BusinessVerticalId = businessVerticalId, Version = version });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving typed configuration {Key}", key);
            return StatusCode(500, "An error occurred while retrieving the configuration");
        }
    }

    // Configuration Versioning Endpoints

    /// <summary>
    /// Create new version of configuration
    /// </summary>
    [HttpPost("{key}/versions")]
    [RequirePermission("SystemConfiguration:Create")]
    public async Task<IActionResult> CreateNewVersion(string key, [FromBody] CreateVersionRequest request)
    {
        try
        {
            var newConfig = await _configurationService.CreateNewVersionAsync(
                key, 
                request.Value, 
                request.ChangeReason, 
                request.BusinessVerticalId, 
                User.Identity?.Name ?? "System");

            return Ok(newConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new version for configuration {Key}", key);
            return StatusCode(500, "An error occurred while creating the configuration version");
        }
    }

    /// <summary>
    /// Get version history for configuration
    /// </summary>
    [HttpGet("{key}/versions")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetVersionHistory(string key, [FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var versions = await _configurationService.GetVersionHistoryAsync(key, businessVerticalId);
            return Ok(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving version history for configuration {Key}", key);
            return StatusCode(500, "An error occurred while retrieving version history");
        }
    }

    /// <summary>
    /// Get specific version of configuration
    /// </summary>
    [HttpGet("{key}/versions/{version}")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetVersion(string key, int version, [FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var config = await _configurationService.GetVersionAsync(key, version, businessVerticalId);
            if (config == null)
                return NotFound($"Configuration version {version} for key '{key}' not found");

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving version {Version} for configuration {Key}", version, key);
            return StatusCode(500, "An error occurred while retrieving the configuration version");
        }
    }

    /// <summary>
    /// Rollback configuration to specific version
    /// </summary>
    [HttpPost("{key}/versions/{targetVersion}/rollback")]
    [RequirePermission("SystemConfiguration:Update")]
    public async Task<IActionResult> RollbackToVersion(string key, int targetVersion, [FromBody] RollbackRequest request)
    {
        try
        {
            var success = await _configurationService.RollbackToVersionAsync(
                key, 
                targetVersion, 
                request.RollbackReason, 
                request.BusinessVerticalId, 
                User.Identity?.Name ?? "System");

            if (!success)
                return BadRequest($"Failed to rollback configuration '{key}' to version {targetVersion}");

            return Ok(new { Message = $"Successfully rolled back configuration '{key}' to version {targetVersion}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back configuration {Key} to version {Version}", key, targetVersion);
            return StatusCode(500, "An error occurred while rolling back the configuration");
        }
    }

    // Configuration Comparison Endpoints

    /// <summary>
    /// Compare two versions of a configuration
    /// </summary>
    [HttpGet("{key}/compare/{version1}/{version2}")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> CompareVersions(string key, int version1, int version2, [FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var comparison = await _configurationService.CompareVersionsAsync(key, version1, version2, businessVerticalId);
            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing versions {Version1} and {Version2} for configuration {Key}", version1, version2, key);
            return StatusCode(500, "An error occurred while comparing configuration versions");
        }
    }

    /// <summary>
    /// Compare configurations between business verticals
    /// </summary>
    [HttpGet("compare")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> CompareConfigurations([FromQuery] int? businessVerticalId1 = null, [FromQuery] int? businessVerticalId2 = null)
    {
        try
        {
            var comparisons = await _configurationService.CompareConfigurationsAsync(businessVerticalId1, businessVerticalId2);
            return Ok(comparisons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing configurations between business verticals {BV1} and {BV2}", businessVerticalId1, businessVerticalId2);
            return StatusCode(500, "An error occurred while comparing configurations");
        }
    }

    // Business Vertical Specific Configuration Endpoints

    /// <summary>
    /// Create vertical-specific configuration
    /// </summary>
    [HttpPost("vertical/{businessVerticalId}/{key}")]
    [RequirePermission("SystemConfiguration:Create")]
    public async Task<IActionResult> CreateVerticalSpecificConfig(int businessVerticalId, string key, [FromBody] CreateConfigRequest request)
    {
        try
        {
            var config = await _configurationService.CreateVerticalSpecificConfigAsync(
                key, 
                request.Value, 
                businessVerticalId, 
                request.ChangeReason, 
                User.Identity?.Name ?? "System");

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vertical specific configuration {Key} for business vertical {BusinessVerticalId}", key, businessVerticalId);
            return StatusCode(500, "An error occurred while creating the vertical-specific configuration");
        }
    }

    /// <summary>
    /// Get effective configuration for business vertical
    /// </summary>
    [HttpGet("vertical/{businessVerticalId}/{key}/effective")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetEffectiveConfiguration(int businessVerticalId, string key)
    {
        try
        {
            var config = await _configurationService.GetEffectiveConfigurationAsync(key, businessVerticalId);
            if (config == null)
                return NotFound($"No effective configuration found for key '{key}' and business vertical {businessVerticalId}");

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving effective configuration {Key} for business vertical {BusinessVerticalId}", key, businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving the effective configuration");
        }
    }

    /// <summary>
    /// Get all configurations for business vertical
    /// </summary>
    [HttpGet("vertical/{businessVerticalId}")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetVerticalConfigurations(int businessVerticalId)
    {
        try
        {
            var configs = await _configurationService.GetVerticalConfigurationsAsync(businessVerticalId);
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configurations for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving vertical configurations");
        }
    }

    /// <summary>
    /// Get all global configurations
    /// </summary>
    [HttpGet("global")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetGlobalConfigurations()
    {
        try
        {
            var configs = await _configurationService.GetGlobalConfigurationsAsync();
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving global configurations");
            return StatusCode(500, "An error occurred while retrieving global configurations");
        }
    }

    /// <summary>
    /// Inherit configuration from global to business vertical
    /// </summary>
    [HttpPost("vertical/{businessVerticalId}/{key}/inherit")]
    [RequirePermission("SystemConfiguration:Create")]
    public async Task<IActionResult> InheritFromGlobal(int businessVerticalId, string key, [FromBody] InheritRequest request)
    {
        try
        {
            var success = await _configurationService.InheritFromGlobalAsync(
                key, 
                businessVerticalId, 
                request.ChangeReason, 
                User.Identity?.Name ?? "System");

            if (!success)
                return BadRequest($"Failed to inherit global configuration '{key}' for business vertical {businessVerticalId}");

            return Ok(new { Message = $"Successfully inherited global configuration '{key}' for business vertical {businessVerticalId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inheriting global configuration {Key} for business vertical {BusinessVerticalId}", key, businessVerticalId);
            return StatusCode(500, "An error occurred while inheriting the global configuration");
        }
    }

    /// <summary>
    /// Override global configuration for business vertical
    /// </summary>
    [HttpPost("vertical/{businessVerticalId}/{key}/override")]
    [RequirePermission("SystemConfiguration:Update")]
    public async Task<IActionResult> OverrideGlobalConfig(int businessVerticalId, string key, [FromBody] OverrideRequest request)
    {
        try
        {
            var success = await _configurationService.OverrideGlobalConfigAsync(
                key, 
                request.NewValue, 
                businessVerticalId, 
                request.ChangeReason, 
                User.Identity?.Name ?? "System");

            if (!success)
                return BadRequest($"Failed to override global configuration '{key}' for business vertical {businessVerticalId}");

            return Ok(new { Message = $"Successfully overridden global configuration '{key}' for business vertical {businessVerticalId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error overriding global configuration {Key} for business vertical {BusinessVerticalId}", key, businessVerticalId);
            return StatusCode(500, "An error occurred while overriding the global configuration");
        }
    }

    // Configuration Validation Endpoints

    /// <summary>
    /// Validate configuration value
    /// </summary>
    [HttpPost("{key}/validate")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> ValidateConfiguration(string key, [FromBody] ValidateRequest request)
    {
        try
        {
            var result = await _configurationService.ValidateConfigurationAsync(key, request.Value, request.DataType, request.BusinessVerticalId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration {Key}", key);
            return StatusCode(500, "An error occurred while validating the configuration");
        }
    }

    /// <summary>
    /// Validate all configurations
    /// </summary>
    [HttpPost("validate-all")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> ValidateAllConfigurations([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var results = await _configurationService.ValidateAllConfigurationsAsync(businessVerticalId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating all configurations for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while validating configurations");
        }
    }

    // Date-based Configuration Endpoints

    /// <summary>
    /// Get active configurations as of specific date
    /// </summary>
    [HttpGet("active")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetActiveConfigurations([FromQuery] DateTime? asOfDate = null, [FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var configs = await _configurationService.GetActiveConfigurationsAsync(asOfDate, businessVerticalId);
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active configurations");
            return StatusCode(500, "An error occurred while retrieving active configurations");
        }
    }

    /// <summary>
    /// Get expired configurations
    /// </summary>
    [HttpGet("expired")]
    [RequirePermission("SystemConfiguration:Read")]
    public async Task<IActionResult> GetExpiredConfigurations([FromQuery] int? businessVerticalId = null)
    {
        try
        {
            var configs = await _configurationService.GetExpiredConfigurationsAsync(businessVerticalId);
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired configurations");
            return StatusCode(500, "An error occurred while retrieving expired configurations");
        }
    }

    /// <summary>
    /// Set expiration date for configuration
    /// </summary>
    [HttpPut("{key}/expiration")]
    [RequirePermission("SystemConfiguration:Update")]
    public async Task<IActionResult> SetExpirationDate(string key, [FromBody] SetExpirationRequest request)
    {
        try
        {
            var success = await _configurationService.SetExpirationDateAsync(
                key, 
                request.ExpirationDate, 
                request.BusinessVerticalId, 
                User.Identity?.Name ?? "System");

            if (!success)
                return BadRequest($"Failed to set expiration date for configuration '{key}'");

            return Ok(new { Message = $"Successfully set expiration date for configuration '{key}'" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting expiration date for configuration {Key}", key);
            return StatusCode(500, "An error occurred while setting the expiration date");
        }
    }
}

// Request DTOs
public record CreateVersionRequest(string Value, string ChangeReason, int? BusinessVerticalId = null);
public record RollbackRequest(string RollbackReason, int? BusinessVerticalId = null);
public record CreateConfigRequest(string Value, string ChangeReason);
public record InheritRequest(string ChangeReason);
public record OverrideRequest(string NewValue, string ChangeReason);
public record ValidateRequest(string Value, string DataType, int? BusinessVerticalId = null);
public record SetExpirationRequest(DateTime ExpirationDate, int? BusinessVerticalId = null); 