using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IPriorityRepository
{
    Task<IEnumerable<Priority>> GetByWorkRequestIdAsync(int workRequestId);
    Task<Priority?> GetByWorkRequestAndDepartmentAsync(int workRequestId, int departmentId);
    Task<Priority> CreateAsync(Priority priority);
    Task UpdateAsync(Priority priority);
    Task DeleteAsync(int id);
    Task<IEnumerable<Priority>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Priority>> GetByDepartmentIdAsync(int departmentId);
    Task<IEnumerable<PendingVoteInfo>> GetPendingVotesForDepartmentAsync(int departmentId);
}

public class PendingVoteInfo
{
    public int WorkRequestId { get; set; }
} 