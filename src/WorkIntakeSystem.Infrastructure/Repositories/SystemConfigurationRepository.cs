using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Repositories
{
    public interface ISystemConfigurationRepository
    {
        Task<SystemConfiguration?> GetLatestActiveAsync(string key, int? businessVerticalId = null);
        Task<SystemConfiguration?> GetByKeyVersionAsync(string key, int? businessVerticalId, int version);
    }

    public class SystemConfigurationRepository : ISystemConfigurationRepository
    {
        private readonly WorkIntakeDbContext _context;

        public SystemConfigurationRepository(WorkIntakeDbContext context)
        {
            _context = context;
        }

        public async Task<SystemConfiguration?> GetLatestActiveAsync(string key, int? businessVerticalId = null)
        {
            var query = _context.SystemConfigurations
                .Where(c => c.ConfigurationKey == key && c.IsActive);
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            return await query
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
    }
} 