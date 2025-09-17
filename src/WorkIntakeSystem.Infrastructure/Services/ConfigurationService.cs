using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ISystemConfigurationRepository _configRepo;
        private readonly IConfiguration _appSettings;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(
            ISystemConfigurationRepository configRepo, 
            IConfiguration appSettings,
            ILogger<ConfigurationService> logger)
        {
            _configRepo = configRepo;
            _appSettings = appSettings;
            _logger = logger;
        }

        // Existing methods
        public async Task<string?> GetValueAsync(string key, int? businessVerticalId = null, int? version = null)
        {
            try
            {
                if (version.HasValue)
                {
                    var config = await _configRepo.GetByKeyVersionAsync(key, businessVerticalId, version.Value);
                    if (config != null && config.IsActive)
                        return config.ConfigurationValue;
                }
                else
                {
                    var config = await _configRepo.GetLatestActiveAsync(key, businessVerticalId);
                    if (config != null && config.IsActive)
                        return config.ConfigurationValue;
                }
                
                // Fallback to appsettings
                return _appSettings[key] ?? null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration value for key {Key}", key);
                return _appSettings[key] ?? null;
            }
        }

        public async Task<T?> GetValueAsync<T>(string key, int? businessVerticalId = null, int? version = null)
        {
            var value = await GetValueAsync(key, businessVerticalId, version);
            if (value == null)
                return default;
            
            try
            {
                // Try to parse as JSON first
                return JsonSerializer.Deserialize<T>(value);
            }
            catch
            {
                try
                {
                    // Fallback: try to convert directly
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return default;
                }
            }
        }

        public async Task<IEnumerable<SystemConfiguration>> GetAllConfigurationsAsync()
        {
            try
            {
                return await _configRepo.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all configurations");
                return Enumerable.Empty<SystemConfiguration>();
            }
        }

        // Configuration Versioning
        public async Task<SystemConfiguration> CreateNewVersionAsync(string key, string newValue, string changeReason, int? businessVerticalId = null, string approvedBy = "System")
        {
            try
            {
                var currentConfig = await _configRepo.GetLatestActiveAsync(key, businessVerticalId);
                var nextVersion = await _configRepo.GetNextVersionNumberAsync(key, businessVerticalId);

                var newConfig = new SystemConfiguration
                {
                    ConfigurationKey = key,
                    ConfigurationValue = newValue,
                    BusinessVerticalId = businessVerticalId,
                    Version = nextVersion,
                    ChangeReason = changeReason,
                    ApprovedBy = approvedBy,
                    ApprovalDate = DateTime.UtcNow,
                    EffectiveDate = DateTime.UtcNow,
                    PreviousVersionId = currentConfig?.Id,
                    DataType = currentConfig?.DataType ?? "String",
                    Description = currentConfig?.Description ?? string.Empty,
                    IsEditable = currentConfig?.IsEditable ?? true
                };

                var createdConfig = await _configRepo.CreateAsync(newConfig);
                _logger.LogInformation("Created new configuration version {Version} for key {Key}", nextVersion, key);
                return createdConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version for configuration key {Key}", key);
                throw;
            }
        }

        public async Task<IEnumerable<SystemConfiguration>> GetVersionHistoryAsync(string key, int? businessVerticalId = null)
        {
            try
            {
                return await _configRepo.GetVersionHistoryAsync(key, businessVerticalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for key {Key}", key);
                return Enumerable.Empty<SystemConfiguration>();
            }
        }

        public async Task<SystemConfiguration?> GetVersionAsync(string key, int version, int? businessVerticalId = null)
        {
            try
            {
                return await _configRepo.GetByKeyVersionAsync(key, businessVerticalId, version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version {Version} for key {Key}", version, key);
                return null;
            }
        }

        public async Task<bool> RollbackToVersionAsync(string key, int targetVersion, string rollbackReason, int? businessVerticalId = null, string approvedBy = "System")
        {
            try
            {
                var targetConfig = await _configRepo.GetByKeyVersionAsync(key, businessVerticalId, targetVersion);
                if (targetConfig == null)
                    return false;

                await CreateNewVersionAsync(key, targetConfig.ConfigurationValue, $"Rollback to version {targetVersion}: {rollbackReason}", businessVerticalId, approvedBy);
                _logger.LogInformation("Rolled back configuration {Key} to version {Version}", key, targetVersion);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back configuration {Key} to version {Version}", key, targetVersion);
                return false;
            }
        }

        // Configuration Comparison
        public async Task<ConfigurationComparison> CompareVersionsAsync(string key, int version1, int version2, int? businessVerticalId = null)
        {
            try
            {
                var config1 = await _configRepo.GetByKeyVersionAsync(key, businessVerticalId, version1);
                var config2 = await _configRepo.GetByKeyVersionAsync(key, businessVerticalId, version2);

                var comparison = new ConfigurationComparison
                {
                    ConfigurationKey = key,
                    Version1 = config1,
                    Version2 = config2,
                    HasDifferences = config1?.ConfigurationValue != config2?.ConfigurationValue
                };

                if (comparison.HasDifferences)
                {
                    comparison.ComparisonResult = $"Value changed from '{config1?.ConfigurationValue}' to '{config2?.ConfigurationValue}'";
                }
                else
                {
                    comparison.ComparisonResult = "No differences found";
                }

                return comparison;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing versions {Version1} and {Version2} for key {Key}", version1, version2, key);
                return new ConfigurationComparison { ConfigurationKey = key, ComparisonResult = "Error during comparison" };
            }
        }

        public async Task<IEnumerable<ConfigurationComparison>> CompareConfigurationsAsync(int? businessVerticalId1, int? businessVerticalId2)
        {
            try
            {
                var configs1 = await _configRepo.GetActiveConfigurationsAsync(null, businessVerticalId1);
                var configs2 = await _configRepo.GetActiveConfigurationsAsync(null, businessVerticalId2);

                var comparisons = new List<ConfigurationComparison>();
                var allKeys = configs1.Select(c => c.ConfigurationKey).Union(configs2.Select(c => c.ConfigurationKey)).Distinct();

                foreach (var key in allKeys)
                {
                    var config1 = configs1.FirstOrDefault(c => c.ConfigurationKey == key);
                    var config2 = configs2.FirstOrDefault(c => c.ConfigurationKey == key);

                    var comparison = new ConfigurationComparison
                    {
                        ConfigurationKey = key,
                        Version1 = config1,
                        Version2 = config2,
                        HasDifferences = config1?.ConfigurationValue != config2?.ConfigurationValue
                    };

                    if (config1 == null)
                        comparison.ComparisonResult = $"Configuration only exists in second context with value '{config2?.ConfigurationValue}'";
                    else if (config2 == null)
                        comparison.ComparisonResult = $"Configuration only exists in first context with value '{config1?.ConfigurationValue}'";
                    else if (comparison.HasDifferences)
                        comparison.ComparisonResult = $"Value differs: '{config1?.ConfigurationValue}' vs '{config2?.ConfigurationValue}'";
                    else
                        comparison.ComparisonResult = "Values are identical";

                    comparisons.Add(comparison);
                }

                return comparisons;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing configurations between business verticals {BV1} and {BV2}", businessVerticalId1, businessVerticalId2);
                return Enumerable.Empty<ConfigurationComparison>();
            }
        }

        // Business Vertical Specific Configuration
        public async Task<SystemConfiguration> CreateVerticalSpecificConfigAsync(string key, string value, int businessVerticalId, string changeReason, string approvedBy = "System")
        {
            try
            {
                return await CreateNewVersionAsync(key, value, changeReason, businessVerticalId, approvedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vertical specific configuration for key {Key} and business vertical {BusinessVerticalId}", key, businessVerticalId);
                throw;
            }
        }

        public async Task<SystemConfiguration?> GetEffectiveConfigurationAsync(string key, int businessVerticalId)
        {
            try
            {
                return await _configRepo.GetEffectiveConfigurationAsync(key, businessVerticalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting effective configuration for key {Key} and business vertical {BusinessVerticalId}", key, businessVerticalId);
                return null;
            }
        }

        public async Task<IEnumerable<SystemConfiguration>> GetVerticalConfigurationsAsync(int businessVerticalId)
        {
            try
            {
                return await _configRepo.GetVerticalConfigurationsAsync(businessVerticalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configurations for business vertical {BusinessVerticalId}", businessVerticalId);
                return Enumerable.Empty<SystemConfiguration>();
            }
        }

        public async Task<IEnumerable<SystemConfiguration>> GetGlobalConfigurationsAsync()
        {
            try
            {
                return await _configRepo.GetGlobalConfigurationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting global configurations");
                return Enumerable.Empty<SystemConfiguration>();
            }
        }

        public async Task<bool> InheritFromGlobalAsync(string key, int businessVerticalId, string changeReason, string approvedBy = "System")
        {
            try
            {
                var globalConfig = await _configRepo.GetGlobalConfigurationAsync(key);
                if (globalConfig == null)
                    return false;

                await CreateVerticalSpecificConfigAsync(key, globalConfig.ConfigurationValue, businessVerticalId, $"Inherited from global: {changeReason}", approvedBy);
                _logger.LogInformation("Configuration {Key} inherited from global for business vertical {BusinessVerticalId}", key, businessVerticalId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inheriting global configuration {Key} for business vertical {BusinessVerticalId}", key, businessVerticalId);
                return false;
            }
        }

        public async Task<bool> OverrideGlobalConfigAsync(string key, string newValue, int businessVerticalId, string changeReason, string approvedBy = "System")
        {
            try
            {
                await CreateVerticalSpecificConfigAsync(key, newValue, businessVerticalId, $"Override global: {changeReason}", approvedBy);
                _logger.LogInformation("Configuration {Key} overridden for business vertical {BusinessVerticalId}", key, businessVerticalId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error overriding global configuration {Key} for business vertical {BusinessVerticalId}", key, businessVerticalId);
                return false;
            }
        }

        // Configuration Validation
        public async Task<ConfigurationValidationResult> ValidateConfigurationAsync(string key, string value, string dataType, int? businessVerticalId = null)
        {
            var result = new ConfigurationValidationResult
            {
                ConfigurationKey = key,
                IsValid = true,
                ValidationMessage = "Configuration is valid"
            };

            try
            {
                switch (dataType.ToLower())
                {
                    case "int":
                    case "integer":
                        if (!int.TryParse(value, out _))
                        {
                            result.IsValid = false;
                            result.ValidationErrors.Add($"Value '{value}' is not a valid integer");
                        }
                        break;

                    case "decimal":
                    case "double":
                        if (!decimal.TryParse(value, out _))
                        {
                            result.IsValid = false;
                            result.ValidationErrors.Add($"Value '{value}' is not a valid decimal");
                        }
                        break;

                    case "bool":
                    case "boolean":
                        if (!bool.TryParse(value, out _))
                        {
                            result.IsValid = false;
                            result.ValidationErrors.Add($"Value '{value}' is not a valid boolean");
                        }
                        break;

                    case "datetime":
                        if (!DateTime.TryParse(value, out _))
                        {
                            result.IsValid = false;
                            result.ValidationErrors.Add($"Value '{value}' is not a valid datetime");
                        }
                        break;

                    case "json":
                        try
                        {
                            JsonDocument.Parse(value);
                        }
                        catch
                        {
                            result.IsValid = false;
                            result.ValidationErrors.Add($"Value '{value}' is not valid JSON");
                        }
                        break;

                    case "string":
                    default:
                        // String validation - check for reasonable length
                        if (value.Length > 10000)
                        {
                            result.IsValid = false;
                            result.ValidationErrors.Add("Value exceeds maximum length of 10,000 characters");
                        }
                        break;
                }

                if (!result.IsValid)
                {
                    result.ValidationMessage = $"Configuration validation failed: {string.Join(", ", result.ValidationErrors)}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration {Key}", key);
                result.IsValid = false;
                result.ValidationMessage = "Error during validation";
                result.ValidationErrors.Add(ex.Message);
            }

            return result;
        }

        public async Task<IEnumerable<ConfigurationValidationResult>> ValidateAllConfigurationsAsync(int? businessVerticalId = null)
        {
            try
            {
                var configurations = await _configRepo.GetActiveConfigurationsAsync(null, businessVerticalId);
                var results = new List<ConfigurationValidationResult>();

                foreach (var config in configurations)
                {
                    var result = await ValidateConfigurationAsync(config.ConfigurationKey, config.ConfigurationValue, config.DataType, businessVerticalId);
                    results.Add(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating all configurations for business vertical {BusinessVerticalId}", businessVerticalId);
                return Enumerable.Empty<ConfigurationValidationResult>();
            }
        }

        // Effective/Expiration Date Handling
        public async Task<IEnumerable<SystemConfiguration>> GetActiveConfigurationsAsync(DateTime? asOfDate = null, int? businessVerticalId = null)
        {
            try
            {
                return await _configRepo.GetActiveConfigurationsAsync(asOfDate, businessVerticalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active configurations");
                return Enumerable.Empty<SystemConfiguration>();
            }
        }

        public async Task<IEnumerable<SystemConfiguration>> GetExpiredConfigurationsAsync(int? businessVerticalId = null)
        {
            try
            {
                return await _configRepo.GetExpiredConfigurationsAsync(businessVerticalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired configurations");
                return Enumerable.Empty<SystemConfiguration>();
            }
        }

        public async Task<bool> SetExpirationDateAsync(string key, DateTime expirationDate, int? businessVerticalId = null, string approvedBy = "System")
        {
            try
            {
                var config = await _configRepo.GetLatestActiveAsync(key, businessVerticalId);
                if (config == null)
                    return false;

                config.ExpirationDate = expirationDate;
                config.ApprovedBy = approvedBy;
                config.ApprovalDate = DateTime.UtcNow;

                await _configRepo.UpdateAsync(config);
                _logger.LogInformation("Set expiration date {ExpirationDate} for configuration {Key}", expirationDate, key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting expiration date for configuration {Key}", key);
                return false;
            }
        }
    }
} 