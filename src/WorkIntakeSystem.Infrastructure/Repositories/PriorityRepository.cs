using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Services;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Repositories;

public class PriorityRepository : IPriorityRepository
{
    private readonly WorkIntakeDbContext _context;

    public PriorityRepository(WorkIntakeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Priority>> GetByWorkRequestIdAsync(int workRequestId)
    {
        return await _context.Priorities
            .Include(p => p.Department)
            .Include(p => p.VotedBy)
            .Where(p => p.WorkRequestId == workRequestId)
            .ToListAsync();
    }

    public async Task<Priority?> GetByWorkRequestAndDepartmentAsync(int workRequestId, int departmentId)
    {
        return await _context.Priorities
            .Include(p => p.Department)
            .Include(p => p.VotedBy)
            .FirstOrDefaultAsync(p => p.WorkRequestId == workRequestId && p.DepartmentId == departmentId);
    }

    public async Task<Priority> CreateAsync(Priority priority)
    {
        priority.CreatedDate = DateTime.UtcNow;
        priority.ModifiedDate = DateTime.UtcNow;
        priority.VotedDate = DateTime.UtcNow;
        
        _context.Priorities.Add(priority);
        await _context.SaveChangesAsync();
        return priority;
    }

    public async Task UpdateAsync(Priority priority)
    {
        priority.ModifiedDate = DateTime.UtcNow;
        _context.Priorities.Update(priority);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var priority = await _context.Priorities.FindAsync(id);
        if (priority != null)
        {
            _context.Priorities.Remove(priority);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Priority>> GetByDepartmentAsync(int departmentId)
    {
        return await _context.Priorities
            .Include(p => p.WorkRequest)
            .Include(p => p.VotedBy)
            .Where(p => p.DepartmentId == departmentId)
            .OrderByDescending(p => p.VotedDate)
            .ToListAsync();
    }
}