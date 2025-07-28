using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WorkIntakeSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventStoreController : ControllerBase
    {
        private readonly WorkIntakeDbContext _context;
        public EventStoreController(WorkIntakeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.EventStore.OrderByDescending(e => e.Timestamp));
        }

        [HttpGet("by-aggregate/{aggregateId}")]
        public IActionResult GetByAggregate(string aggregateId)
        {
            var events = _context.EventStore.Where(e => e.AggregateId == aggregateId).OrderBy(e => e.EventVersion);
            return Ok(events);
        }

        [HttpPost("replay/{aggregateId}")]
        [Authorize(Roles = "SystemAdministrator")]
        public async Task<IActionResult> Replay(string aggregateId)
        {
            // For demo: just return the ordered events. Real replay logic would rebuild state.
            var events = _context.EventStore.Where(e => e.AggregateId == aggregateId).OrderBy(e => e.EventVersion).ToList();
            // TODO: Implement actual replay logic if needed
            return Ok(events);
        }
    }
} 