using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Repositories
{
    public interface ISystemConfigurationRepository
    {
        // Existing methods
        Task<SystemConfiguration?> GetLatestActiveAsync(string key, int? businessVerticalId = null);
        Task<SystemConfiguration?> GetByKeyVersionAsync(string key, int? businessVerticalId, int version);

        // Configuration Versioning
        Task<IEnumerable<SystemConfiguration>> GetVersionHistoryAsync(string key, int? businessVerticalId = null);
        Task<SystemConfiguration> CreateAsync(SystemConfiguration configuration);
        Task<SystemConfiguration> UpdateAsync(SystemConfiguration configuration);
        Task<bool> DeactivateAsync(int configurationId);
        Task<int> GetNextVersionNumberAsync(string key, int? businessVerticalId = null);

        // Business Vertical Specific Configuration
        Task<SystemConfiguration?> GetEffectiveConfigurationAsync(string key, int businessVerticalId);
        Task<IEnumerable<SystemConfiguration>> GetVerticalConfigurationsAsync(int businessVerticalId);
        Task<IEnumerable<SystemConfiguration>> GetGlobalConfigurationsAsync();
        Task<SystemConfiguration?> GetGlobalConfigurationAsync(string key);

        // Date-based queries
        Task<IEnumerable<SystemConfiguration>> GetActiveConfigurationsAsync(DateTime? asOfDate = null, int? businessVerticalId = null);
        Task<IEnumerable<SystemConfiguration>> GetExpiredConfigurationsAsync(int? businessVerticalId = null);

        // General queries
        Task<IEnumerable<SystemConfiguration>> GetAllByKeyAsync(string key);
        Task<SystemConfiguration?> GetByIdAsync(int id);
        Task<IEnumerable<SystemConfiguration>> GetAllAsync();
    }

    public class SystemConfigurationRepository : ISystemConfigurationRepository
    {
        private readonly WorkIntakeDbContext _context;

        public SystemConfigurationRepository(WorkIntakeDbContext context)
        {
            _context = context;
        }

        // Existing methods
        public async Task<SystemConfiguration?> GetLatestActiveAsync(string key, int? businessVerticalId = null)
        {
            var query = _context.SystemConfigurations
                .Where(c => c.ConfigurationKey == key && c.IsActive);
            
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            
            return await query
                .Where(c => c.EffectiveDate <= DateTime.UtcNow && (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow))
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<SystemConfiguration?> GetByKeyVersionAsync(string key, int? businessVerticalId, int version)
        {
            var query = _context.SystemConfigurations
                .Where(c => c.ConfigurationKey == key && c.Version == version && c.IsActive);
            
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            
            return await query.FirstOrDefaultAsync();
        }

        // Configuration Versioning
        public async Task<IEnumerable<SystemConfiguration>> GetVersionHistoryAsync(string key, int? businessVerticalId = null)
        {
            var query = _context.SystemConfigurations
                .Include(c => c.BusinessVertical)
                .Include(c => c.PreviousVersion)
                .Where(c => c.ConfigurationKey == key);
            
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            
            return await query
                .OrderByDescending(c => c.Version)
                .ToListAsync();
        }

        public async Task<SystemConfiguration> CreateAsync(SystemConfiguration configuration)
        {
            configuration.CreatedDate = DateTime.UtcNow;
            configuration.ModifiedDate = DateTime.UtcNow;
            configuration.IsActive = true;

            _context.SystemConfigurations.Add(configuration);
            await _context.SaveChangesAsync();
            return configuration;
        }

        public async Task<SystemConfiguration> UpdateAsync(SystemConfiguration configuration)
        {
            configuration.ModifiedDate = DateTime.UtcNow;
            _context.SystemConfigurations.Update(configuration);
            await _context.SaveChangesAsync();
            return configuration;
        }

        public async Task<bool> DeactivateAsync(int configurationId)
        {
            var configuration = await _context.SystemConfigurations.FindAsync(configurationId);
            if (configuration == null) return false;

            configuration.IsActive = false;
            configuration.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextVersionNumberAsync(string key, int? businessVerticalId = null)
        {
            var query = _context.SystemConfigurations
                .Where(c => c.ConfigurationKey == key);
            
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            
            var maxVersion = await query.MaxAsync(c => (int?)c.Version) ?? 0;
            return maxVersion + 1;
        }

        // Business Vertical Specific Configuration
        public async Task<SystemConfiguration?> GetEffectiveConfigurationAsync(string key, int businessVerticalId)
        {
            // First, try to get business vertical specific configuration
            var verticalConfig = await GetLatestActiveAsync(key, businessVerticalId);
            if (verticalConfig != null)
                return verticalConfig;

            // If no vertical-specific config, fall back to global configuration
            return await GetLatestActiveAsync(key, null);
        }

        public async Task<IEnumerable<SystemConfiguration>> GetVerticalConfigurationsAsync(int businessVerticalId)
        {
            return await _context.SystemConfigurations
                .Include(c => c.BusinessVertical)
                .Where(c => c.BusinessVerticalId == businessVerticalId && c.IsActive)
                .Where(c => c.EffectiveDate <= DateTime.UtcNow && (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow))
                .OrderBy(c => c.ConfigurationKey)
                .ThenByDescending(c => c.Version)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemConfiguration>> GetGlobalConfigurationsAsync()
        {
            return await _context.SystemConfigurations
                .Where(c => c.BusinessVerticalId == null && c.IsActive)
                .Where(c => c.EffectiveDate <= DateTime.UtcNow && (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow))
                .OrderBy(c => c.ConfigurationKey)
                .ThenByDescending(c => c.Version)
                .ToListAsync();
        }

        public async Task<SystemConfiguration?> GetGlobalConfigurationAsync(string key)
        {
            return await GetLatestActiveAsync(key, null);
        }

        // Date-based queries
        public async Task<IEnumerable<SystemConfiguration>> GetActiveConfigurationsAsync(DateTime? asOfDate = null, int? businessVerticalId = null)
        {
            var effectiveDate = asOfDate ?? DateTime.UtcNow;
            var query = _context.SystemConfigurations
                .Include(c => c.BusinessVertical)
                .Where(c => c.IsActive && c.EffectiveDate <= effectiveDate && (c.ExpirationDate == null || c.ExpirationDate > effectiveDate));
            
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            
            return await query
                .OrderBy(c => c.ConfigurationKey)
                .ThenByDescending(c => c.Version)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemConfiguration>> GetExpiredConfigurationsAsync(int? businessVerticalId = null)
        {
            var query = _context.SystemConfigurations
                .Include(c => c.BusinessVertical)
                .Where(c => c.IsActive && c.ExpirationDate != null && c.ExpirationDate <= DateTime.UtcNow);
            
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            
            return await query
                .OrderBy(c => c.ExpirationDate)
                .ToListAsync();
        }

        // General queries
        public async Task<IEnumerable<SystemConfiguration>> GetAllByKeyAsync(string key)
        {
            return await _context.SystemConfigurations
                .Include(c => c.BusinessVertical)
                .Include(c => c.PreviousVersion)
                .Where(c => c.ConfigurationKey == key)
                .OrderByDescending(c => c.Version)
                .ToListAsync();
        }

        public async Task<SystemConfiguration?> GetByIdAsync(int id)
        {
            return await _context.SystemConfigurations
                .Include(c => c.BusinessVertical)
                .Include(c => c.PreviousVersion)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<SystemConfiguration>> GetAllAsync()
        {
            return await _context.SystemConfigurations
                .Include(c => c.BusinessVertical)
                .Include(c => c.PreviousVersion)
                .OrderByDescending(c => c.Version)
                .ToListAsync();
        }
    }
} 