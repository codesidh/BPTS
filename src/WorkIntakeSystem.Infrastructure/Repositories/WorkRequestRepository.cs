using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Services;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Repositories;

public class WorkRequestRepository : IWorkRequestRepository
{
    private readonly WorkIntakeDbContext _context;

    public WorkRequestRepository(WorkIntakeDbContext context)
    {
        _context = context;
    }

    public async Task<WorkRequest?> GetByIdAsync(int id)
    {
        return await _context.WorkRequests
            .Include(wr => wr.BusinessVertical)
            .Include(wr => wr.Department)
            .Include(wr => wr.Submitter)
            .Include(wr => wr.Capability)
            .Include(wr => wr.PriorityVotes)
                .ThenInclude(p => p.Department)
            .FirstOrDefaultAsync(wr => wr.Id == id);
    }

    public async Task<IEnumerable<WorkRequest>> GetAllActiveAsync()
    {
        return await _context.WorkRequests
            .Include(wr => wr.BusinessVertical)
            .Include(wr => wr.Department)
            .Include(wr => wr.Submitter)
            .Include(wr => wr.PriorityVotes)
            .Where(wr => wr.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkRequest>> GetByBusinessVerticalAsync(int businessVerticalId)
    {
        return await _context.WorkRequests
            .Include(wr => wr.Department)
            .Include(wr => wr.Submitter)
            .Include(wr => wr.PriorityVotes)
            .Where(wr => wr.BusinessVerticalId == businessVerticalId && wr.IsActive)
            .OrderByDescending(wr => wr.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkRequest>> GetByDepartmentAsync(int departmentId)
    {
        return await _context.WorkRequests
            .Include(wr => wr.BusinessVertical)
            .Include(wr => wr.Submitter)
            .Include(wr => wr.PriorityVotes)
            .Where(wr => wr.DepartmentId == departmentId && wr.IsActive)
            .OrderByDescending(wr => wr.Priority)
            .ToListAsync();
    }

    public async Task<WorkRequest> CreateAsync(WorkRequest workRequest)
    {
        workRequest.CreatedDate = DateTime.UtcNow;
        workRequest.ModifiedDate = DateTime.UtcNow;
        
        _context.WorkRequests.Add(workRequest);
        await _context.SaveChangesAsync();
        return workRequest;
    }

    public async Task UpdateAsync(WorkRequest workRequest)
    {
        workRequest.ModifiedDate = DateTime.UtcNow;
        _context.WorkRequests.Update(workRequest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var workRequest = await GetByIdAsync(id);
        if (workRequest != null)
        {
            workRequest.IsActive = false;
            workRequest.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<WorkRequest>> GetPendingPriorityVotesAsync(int departmentId)
    {
        return await _context.WorkRequests
            .Include(wr => wr.PriorityVotes)
            .Where(wr => wr.IsActive && 
                         wr.Status == Core.Enums.WorkStatus.Submitted &&
                         !wr.PriorityVotes.Any(p => p.DepartmentId == departmentId))
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkRequest>> GetByPriorityLevelAsync(Core.Enums.PriorityLevel priorityLevel)
    {
        return await _context.WorkRequests
            .Include(wr => wr.BusinessVertical)
            .Include(wr => wr.Department)
            .Include(wr => wr.Submitter)
            .Where(wr => wr.IsActive && wr.PriorityLevel == priorityLevel)
            .OrderByDescending(wr => wr.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkRequest>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.WorkRequests
            .Include(wr => wr.BusinessVertical)
            .Include(wr => wr.Department)
            .Include(wr => wr.Submitter)
            .Include(wr => wr.PriorityVotes)
            .Where(wr => ids.Contains(wr.Id) && wr.IsActive)
            .ToListAsync();
    }
}