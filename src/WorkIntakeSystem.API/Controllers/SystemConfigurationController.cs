using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Infrastructure.Repositories;
using System.Threading.Tasks;
using System.Linq;

namespace WorkIntakeSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SystemAdministrator")]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly ISystemConfigurationRepository _repo;
        private readonly WorkIntakeSystem.Infrastructure.Data.WorkIntakeDbContext _context;

        public SystemConfigurationController(ISystemConfigurationRepository repo, WorkIntakeSystem.Infrastructure.Data.WorkIntakeDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.SystemConfigurations);
        }

        [HttpGet("by-key")]
        public async Task<IActionResult> GetByKey(string key, int? businessVerticalId = null, int? version = null)
        {
            if (version.HasValue)
            {
                var config = await _repo.GetByKeyVersionAsync(key, businessVerticalId, version.Value);
                return config != null ? Ok(config) : NotFound();
            }
            else
            {
                var config = await _repo.GetLatestActiveAsync(key, businessVerticalId);
                return config != null ? Ok(config) : NotFound();
            }
        }

        [HttpGet("history")]
        public IActionResult GetHistory(string key, int? businessVerticalId = null)
        {
            var query = _context.SystemConfigurations.Where(c => c.ConfigurationKey == key);
            if (businessVerticalId.HasValue)
                query = query.Where(c => c.BusinessVerticalId == businessVerticalId);
            return Ok(query.OrderByDescending(c => c.Version));
        }

        [HttpPost]
        public async Task<IActionResult> Create(SystemConfiguration config)
        {
            _context.SystemConfigurations.Add(config);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByKey), new { key = config.ConfigurationKey, businessVerticalId = config.BusinessVerticalId, version = config.Version }, config);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SystemConfiguration config)
        {
            var existing = await _context.SystemConfigurations.FindAsync(id);
            if (existing == null) return NotFound();
            _context.Entry(existing).CurrentValues.SetValues(config);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var config = await _context.SystemConfigurations.FindAsync(id);
            if (config == null) return NotFound();
            config.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 