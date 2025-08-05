using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IConfigurationService
    {
        // Existing methods
        Task<string?> GetValueAsync(string key, int? businessVerticalId = null, int? version = null);
        Task<T?> GetValueAsync<T>(string key, int? businessVerticalId = null, int? version = null);

        // Configuration Versioning
        Task<SystemConfiguration> CreateNewVersionAsync(string key, string newValue, string changeReason, int? businessVerticalId = null, string approvedBy = "System");
        Task<IEnumerable<SystemConfiguration>> GetVersionHistoryAsync(string key, int? businessVerticalId = null);
        Task<SystemConfiguration?> GetVersionAsync(string key, int version, int? businessVerticalId = null);
        Task<bool> RollbackToVersionAsync(string key, int targetVersion, string rollbackReason, int? businessVerticalId = null, string approvedBy = "System");

        // Configuration Comparison
        Task<ConfigurationComparison> CompareVersionsAsync(string key, int version1, int version2, int? businessVerticalId = null);
        Task<IEnumerable<ConfigurationComparison>> CompareConfigurationsAsync(int? businessVerticalId1, int? businessVerticalId2);

        // Business Vertical Specific Configuration
        Task<SystemConfiguration> CreateVerticalSpecificConfigAsync(string key, string value, int businessVerticalId, string changeReason, string approvedBy = "System");
        Task<SystemConfiguration?> GetEffectiveConfigurationAsync(string key, int businessVerticalId);
        Task<IEnumerable<SystemConfiguration>> GetVerticalConfigurationsAsync(int businessVerticalId);
        Task<IEnumerable<SystemConfiguration>> GetGlobalConfigurationsAsync();
        Task<bool> InheritFromGlobalAsync(string key, int businessVerticalId, string changeReason, string approvedBy = "System");
        Task<bool> OverrideGlobalConfigAsync(string key, string newValue, int businessVerticalId, string changeReason, string approvedBy = "System");

        // Configuration Validation
        Task<ConfigurationValidationResult> ValidateConfigurationAsync(string key, string value, string dataType, int? businessVerticalId = null);
        Task<IEnumerable<ConfigurationValidationResult>> ValidateAllConfigurationsAsync(int? businessVerticalId = null);

        // Effective/Expiration Date Handling
        Task<IEnumerable<SystemConfiguration>> GetActiveConfigurationsAsync(DateTime? asOfDate = null, int? businessVerticalId = null);
        Task<IEnumerable<SystemConfiguration>> GetExpiredConfigurationsAsync(int? businessVerticalId = null);
        Task<bool> SetExpirationDateAsync(string key, DateTime expirationDate, int? businessVerticalId = null, string approvedBy = "System");
    }

    public class ConfigurationComparison
    {
        public string ConfigurationKey { get; set; } = string.Empty;
        public SystemConfiguration? Version1 { get; set; }
        public SystemConfiguration? Version2 { get; set; }
        public string ComparisonResult { get; set; } = string.Empty;
        public bool HasDifferences { get; set; }
        public DateTime ComparisonDate { get; set; } = DateTime.UtcNow;
    }

    public class ConfigurationValidationResult
    {
        public string ConfigurationKey { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();
        public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
    }
} 