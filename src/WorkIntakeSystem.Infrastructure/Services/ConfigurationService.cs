using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Repositories;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ISystemConfigurationRepository _configRepo;
        private readonly IConfiguration _appSettings;

        public ConfigurationService(ISystemConfigurationRepository configRepo, IConfiguration appSettings)
        {
            _configRepo = configRepo;
            _appSettings = appSettings;
        }

        public async Task<string?> GetValueAsync(string key, int? businessVerticalId = null, int? version = null)
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
            return _appSettings[key];
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
    }
} 