using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Core.Services;

public class PriorityCalculationService : IPriorityCalculationService
{
    private readonly IWorkRequestRepository _workRequestRepository;
    private readonly IPriorityRepository _priorityRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public PriorityCalculationService(
        IWorkRequestRepository workRequestRepository,
        IPriorityRepository priorityRepository,
        IDepartmentRepository departmentRepository)
    {
        _workRequestRepository = workRequestRepository;
        _priorityRepository = priorityRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<decimal> CalculatePriorityScoreAsync(WorkRequest workRequest)
    {
        var baseScore = await CalculateBasePriorityAsync(workRequest.Id);
        var timeDecay = await CalculateTimeDecayFactorAsync(workRequest.CreatedDate);
        var businessValue = await CalculateBusinessValueWeightAsync(workRequest);
        var capacityAdjustment = await CalculateCapacityAdjustmentAsync(workRequest);

        // Enhanced Priority Score = Base_Score × Time_Decay_Factor × Business_Value_Weight × Capacity_Adjustment
        var enhancedScore = baseScore * timeDecay * businessValue * capacityAdjustment;

        // Normalize to 0-1 scale
        return Math.Min(1.0m, Math.Max(0.0m, enhancedScore));
    }

    public async Task<decimal> CalculateBasePriorityAsync(int workRequestId)
    {
        var priorities = await _priorityRepository.GetByWorkRequestIdAsync(workRequestId);
        var departments = await _departmentRepository.GetAllAsync();

        if (!priorities.Any() || !departments.Any())
            return 0.0m;

        decimal totalWeightedScore = 0;
        decimal totalWeight = 0;

        foreach (var priority in priorities)
        {
            var department = departments.FirstOrDefault(d => d.Id == priority.DepartmentId);
            if (department == null) continue;

            var voteValue = priority.Vote switch
            {
                PriorityVote.High => 1.0m,
                PriorityVote.Medium => 0.5m,
                PriorityVote.Low => 0.1m,
                _ => 0.0m
            };

            totalWeightedScore += department.VotingWeight * voteValue;
            totalWeight += department.VotingWeight;
        }

        return totalWeight > 0 ? totalWeightedScore / totalWeight : 0.0m;
    }

    public async Task<decimal> CalculateTimeDecayFactorAsync(DateTime createdDate)
    {
        var daysOld = (DateTime.UtcNow - createdDate).Days;
        
        // Configurable time decay curve - increases priority for older requests
        // Using a logarithmic curve: 1 + log(days + 1) / 100
        var factor = 1.0m + (decimal)(Math.Log(daysOld + 1) / 100.0);
        
        return Math.Min(2.0m, factor); // Cap at 2x multiplier
    }

    public async Task<decimal> CalculateBusinessValueWeightAsync(WorkRequest workRequest)
    {
        // Business value weight ranges from 1.0 to 2.0
        return 1.0m + workRequest.BusinessValue;
    }

    public async Task<decimal> CalculateCapacityAdjustmentAsync(WorkRequest workRequest)
    {
        var department = await _departmentRepository.GetByIdAsync(workRequest.DepartmentId);
        if (department == null) return 1.0m;

        // Capacity adjustment based on department utilization
        // Higher utilization = lower capacity adjustment (0.5-1.5 range)
        var utilizationFactor = department.CurrentUtilization / 100.0m;
        var adjustment = 1.5m - utilizationFactor;
        
        return Math.Min(1.5m, Math.Max(0.5m, adjustment));
    }

    public async Task UpdatePriorityAsync(int workRequestId)
    {
        var workRequest = await _workRequestRepository.GetByIdAsync(workRequestId);
        if (workRequest == null) return;

        var priorityScore = await CalculatePriorityScoreAsync(workRequest);
        workRequest.Priority = priorityScore;

        // Update priority level based on score
        workRequest.PriorityLevel = priorityScore switch
        {
            >= 0.8m => PriorityLevel.Critical,
            >= 0.6m => PriorityLevel.High,
            >= 0.4m => PriorityLevel.Medium,
            _ => PriorityLevel.Low
        };

        await _workRequestRepository.UpdateAsync(workRequest);
    }

    public async Task RecalculateAllPrioritiesAsync()
    {
        var workRequests = await _workRequestRepository.GetAllActiveAsync();
        
        foreach (var workRequest in workRequests)
        {
            await UpdatePriorityAsync(workRequest.Id);
        }
    }
}

// Repository interfaces (to be implemented in Infrastructure layer)
public interface IWorkRequestRepository
{
    Task<WorkRequest?> GetByIdAsync(int id);
    Task<IEnumerable<WorkRequest>> GetAllActiveAsync();
    Task UpdateAsync(WorkRequest workRequest);
}

public interface IPriorityRepository
{
    Task<IEnumerable<Priority>> GetByWorkRequestIdAsync(int workRequestId);
}

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllAsync();
}