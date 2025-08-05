using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialSystemsController : ControllerBase
{
    private readonly IFinancialSystemsIntegrationService _financialSystemsService;
    private readonly ILogger<FinancialSystemsController> _logger;

    public FinancialSystemsController(
        IFinancialSystemsIntegrationService financialSystemsService,
        ILogger<FinancialSystemsController> logger)
    {
        _financialSystemsService = financialSystemsService;
        _logger = logger;
    }

    #region Budget Tracking

    [HttpPost("budget-allocations")]
    public async Task<IActionResult> CreateBudgetAllocation([FromBody] CreateBudgetAllocationRequest request)
    {
        try
        {
            var allocation = await _financialSystemsService.CreateBudgetAllocationAsync(
                request.WorkRequestId, request.Amount, request.BudgetCode, request.Description);
            
            if (allocation.Id > 0)
            {
                return Ok(new { Success = true, Allocation = allocation });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to create budget allocation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget allocation");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPut("budget-allocations/{allocationId}")]
    public async Task<IActionResult> UpdateBudgetAllocation(int allocationId, [FromBody] UpdateBudgetAllocationRequest request)
    {
        try
        {
            var success = await _financialSystemsService.UpdateBudgetAllocationAsync(
                allocationId, request.NewAmount, request.Reason);
            
            if (success)
            {
                return Ok(new { Success = true, Message = "Budget allocation updated successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to update budget allocation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget allocation {AllocationId}", allocationId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("budget-allocations/{allocationId}")]
    public async Task<IActionResult> GetBudgetAllocation(int allocationId)
    {
        try
        {
            var allocation = await _financialSystemsService.GetBudgetAllocationAsync(allocationId);
            
            if (allocation.Id > 0)
            {
                return Ok(new { Success = true, Allocation = allocation });
            }
            
            return NotFound(new { Success = false, Message = "Budget allocation not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget allocation {AllocationId}", allocationId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/budget-allocations")]
    public async Task<IActionResult> GetBudgetAllocationsByWorkRequest(int workRequestId)
    {
        try
        {
            var allocations = await _financialSystemsService.GetBudgetAllocationsByWorkRequestAsync(workRequestId);
            return Ok(new { Success = true, Allocations = allocations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget allocations for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/budget-summary")]
    public async Task<IActionResult> GetBudgetSummary(int workRequestId)
    {
        try
        {
            var summary = await _financialSystemsService.GetBudgetSummaryAsync(workRequestId);
            return Ok(new { Success = true, Summary = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget summary for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    #endregion

    #region Cost Tracking

    [HttpPost("cost-records")]
    public async Task<IActionResult> RecordCost([FromBody] RecordCostRequest request)
    {
        try
        {
            var costRecord = await _financialSystemsService.RecordCostAsync(
                request.WorkRequestId, request.Amount, request.CostType, request.Description, request.Date);
            
            if (costRecord.Id > 0)
            {
                return Ok(new { Success = true, CostRecord = costRecord });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to record cost" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cost");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPut("cost-records/{costRecordId}")]
    public async Task<IActionResult> UpdateCostRecord(int costRecordId, [FromBody] UpdateCostRecordRequest request)
    {
        try
        {
            var success = await _financialSystemsService.UpdateCostRecordAsync(
                costRecordId, request.NewAmount, request.Reason);
            
            if (success)
            {
                return Ok(new { Success = true, Message = "Cost record updated successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to update cost record" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cost record {CostRecordId}", costRecordId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("cost-records/{costRecordId}")]
    public async Task<IActionResult> GetCostRecord(int costRecordId)
    {
        try
        {
            var costRecord = await _financialSystemsService.GetCostRecordAsync(costRecordId);
            
            if (costRecord.Id > 0)
            {
                return Ok(new { Success = true, CostRecord = costRecord });
            }
            
            return NotFound(new { Success = false, Message = "Cost record not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost record {CostRecordId}", costRecordId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/cost-records")]
    public async Task<IActionResult> GetCostRecordsByWorkRequest(int workRequestId)
    {
        try
        {
            var costRecords = await _financialSystemsService.GetCostRecordsByWorkRequestAsync(workRequestId);
            return Ok(new { Success = true, CostRecords = costRecords });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost records for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/cost-summary")]
    public async Task<IActionResult> GetCostSummary(int workRequestId)
    {
        try
        {
            var summary = await _financialSystemsService.GetCostSummaryAsync(workRequestId);
            return Ok(new { Success = true, Summary = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost summary for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    #endregion

    #region Cost Allocation

    [HttpPost("cost-allocations")]
    public async Task<IActionResult> AllocateCost([FromBody] AllocateCostRequest request)
    {
        try
        {
            var allocation = await _financialSystemsService.AllocateCostAsync(
                request.WorkRequestId, request.CostRecordId, request.Department, request.Project, request.Percentage);
            
            if (allocation.Id > 0)
            {
                return Ok(new { Success = true, Allocation = allocation });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to allocate cost" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating cost");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPut("cost-allocations/{allocationId}")]
    public async Task<IActionResult> UpdateCostAllocation(int allocationId, [FromBody] UpdateCostAllocationRequest request)
    {
        try
        {
            var success = await _financialSystemsService.UpdateCostAllocationAsync(
                allocationId, request.NewPercentage, request.Reason);
            
            if (success)
            {
                return Ok(new { Success = true, Message = "Cost allocation updated successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to update cost allocation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cost allocation {AllocationId}", allocationId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("cost-allocations/{allocationId}")]
    public async Task<IActionResult> GetCostAllocation(int allocationId)
    {
        try
        {
            var allocation = await _financialSystemsService.GetCostAllocationAsync(allocationId);
            
            if (allocation.Id > 0)
            {
                return Ok(new { Success = true, Allocation = allocation });
            }
            
            return NotFound(new { Success = false, Message = "Cost allocation not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost allocation {AllocationId}", allocationId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/cost-allocations")]
    public async Task<IActionResult> GetCostAllocationsByWorkRequest(int workRequestId)
    {
        try
        {
            var allocations = await _financialSystemsService.GetCostAllocationsByWorkRequestAsync(workRequestId);
            return Ok(new { Success = true, Allocations = allocations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost allocations for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    #endregion

    #region Financial Reporting

    [HttpGet("work-requests/{workRequestId}/financial-report")]
    public async Task<IActionResult> GenerateFinancialReport(int workRequestId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var report = await _financialSystemsService.GenerateFinancialReportAsync(workRequestId, fromDate, toDate);
            return Ok(new { Success = true, Report = report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/roi-analysis")]
    public async Task<IActionResult> CalculateROI(int workRequestId)
    {
        try
        {
            var roiAnalysis = await _financialSystemsService.CalculateROIAsync(workRequestId);
            return Ok(new { Success = true, ROIAnalysis = roiAnalysis });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ROI for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/budget-variance")]
    public async Task<IActionResult> CalculateBudgetVariance(int workRequestId)
    {
        try
        {
            var budgetVariance = await _financialSystemsService.CalculateBudgetVarianceAsync(workRequestId);
            return Ok(new { Success = true, BudgetVariance = budgetVariance });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating budget variance for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("work-requests/{workRequestId}/financial-metrics")]
    public async Task<IActionResult> GetFinancialMetrics(int workRequestId)
    {
        try
        {
            var metrics = await _financialSystemsService.GetFinancialMetricsAsync(workRequestId);
            return Ok(new { Success = true, Metrics = metrics });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial metrics for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    #endregion

    #region External System Integration

    [HttpPost("sync/{systemName}")]
    public async Task<IActionResult> SyncWithFinancialSystem(string systemName, [FromBody] object financialData)
    {
        try
        {
            var success = await _financialSystemsService.SyncWithFinancialSystemAsync(systemName, financialData);
            
            if (success)
            {
                return Ok(new { Success = true, Message = $"Successfully synced with {systemName}" });
            }
            
            return BadRequest(new { Success = false, Message = $"Failed to sync with {systemName}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing with financial system {SystemName}", systemName);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("connected-systems")]
    public async Task<IActionResult> GetConnectedFinancialSystems()
    {
        try
        {
            var systems = await _financialSystemsService.GetConnectedFinancialSystemsAsync();
            return Ok(new { Success = true, Systems = systems });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connected financial systems");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("test-connection/{systemName}")]
    public async Task<IActionResult> TestFinancialSystemConnection(string systemName)
    {
        try
        {
            var isConnected = await _financialSystemsService.TestFinancialSystemConnectionAsync(systemName);
            return Ok(new { Success = true, IsConnected = isConnected, SystemName = systemName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to financial system {SystemName}", systemName);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    #endregion
}

#region Request Models

public class CreateBudgetAllocationRequest
{
    public int WorkRequestId { get; set; }
    public decimal Amount { get; set; }
    public string BudgetCode { get; set; } = "";
    public string Description { get; set; } = "";
}

public class UpdateBudgetAllocationRequest
{
    public decimal NewAmount { get; set; }
    public string Reason { get; set; } = "";
}

public class RecordCostRequest
{
    public int WorkRequestId { get; set; }
    public decimal Amount { get; set; }
    public string CostType { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
}

public class UpdateCostRecordRequest
{
    public decimal NewAmount { get; set; }
    public string Reason { get; set; } = "";
}

public class AllocateCostRequest
{
    public int WorkRequestId { get; set; }
    public int CostRecordId { get; set; }
    public string Department { get; set; } = "";
    public string Project { get; set; } = "";
    public decimal Percentage { get; set; }
}

public class UpdateCostAllocationRequest
{
    public decimal NewPercentage { get; set; }
    public string Reason { get; set; } = "";
}

#endregion 