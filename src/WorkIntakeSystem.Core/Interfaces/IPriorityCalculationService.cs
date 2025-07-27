using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IPriorityCalculationService
{
    Task<decimal> CalculatePriorityScoreAsync(WorkRequest workRequest);
    Task<decimal> CalculateBasePriorityAsync(int workRequestId);
    Task<decimal> CalculateTimeDecayFactorAsync(DateTime createdDate);
    Task<decimal> CalculateBusinessValueWeightAsync(WorkRequest workRequest);
    Task<decimal> CalculateCapacityAdjustmentAsync(WorkRequest workRequest);
    Task UpdatePriorityAsync(int workRequestId);
    Task RecalculateAllPrioritiesAsync();
}