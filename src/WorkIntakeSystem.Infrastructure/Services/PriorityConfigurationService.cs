using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Services;

public class PriorityConfigurationService : IPriorityConfigurationService
{
    private readonly WorkIntakeDbContext _context;
    private readonly IConfigurationService _configurationService;
    private readonly IPriorityCalculationService _priorityCalculationService;
    private readonly ILogger<PriorityConfigurationService> _logger;

    public PriorityConfigurationService(
        WorkIntakeDbContext context,
        IConfigurationService configurationService,
        IPriorityCalculationService priorityCalculationService,
        ILogger<PriorityConfigurationService> logger)
    {
        _context = context;
        _configurationService = configurationService;
        _priorityCalculationService = priorityCalculationService;
        _logger = logger;
    }

    // CRUD Operations
    public async Task<PriorityConfiguration> CreateConfigurationAsync(PriorityConfiguration configuration)
    {
        try
        {
            configuration.CreatedDate = DateTime.UtcNow;
            configuration.ModifiedDate = DateTime.UtcNow;

            _context.PriorityConfigurations.Add(configuration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created priority configuration {ConfigurationId} for business vertical {BusinessVerticalId}",
                configuration.Id, configuration.BusinessVerticalId);

            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating priority configuration for business vertical {BusinessVerticalId}",
                configuration.BusinessVerticalId);
            throw;
        }
    }

    public async Task<PriorityConfiguration?> GetConfigurationAsync(int configurationId)
    {
        return await _context.PriorityConfigurations
            .Include(pc => pc.BusinessVertical)
            .FirstOrDefaultAsync(pc => pc.Id == configurationId);
    }

    public async Task<PriorityConfiguration?> GetConfigurationByBusinessVerticalAsync(int businessVerticalId, string priorityName)
    {
        return await _context.PriorityConfigurations
            .Include(pc => pc.BusinessVertical)
            .FirstOrDefaultAsync(pc => pc.BusinessVerticalId == businessVerticalId && pc.PriorityName == priorityName);
    }

    public async Task<IEnumerable<PriorityConfiguration>> GetAllConfigurationsAsync()
    {
        return await _context.PriorityConfigurations
            .Include(pc => pc.BusinessVertical)
            .Where(pc => pc.IsActive)
            .OrderBy(pc => pc.BusinessVerticalId)
            .ThenBy(pc => pc.PriorityName)
            .ToListAsync();
    }

    public async Task<IEnumerable<PriorityConfiguration>> GetConfigurationsByBusinessVerticalAsync(int businessVerticalId)
    {
        return await _context.PriorityConfigurations
            .Include(pc => pc.BusinessVertical)
            .Where(pc => pc.BusinessVerticalId == businessVerticalId && pc.IsActive)
            .OrderBy(pc => pc.PriorityName)
            .ToListAsync();
    }

    public async Task<PriorityConfiguration> UpdateConfigurationAsync(PriorityConfiguration configuration)
    {
        try
        {
            var existing = await _context.PriorityConfigurations.FindAsync(configuration.Id);
            if (existing == null)
                throw new InvalidOperationException($"Priority configuration {configuration.Id} not found");

            // Update properties
            existing.PriorityName = configuration.PriorityName;
            existing.MinScore = configuration.MinScore;
            existing.MaxScore = configuration.MaxScore;
            existing.ColorCode = configuration.ColorCode;
            existing.IconClass = configuration.IconClass;
            existing.Description = configuration.Description;
            existing.IsActive = configuration.IsActive;
            existing.EscalationRules = configuration.EscalationRules;
            existing.TimeDecayConfiguration = configuration.TimeDecayConfiguration;
            existing.BusinessValueWeights = configuration.BusinessValueWeights;
            existing.CapacityFactors = configuration.CapacityFactors;
            existing.AutoAdjustmentRules = configuration.AutoAdjustmentRules;
            existing.SLAHours = configuration.SLAHours;
            existing.EscalationThresholdHours = configuration.EscalationThresholdHours;
            existing.NotificationSettings = configuration.NotificationSettings;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = configuration.ModifiedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated priority configuration {ConfigurationId}", configuration.Id);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating priority configuration {ConfigurationId}", configuration.Id);
            throw;
        }
    }

    public async Task<bool> DeleteConfigurationAsync(int configurationId)
    {
        try
        {
            var configuration = await _context.PriorityConfigurations.FindAsync(configurationId);
            if (configuration == null) return false;

            // Soft delete
            configuration.IsActive = false;
            configuration.ModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted priority configuration {ConfigurationId}", configurationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting priority configuration {ConfigurationId}", configurationId);
            return false;
        }
    }

    // Algorithm Management
    public async Task<PriorityAlgorithmConfig> GetAlgorithmConfigAsync(int businessVerticalId)
    {
        try
        {
            var configJson = await _configurationService.GetValueAsync($"PriorityAlgorithm:BusinessVertical_{businessVerticalId}");
            
            if (string.IsNullOrEmpty(configJson))
            {
                return new PriorityAlgorithmConfig();
            }

            return JsonSerializer.Deserialize<PriorityAlgorithmConfig>(configJson) ?? new PriorityAlgorithmConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting algorithm config for business vertical {BusinessVerticalId}", businessVerticalId);
            return new PriorityAlgorithmConfig();
        }
    }

    public async Task<bool> SetAlgorithmConfigAsync(int businessVerticalId, PriorityAlgorithmConfig config)
    {
        try
        {
            config.LastModified = DateTime.UtcNow;
            var configJson = JsonSerializer.Serialize(config);
            
            var systemConfig = new SystemConfiguration
            {
                ConfigurationKey = $"PriorityAlgorithm:BusinessVertical_{businessVerticalId}",
                ConfigurationValue = configJson,
                DataType = "JSON",
                BusinessVerticalId = businessVerticalId,
                Description = "Priority algorithm configuration",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                CreatedBy = config.ModifiedBy,
                ModifiedBy = config.ModifiedBy
            };

            _context.SystemConfigurations.Add(systemConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set algorithm config for business vertical {BusinessVerticalId}", businessVerticalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting algorithm config for business vertical {BusinessVerticalId}", businessVerticalId);
            return false;
        }
    }

    public async Task<decimal> TestPriorityCalculationAsync(int businessVerticalId, WorkRequest testRequest)
    {
        try
        {
            // Use existing priority calculation service with test data
            var priorityScore = await _priorityCalculationService.CalculatePriorityScoreAsync(testRequest);
            
            _logger.LogInformation("Tested priority calculation for business vertical {BusinessVerticalId}: {Score}",
                businessVerticalId, priorityScore);
                
            return priorityScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing priority calculation for business vertical {BusinessVerticalId}", businessVerticalId);
            return 0.0m;
        }
    }

    public async Task<PriorityPreviewResult> PreviewPriorityChangesAsync(int businessVerticalId, PriorityAlgorithmConfig newConfig)
    {
        try
        {
            var result = new PriorityPreviewResult();
            
            // Get work requests for this business vertical
            var workRequests = await _context.WorkRequests
                .Where(wr => wr.BusinessVerticalId == businessVerticalId && wr.Status != WorkIntakeSystem.Core.Enums.WorkStatus.Closed)
                .ToListAsync();

            result.TotalWorkRequests = workRequests.Count;

            // Calculate current vs new priorities (simplified logic)
            foreach (var workRequest in workRequests.Take(100)) // Limit for performance
            {
                var currentPriority = workRequest.Priority;
                var newPriority = await TestPriorityCalculationAsync(businessVerticalId, workRequest);
                
                if (Math.Abs(currentPriority - newPriority) > 0.05m) // 5% threshold
                {
                    result.Changes.Add(new PriorityChangePreview
                    {
                        WorkRequestId = workRequest.Id,
                        Title = workRequest.Title,
                        CurrentPriority = currentPriority,
                        NewPriority = newPriority,
                        Change = newPriority - currentPriority,
                        ChangeReason = "Algorithm configuration change"
                    });
                }
            }

            result.AffectedWorkRequests = result.Changes.Count;

            // Generate distribution comparison (simplified)
            result.DistributionComparison.CurrentDistribution = workRequests
                .GroupBy(wr => GetPriorityBucket(wr.Priority))
                .ToDictionary(g => g.Key, g => g.Count());

            _logger.LogInformation("Generated priority preview for business vertical {BusinessVerticalId}: {AffectedCount} affected",
                businessVerticalId, result.AffectedWorkRequests);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing priority changes for business vertical {BusinessVerticalId}", businessVerticalId);
            return new PriorityPreviewResult();
        }
    }

    // Time Decay Configuration
    public async Task<TimeDecayConfig> GetTimeDecayConfigAsync(int businessVerticalId)
    {
        try
        {
            var configJson = await _configurationService.GetValueAsync($"TimeDecay:BusinessVertical_{businessVerticalId}");
            
            if (string.IsNullOrEmpty(configJson))
            {
                return new TimeDecayConfig();
            }

            return JsonSerializer.Deserialize<TimeDecayConfig>(configJson) ?? new TimeDecayConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time decay config for business vertical {BusinessVerticalId}", businessVerticalId);
            return new TimeDecayConfig();
        }
    }

    public async Task<bool> SetTimeDecayConfigAsync(int businessVerticalId, TimeDecayConfig config)
    {
        try
        {
            var configJson = JsonSerializer.Serialize(config);
            
            var systemConfig = new SystemConfiguration
            {
                ConfigurationKey = $"TimeDecay:BusinessVertical_{businessVerticalId}",
                ConfigurationValue = configJson,
                DataType = "JSON",
                BusinessVerticalId = businessVerticalId,
                Description = "Time decay configuration for priority calculation",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.SystemConfigurations.Add(systemConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set time decay config for business vertical {BusinessVerticalId}", businessVerticalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting time decay config for business vertical {BusinessVerticalId}", businessVerticalId);
            return false;
        }
    }

    public async Task<decimal> CalculateTimeDecayFactorAsync(DateTime createdDate, TimeDecayConfig config)
    {
        if (!config.IsEnabled) return 1.0m;

        var daysOld = (DateTime.UtcNow - createdDate).Days;
        
        if (daysOld < config.StartDelayDays) return 1.0m;

        var effectiveDays = daysOld - config.StartDelayDays;

        var factor = config.DecayFunction.ToLower() switch
        {
            "linear" => 1.0m + (config.DecayRate * effectiveDays),
            "exponential" => (decimal)Math.Pow((double)(1.0m + config.DecayRate), effectiveDays),
            "logarithmic" or _ => 1.0m + (decimal)(Math.Log(effectiveDays + 1) * (double)config.DecayRate)
        };

        return Math.Min(config.MaxMultiplier, Math.Max(1.0m, factor));
    }

    // Business Value Weights
    public async Task<BusinessValueWeightConfig> GetBusinessValueWeightsAsync(int businessVerticalId)
    {
        try
        {
            var configJson = await _configurationService.GetValueAsync($"BusinessValueWeights:BusinessVertical_{businessVerticalId}");
            
            if (string.IsNullOrEmpty(configJson))
            {
                return new BusinessValueWeightConfig();
            }

            return JsonSerializer.Deserialize<BusinessValueWeightConfig>(configJson) ?? new BusinessValueWeightConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business value weights for business vertical {BusinessVerticalId}", businessVerticalId);
            return new BusinessValueWeightConfig();
        }
    }

    public async Task<bool> SetBusinessValueWeightsAsync(int businessVerticalId, BusinessValueWeightConfig config)
    {
        try
        {
            var configJson = JsonSerializer.Serialize(config);
            
            var systemConfig = new SystemConfiguration
            {
                ConfigurationKey = $"BusinessValueWeights:BusinessVertical_{businessVerticalId}",
                ConfigurationValue = configJson,
                DataType = "JSON",
                BusinessVerticalId = businessVerticalId,
                Description = "Business value weight configuration",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.SystemConfigurations.Add(systemConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set business value weights for business vertical {BusinessVerticalId}", businessVerticalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting business value weights for business vertical {BusinessVerticalId}", businessVerticalId);
            return false;
        }
    }

    // Capacity Adjustment Factors
    public async Task<CapacityAdjustmentConfig> GetCapacityAdjustmentConfigAsync(int businessVerticalId)
    {
        try
        {
            var configJson = await _configurationService.GetValueAsync($"CapacityAdjustment:BusinessVertical_{businessVerticalId}");
            
            if (string.IsNullOrEmpty(configJson))
            {
                return new CapacityAdjustmentConfig();
            }

            return JsonSerializer.Deserialize<CapacityAdjustmentConfig>(configJson) ?? new CapacityAdjustmentConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting capacity adjustment config for business vertical {BusinessVerticalId}", businessVerticalId);
            return new CapacityAdjustmentConfig();
        }
    }

    public async Task<bool> SetCapacityAdjustmentConfigAsync(int businessVerticalId, CapacityAdjustmentConfig config)
    {
        try
        {
            var configJson = JsonSerializer.Serialize(config);
            
            var systemConfig = new SystemConfiguration
            {
                ConfigurationKey = $"CapacityAdjustment:BusinessVertical_{businessVerticalId}",
                ConfigurationValue = configJson,
                DataType = "JSON",
                BusinessVerticalId = businessVerticalId,
                Description = "Capacity adjustment configuration",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.SystemConfigurations.Add(systemConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set capacity adjustment config for business vertical {BusinessVerticalId}", businessVerticalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting capacity adjustment config for business vertical {BusinessVerticalId}", businessVerticalId);
            return false;
        }
    }

    // Auto-adjustment Rules
    public async Task<AutoAdjustmentRulesConfig> GetAutoAdjustmentRulesAsync(int businessVerticalId)
    {
        try
        {
            var configJson = await _configurationService.GetValueAsync($"AutoAdjustmentRules:BusinessVertical_{businessVerticalId}");
            
            if (string.IsNullOrEmpty(configJson))
            {
                return new AutoAdjustmentRulesConfig();
            }

            return JsonSerializer.Deserialize<AutoAdjustmentRulesConfig>(configJson) ?? new AutoAdjustmentRulesConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auto adjustment rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return new AutoAdjustmentRulesConfig();
        }
    }

    public async Task<bool> SetAutoAdjustmentRulesAsync(int businessVerticalId, AutoAdjustmentRulesConfig config)
    {
        try
        {
            var configJson = JsonSerializer.Serialize(config);
            
            var systemConfig = new SystemConfiguration
            {
                ConfigurationKey = $"AutoAdjustmentRules:BusinessVertical_{businessVerticalId}",
                ConfigurationValue = configJson,
                DataType = "JSON",
                BusinessVerticalId = businessVerticalId,
                Description = "Auto adjustment rules configuration",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.SystemConfigurations.Add(systemConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set auto adjustment rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting auto adjustment rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return false;
        }
    }

    public async Task<bool> ProcessAutoAdjustmentsAsync(int? businessVerticalId = null)
    {
        try
        {
            _logger.LogInformation("Processing auto adjustments for business vertical {BusinessVerticalId}", businessVerticalId);
            
            // Implementation would process auto-adjustment rules
            // This is a placeholder implementation
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto adjustments");
            return false;
        }
    }

    // Escalation and SLA Management
    public async Task<EscalationRulesConfig> GetEscalationRulesAsync(int businessVerticalId)
    {
        try
        {
            var configJson = await _configurationService.GetValueAsync($"EscalationRules:BusinessVertical_{businessVerticalId}");
            
            if (string.IsNullOrEmpty(configJson))
            {
                return new EscalationRulesConfig();
            }

            return JsonSerializer.Deserialize<EscalationRulesConfig>(configJson) ?? new EscalationRulesConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return new EscalationRulesConfig();
        }
    }

    public async Task<bool> SetEscalationRulesAsync(int businessVerticalId, EscalationRulesConfig config)
    {
        try
        {
            var configJson = JsonSerializer.Serialize(config);
            
            var systemConfig = new SystemConfiguration
            {
                ConfigurationKey = $"EscalationRules:BusinessVertical_{businessVerticalId}",
                ConfigurationValue = configJson,
                DataType = "JSON",
                BusinessVerticalId = businessVerticalId,
                Description = "Escalation rules configuration",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.SystemConfigurations.Add(systemConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set escalation rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting escalation rules for business vertical {BusinessVerticalId}", businessVerticalId);
            return false;
        }
    }

    public async Task<IEnumerable<PriorityEscalation>> GetPendingEscalationsAsync(int? businessVerticalId = null)
    {
        try
        {
            var query = _context.WorkRequests
                .Where(wr => wr.Status != WorkIntakeSystem.Core.Enums.WorkStatus.Closed);

            if (businessVerticalId.HasValue)
            {
                query = query.Where(wr => wr.BusinessVerticalId == businessVerticalId.Value);
            }

            var workRequests = await query.ToListAsync();
            var escalations = new List<PriorityEscalation>();

            foreach (var wr in workRequests)
            {
                var hoursOld = (DateTime.UtcNow - wr.CreatedDate).TotalHours;
                var escalationRules = await GetEscalationRulesAsync(wr.BusinessVerticalId);
                
                if (escalationRules.IsEnabled && hoursOld > escalationRules.DefaultSLAHours)
                {
                    escalations.Add(new PriorityEscalation
                    {
                        WorkRequestId = wr.Id,
                        WorkRequestTitle = wr.Title,
                        CreatedDate = wr.CreatedDate,
                        EscalationDate = wr.CreatedDate.AddHours(escalationRules.DefaultSLAHours),
                        EscalationReason = "SLA threshold exceeded",
                        CurrentStatus = wr.Status.ToString(),
                        CurrentPriority = wr.Priority,
                        AssignedTo = "Unassigned"
                    });
                }
            }

            return escalations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending escalations");
            return new List<PriorityEscalation>();
        }
    }

    public async Task<bool> ProcessEscalationsAsync(int? businessVerticalId = null)
    {
        try
        {
            _logger.LogInformation("Processing escalations for business vertical {BusinessVerticalId}", businessVerticalId);
            
            // Implementation would process escalation rules
            // This is a placeholder implementation
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing escalations");
            return false;
        }
    }

    // Validation and Testing
    public async Task<PriorityConfigValidationResult> ValidateConfigurationAsync(PriorityConfiguration configuration)
    {
        var result = new PriorityConfigValidationResult();

        try
        {
            // Validate basic properties
            if (string.IsNullOrWhiteSpace(configuration.PriorityName))
                result.Errors.Add("Priority name is required");

            if (configuration.MinScore < 0 || configuration.MinScore > 1)
                result.Errors.Add("Min score must be between 0 and 1");

            if (configuration.MaxScore < 0 || configuration.MaxScore > 1)
                result.Errors.Add("Max score must be between 0 and 1");

            if (configuration.MinScore >= configuration.MaxScore)
                result.Errors.Add("Min score must be less than max score");

            // Validate JSON configurations
            if (!IsValidJson(configuration.TimeDecayConfiguration))
                result.Errors.Add("Invalid time decay configuration JSON");

            if (!IsValidJson(configuration.BusinessValueWeights))
                result.Errors.Add("Invalid business value weights JSON");

            if (!IsValidJson(configuration.CapacityFactors))
                result.Errors.Add("Invalid capacity factors JSON");

            if (!IsValidJson(configuration.AutoAdjustmentRules))
                result.Errors.Add("Invalid auto adjustment rules JSON");

            if (!IsValidJson(configuration.EscalationRules))
                result.Errors.Add("Invalid escalation rules JSON");

            if (!IsValidJson(configuration.NotificationSettings))
                result.Errors.Add("Invalid notification settings JSON");

            // Check for duplicate priority names within same business vertical
            var existingConfig = await GetConfigurationByBusinessVerticalAsync(
                configuration.BusinessVerticalId, configuration.PriorityName);
            
            if (existingConfig != null && existingConfig.Id != configuration.Id)
                result.Errors.Add($"Priority name '{configuration.PriorityName}' already exists for this business vertical");

            // Add warnings
            if (configuration.SLAHours.HasValue && configuration.SLAHours < 24)
                result.Warnings.Add("SLA hours less than 24 may cause excessive notifications");

            if (configuration.EscalationThresholdHours.HasValue && configuration.EscalationThresholdHours < configuration.SLAHours)
                result.Warnings.Add("Escalation threshold should be greater than SLA hours");

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating priority configuration {ConfigurationId}", configuration.Id);
            result.Errors.Add("Validation error occurred");
            result.IsValid = false;
        }

        return result;
    }

    public async Task<IEnumerable<string>> GetValidationErrorsAsync(PriorityConfiguration configuration)
    {
        var validationResult = await ValidateConfigurationAsync(configuration);
        return validationResult.Errors;
    }

    public async Task<bool> IsConfigurationValidAsync(int configurationId)
    {
        var configuration = await GetConfigurationAsync(configurationId);
        if (configuration == null) return false;

        var validationResult = await ValidateConfigurationAsync(configuration);
        return validationResult.IsValid;
    }

    // Analytics and Insights
    public async Task<PriorityTrendAnalysis> GetPriorityTrendsAsync(int businessVerticalId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var workRequests = await _context.WorkRequests
                .Where(wr => wr.BusinessVerticalId == businessVerticalId && 
                            wr.CreatedDate >= fromDate && wr.CreatedDate <= toDate)
                .OrderBy(wr => wr.CreatedDate)
                .ToListAsync();

            var analysis = new PriorityTrendAnalysis();
            
            if (workRequests.Any())
            {
                analysis.AveragePriorityScore = workRequests.Average(wr => wr.Priority);
                
                // Calculate daily trend points
                var dailyGroups = workRequests
                    .GroupBy(wr => wr.CreatedDate.Date)
                    .OrderBy(g => g.Key);

                foreach (var group in dailyGroups)
                {
                    analysis.TrendData.Add(new PriorityTrendPoint
                    {
                        Date = group.Key,
                        AveragePriority = group.Average(wr => wr.Priority),
                        TotalWorkRequests = group.Count(),
                        PriorityDistribution = group
                            .GroupBy(wr => GetPriorityBucket(wr.Priority))
                            .ToDictionary(g => g.Key, g => g.Count())
                    });
                }

                // Calculate volatility (standard deviation)
                var priorities = workRequests.Select(wr => (double)wr.Priority);
                var mean = priorities.Average();
                var variance = priorities.Select(p => Math.Pow(p - mean, 2)).Average();
                analysis.PriorityVolatility = (decimal)Math.Sqrt(variance);

                // Generate insights
                analysis.Insights.Add($"Analyzed {workRequests.Count} work requests over {(toDate - fromDate).Days} days");
                
                if (analysis.PriorityVolatility > 0.2m)
                    analysis.Insights.Add("High priority volatility detected - consider reviewing priority calculation parameters");
                
                if (analysis.AveragePriorityScore < 0.3m)
                    analysis.Insights.Add("Low average priority scores - consider adjusting business value weights");
            }

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority trends for business vertical {BusinessVerticalId}", businessVerticalId);
            return new PriorityTrendAnalysis();
        }
    }

    public async Task<IEnumerable<PriorityEffectivenessMetric>> GetEffectivenessMetricsAsync(int businessVerticalId)
    {
        try
        {
            var metrics = new List<PriorityEffectivenessMetric>();
            
            var workRequests = await _context.WorkRequests
                .Where(wr => wr.BusinessVerticalId == businessVerticalId)
                .ToListAsync();

            if (workRequests.Any())
            {
                // Average priority score
                metrics.Add(new PriorityEffectivenessMetric
                {
                    MetricName = "Average Priority Score",
                    Value = workRequests.Average(wr => wr.Priority),
                    Unit = "Score (0-1)",
                    Description = "Overall average priority score across all work requests"
                });

                // Priority distribution
                var highPriorityCount = workRequests.Count(wr => wr.Priority >= 0.7m);
                var highPriorityPercentage = (decimal)highPriorityCount / workRequests.Count * 100;
                
                metrics.Add(new PriorityEffectivenessMetric
                {
                    MetricName = "High Priority Percentage",
                    Value = highPriorityPercentage,
                    Unit = "Percentage",
                    Description = "Percentage of work requests with high priority (≥0.7)"
                });

                // SLA compliance (simplified calculation)
                var slaCompliantCount = workRequests.Count(wr => wr.ActualDate.HasValue && 
                    wr.TargetDate.HasValue && wr.ActualDate <= wr.TargetDate);
                var slaComplianceRate = workRequests.Count(wr => wr.TargetDate.HasValue) > 0 ?
                    (decimal)slaCompliantCount / workRequests.Count(wr => wr.TargetDate.HasValue) * 100 : 0;
                
                metrics.Add(new PriorityEffectivenessMetric
                {
                    MetricName = "SLA Compliance Rate",
                    Value = slaComplianceRate,
                    Unit = "Percentage",
                    Description = "Percentage of work requests completed within target date"
                });
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effectiveness metrics for business vertical {BusinessVerticalId}", businessVerticalId);
            return new List<PriorityEffectivenessMetric>();
        }
    }

    public async Task<PriorityRecommendation> GetConfigurationRecommendationsAsync(int businessVerticalId)
    {
        try
        {
            var recommendation = new PriorityRecommendation
            {
                RecommendationType = "Algorithm",
                Title = "Priority Configuration Analysis",
                ConfidenceScore = 0.75m
            };

            var workRequests = await _context.WorkRequests
                .Where(wr => wr.BusinessVerticalId == businessVerticalId)
                .ToListAsync();

            if (workRequests.Any())
            {
                var avgPriority = workRequests.Average(wr => wr.Priority);
                var highPriorityCount = workRequests.Count(wr => wr.Priority >= 0.7m);
                var highPriorityPercentage = (decimal)highPriorityCount / workRequests.Count;

                if (avgPriority < 0.3m)
                {
                    recommendation.Description = "Average priority scores are low. Consider increasing business value weights or adjusting time decay factors.";
                    recommendation.SuggestedChanges.Add("BusinessValueWeight", 1.2m);
                    recommendation.Rationale = "Low priority scores may indicate under-prioritization of work requests";
                }
                else if (highPriorityPercentage > 0.8m)
                {
                    recommendation.Description = "Too many high-priority items. Consider tightening priority thresholds or adjusting calculation weights.";
                    recommendation.SuggestedChanges.Add("MaxTimeDecayMultiplier", 1.5m);
                    recommendation.Rationale = "High percentage of high-priority items may reduce prioritization effectiveness";
                }
                else
                {
                    recommendation.Description = "Priority distribution appears balanced. Current configuration is working well.";
                    recommendation.Rationale = "Priority distribution is within optimal ranges";
                }
            }
            else
            {
                recommendation.Description = "Insufficient data for recommendations. Add more work requests to get meaningful insights.";
                recommendation.ConfidenceScore = 0.0m;
            }

            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration recommendations for business vertical {BusinessVerticalId}", businessVerticalId);
            return new PriorityRecommendation
            {
                Title = "Error generating recommendations",
                Description = "Unable to generate recommendations due to system error"
            };
        }
    }

    // Helper methods
    private bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        
        try
        {
            JsonSerializer.Deserialize<object>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetPriorityBucket(decimal priority)
    {
        return priority switch
        {
            >= 0.8m => "Critical",
            >= 0.6m => "High",
            >= 0.4m => "Medium",
            _ => "Low"
        };
    }
} 