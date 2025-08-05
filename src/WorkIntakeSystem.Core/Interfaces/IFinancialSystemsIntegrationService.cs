using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IFinancialSystemsIntegrationService
{
    // Budget Tracking
    Task<BudgetAllocation> CreateBudgetAllocationAsync(int workRequestId, decimal amount, string budgetCode, string description);
    Task<bool> UpdateBudgetAllocationAsync(int allocationId, decimal newAmount, string reason);
    Task<BudgetAllocation> GetBudgetAllocationAsync(int allocationId);
    Task<List<BudgetAllocation>> GetBudgetAllocationsByWorkRequestAsync(int workRequestId);
    Task<BudgetSummary> GetBudgetSummaryAsync(int workRequestId);
    
    // Cost Tracking
    Task<CostRecord> RecordCostAsync(int workRequestId, decimal amount, string costType, string description, DateTime date);
    Task<bool> UpdateCostRecordAsync(int costRecordId, decimal newAmount, string reason);
    Task<CostRecord> GetCostRecordAsync(int costRecordId);
    Task<List<CostRecord>> GetCostRecordsByWorkRequestAsync(int workRequestId);
    Task<CostSummary> GetCostSummaryAsync(int workRequestId);
    
    // Cost Allocation
    Task<CostAllocation> AllocateCostAsync(int workRequestId, int costRecordId, string department, string project, decimal percentage);
    Task<bool> UpdateCostAllocationAsync(int allocationId, decimal newPercentage, string reason);
    Task<CostAllocation> GetCostAllocationAsync(int allocationId);
    Task<List<CostAllocation>> GetCostAllocationsByWorkRequestAsync(int workRequestId);
    
    // Financial Reporting
    Task<FinancialReport> GenerateFinancialReportAsync(int workRequestId, DateTime fromDate, DateTime toDate);
    Task<ROIAnalysis> CalculateROIAsync(int workRequestId);
    Task<BudgetVariance> CalculateBudgetVarianceAsync(int workRequestId);
    Task<List<FinancialMetric>> GetFinancialMetricsAsync(int workRequestId);
    
    // External System Integration
    Task<bool> SyncWithFinancialSystemAsync(string systemName, object financialData);
    Task<List<ExternalFinancialSystem>> GetConnectedFinancialSystemsAsync();
    Task<bool> TestFinancialSystemConnectionAsync(string systemName);
}

public class BudgetAllocation
{
    public int Id { get; set; }
    public int WorkRequestId { get; set; }
    public decimal Amount { get; set; }
    public string BudgetCode { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Status { get; set; } = "";
    public decimal RemainingAmount { get; set; }
    public decimal SpentAmount { get; set; }
}

public class BudgetSummary
{
    public int WorkRequestId { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal BudgetUtilizationPercentage { get; set; }
    public List<BudgetAllocation> Allocations { get; set; } = new List<BudgetAllocation>();
}

public class CostRecord
{
    public int Id { get; set; }
    public int WorkRequestId { get; set; }
    public decimal Amount { get; set; }
    public string CostType { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Status { get; set; } = "";
    public string ApprovedBy { get; set; } = "";
    public DateTime? ApprovedAt { get; set; }
}

public class CostSummary
{
    public int WorkRequestId { get; set; }
    public decimal TotalCost { get; set; }
    public decimal ApprovedCost { get; set; }
    public decimal PendingCost { get; set; }
    public decimal RejectedCost { get; set; }
    public List<CostRecord> CostRecords { get; set; } = new List<CostRecord>();
}

public class CostAllocation
{
    public int Id { get; set; }
    public int WorkRequestId { get; set; }
    public int CostRecordId { get; set; }
    public string Department { get; set; } = "";
    public string Project { get; set; } = "";
    public decimal Percentage { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Reason { get; set; } = "";
}

public class FinancialReport
{
    public int WorkRequestId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal BudgetUtilizationPercentage { get; set; }
    public List<CostRecord> CostBreakdown { get; set; } = new List<CostRecord>();
    public List<BudgetAllocation> BudgetBreakdown { get; set; } = new List<BudgetAllocation>();
    public List<CostAllocation> AllocationBreakdown { get; set; } = new List<CostAllocation>();
}

public class ROIAnalysis
{
    public int WorkRequestId { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal ActualReturn { get; set; }
    public decimal ROI { get; set; }
    public decimal ROIPercentage { get; set; }
    public DateTime AnalysisDate { get; set; }
    public string AnalysisNotes { get; set; } = "";
}

public class BudgetVariance
{
    public int WorkRequestId { get; set; }
    public decimal PlannedBudget { get; set; }
    public decimal ActualSpent { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public string VarianceType { get; set; } = ""; // Favorable, Unfavorable
    public string VarianceReason { get; set; } = "";
    public DateTime AnalysisDate { get; set; }
}

public class FinancialMetric
{
    public int WorkRequestId { get; set; }
    public string MetricName { get; set; } = "";
    public decimal Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTime CalculatedAt { get; set; }
    public string Description { get; set; } = "";
}

public class ExternalFinancialSystem
{
    public string SystemName { get; set; } = "";
    public string SystemType { get; set; } = ""; // ERP, Accounting, etc.
    public bool IsConnected { get; set; }
    public DateTime LastSyncTime { get; set; }
    public string Status { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
} 