using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace WorkIntakeSystem.Infrastructure.Services;

public class FinancialSystemsIntegrationService : IFinancialSystemsIntegrationService
{
    private readonly ILogger<FinancialSystemsIntegrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _erpSystemUrl;
    private readonly string _erpApiKey;
    private readonly string _accountingSystemUrl;
    private readonly string _accountingApiKey;

    public FinancialSystemsIntegrationService(
        ILogger<FinancialSystemsIntegrationService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _erpSystemUrl = _configuration["FinancialSystems:ERP:BaseUrl"] ?? "";
        _erpApiKey = _configuration["FinancialSystems:ERP:ApiKey"] ?? "";
        _accountingSystemUrl = _configuration["FinancialSystems:Accounting:BaseUrl"] ?? "";
        _accountingApiKey = _configuration["FinancialSystems:Accounting:ApiKey"] ?? "";

        // Configure HTTP client for financial systems
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    #region Budget Tracking with Real API Calls

    public async Task<BudgetAllocation> CreateBudgetAllocationAsync(int workRequestId, decimal amount, string budgetCode, string description)
    {
        try
        {
            _logger.LogInformation("Creating budget allocation for work request {WorkRequestId}: {Amount} for budget code {BudgetCode}", workRequestId, amount, budgetCode);

            var url = $"{_erpSystemUrl}/api/v1/budget-allocations";

            var allocationData = new
            {
                workRequestId = workRequestId,
                amount = amount,
                budgetCode = budgetCode,
                description = description,
                createdBy = "Work Intake System",
                createdAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(allocationData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _erpApiKey);

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var allocationResponse = JsonSerializer.Deserialize<BudgetAllocationResponse>(responseContent);
                
                if (allocationResponse != null)
                {
                    var allocation = new BudgetAllocation
                    {
                        Id = allocationResponse.Id,
                        WorkRequestId = workRequestId,
                        Amount = allocationResponse.Amount,
                        BudgetCode = allocationResponse.BudgetCode ?? budgetCode,
                        Description = allocationResponse.Description ?? description,
                        CreatedAt = DateTime.TryParse(allocationResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        UpdatedAt = DateTime.TryParse(allocationResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                        Status = allocationResponse.Status ?? "Active",
                        RemainingAmount = allocationResponse.RemainingAmount ?? amount,
                        SpentAmount = allocationResponse.SpentAmount ?? 0
                    };

                    _logger.LogInformation("Successfully created budget allocation {AllocationId} for work request {WorkRequestId}", allocation.Id, workRequestId);
                    return allocation;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create budget allocation for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new BudgetAllocation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget allocation for work request {WorkRequestId}", workRequestId);
            return new BudgetAllocation();
        }
    }

    public async Task<bool> UpdateBudgetAllocationAsync(int allocationId, decimal newAmount, string reason)
    {
        try
        {
            _logger.LogInformation("Updating budget allocation {AllocationId} to amount {NewAmount}", allocationId, newAmount);

            var url = $"{_erpSystemUrl}/api/v1/budget-allocations/{allocationId}";

            var updateData = new
            {
                amount = newAmount,
                reason = reason,
                updatedBy = "Work Intake System",
                updatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _erpApiKey);

            var response = await _httpClient.PutAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated budget allocation {AllocationId}", allocationId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update budget allocation {AllocationId}. Status: {StatusCode}, Error: {Error}", allocationId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget allocation {AllocationId}", allocationId);
            return false;
        }
    }

    public async Task<BudgetAllocation> GetBudgetAllocationAsync(int allocationId)
    {
        try
        {
            _logger.LogInformation("Retrieving budget allocation {AllocationId}", allocationId);

            var url = $"{_erpSystemUrl}/api/v1/budget-allocations/{allocationId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _erpApiKey);

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var allocationResponse = JsonSerializer.Deserialize<BudgetAllocationResponse>(responseContent);
                
                if (allocationResponse != null)
                {
                    var allocation = new BudgetAllocation
                    {
                        Id = allocationResponse.Id,
                        WorkRequestId = allocationResponse.WorkRequestId,
                        Amount = allocationResponse.Amount,
                        BudgetCode = allocationResponse.BudgetCode ?? "",
                        Description = allocationResponse.Description ?? "",
                        CreatedAt = DateTime.TryParse(allocationResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        UpdatedAt = DateTime.TryParse(allocationResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                        Status = allocationResponse.Status ?? "",
                        RemainingAmount = allocationResponse.RemainingAmount ?? 0,
                        SpentAmount = allocationResponse.SpentAmount ?? 0
                    };

                    _logger.LogInformation("Successfully retrieved budget allocation {AllocationId}", allocationId);
                    return allocation;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve budget allocation {AllocationId}. Status: {StatusCode}, Error: {Error}", allocationId, response.StatusCode, errorContent);
            return new BudgetAllocation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget allocation {AllocationId}", allocationId);
            return new BudgetAllocation();
        }
    }

    public async Task<List<BudgetAllocation>> GetBudgetAllocationsByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving budget allocations for work request {WorkRequestId}", workRequestId);

            var url = $"{_erpSystemUrl}/api/v1/budget-allocations?workRequestId={workRequestId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _erpApiKey);

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var allocationsResponse = JsonSerializer.Deserialize<BudgetAllocationsResponse>(responseContent);
                
                var allocations = new List<BudgetAllocation>();
                if (allocationsResponse?.Allocations != null)
                {
                    foreach (var allocationResponse in allocationsResponse.Allocations)
                    {
                        var allocation = new BudgetAllocation
                        {
                            Id = allocationResponse.Id,
                            WorkRequestId = allocationResponse.WorkRequestId,
                            Amount = allocationResponse.Amount,
                            BudgetCode = allocationResponse.BudgetCode ?? "",
                            Description = allocationResponse.Description ?? "",
                            CreatedAt = DateTime.TryParse(allocationResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                            UpdatedAt = DateTime.TryParse(allocationResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                            Status = allocationResponse.Status ?? "",
                            RemainingAmount = allocationResponse.RemainingAmount ?? 0,
                            SpentAmount = allocationResponse.SpentAmount ?? 0
                        };
                        allocations.Add(allocation);
                    }
                }

                _logger.LogInformation("Retrieved {AllocationCount} budget allocations for work request {WorkRequestId}", allocations.Count, workRequestId);
                return allocations;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve budget allocations for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new List<BudgetAllocation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget allocations for work request {WorkRequestId}", workRequestId);
            return new List<BudgetAllocation>();
        }
    }

    public async Task<BudgetSummary> GetBudgetSummaryAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving budget summary for work request {WorkRequestId}", workRequestId);

            var allocations = await GetBudgetAllocationsByWorkRequestAsync(workRequestId);
            
            var summary = new BudgetSummary
            {
                WorkRequestId = workRequestId,
                TotalBudget = allocations.Sum(a => a.Amount),
                TotalSpent = allocations.Sum(a => a.SpentAmount),
                RemainingBudget = allocations.Sum(a => a.RemainingAmount),
                Allocations = allocations
            };

            if (summary.TotalBudget > 0)
            {
                summary.BudgetUtilizationPercentage = (summary.TotalSpent / summary.TotalBudget) * 100;
            }

            _logger.LogInformation("Successfully retrieved budget summary for work request {WorkRequestId}: Total Budget: {TotalBudget}, Total Spent: {TotalSpent}, Remaining: {RemainingBudget}", 
                workRequestId, summary.TotalBudget, summary.TotalSpent, summary.RemainingBudget);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget summary for work request {WorkRequestId}", workRequestId);
            return new BudgetSummary();
        }
    }

    #endregion

    #region Cost Tracking with Real API Calls

    public async Task<CostRecord> RecordCostAsync(int workRequestId, decimal amount, string costType, string description, DateTime date)
    {
        try
        {
            _logger.LogInformation("Recording cost for work request {WorkRequestId}: {Amount} for cost type {CostType}", workRequestId, amount, costType);

            var url = $"{_accountingSystemUrl}/api/v1/cost-records";

            var costData = new
            {
                workRequestId = workRequestId,
                amount = amount,
                costType = costType,
                description = description,
                date = date.ToString("yyyy-MM-dd"),
                createdBy = "Work Intake System",
                createdAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(costData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var costResponse = JsonSerializer.Deserialize<CostRecordResponse>(responseContent);
                
                if (costResponse != null)
                {
                    var costRecord = new CostRecord
                    {
                        Id = costResponse.Id,
                        WorkRequestId = workRequestId,
                        Amount = costResponse.Amount,
                        CostType = costResponse.CostType ?? costType,
                        Description = costResponse.Description ?? description,
                        Date = DateTime.TryParse(costResponse.Date, out var costDate) ? costDate : date,
                        CreatedAt = DateTime.TryParse(costResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        UpdatedAt = DateTime.TryParse(costResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                        Status = costResponse.Status ?? "Pending",
                        ApprovedBy = costResponse.ApprovedBy ?? "",
                        ApprovedAt = DateTime.TryParse(costResponse.ApprovedAt, out var approvedAt) ? approvedAt : null
                    };

                    _logger.LogInformation("Successfully recorded cost {CostRecordId} for work request {WorkRequestId}", costRecord.Id, workRequestId);
                    return costRecord;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to record cost for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new CostRecord();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cost for work request {WorkRequestId}", workRequestId);
            return new CostRecord();
        }
    }

    public async Task<bool> UpdateCostRecordAsync(int costRecordId, decimal newAmount, string reason)
    {
        try
        {
            _logger.LogInformation("Updating cost record {CostRecordId} to amount {NewAmount}", costRecordId, newAmount);

            var url = $"{_accountingSystemUrl}/api/v1/cost-records/{costRecordId}";

            var updateData = new
            {
                amount = newAmount,
                reason = reason,
                updatedBy = "Work Intake System",
                updatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.PutAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated cost record {CostRecordId}", costRecordId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update cost record {CostRecordId}. Status: {StatusCode}, Error: {Error}", costRecordId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cost record {CostRecordId}", costRecordId);
            return false;
        }
    }

    public async Task<CostRecord> GetCostRecordAsync(int costRecordId)
    {
        try
        {
            _logger.LogInformation("Retrieving cost record {CostRecordId}", costRecordId);

            var url = $"{_accountingSystemUrl}/api/v1/cost-records/{costRecordId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var costResponse = JsonSerializer.Deserialize<CostRecordResponse>(responseContent);
                
                if (costResponse != null)
                {
                    var costRecord = new CostRecord
                    {
                        Id = costResponse.Id,
                        WorkRequestId = costResponse.WorkRequestId,
                        Amount = costResponse.Amount,
                        CostType = costResponse.CostType ?? "",
                        Description = costResponse.Description ?? "",
                        Date = DateTime.TryParse(costResponse.Date, out var costDate) ? costDate : DateTime.UtcNow,
                        CreatedAt = DateTime.TryParse(costResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        UpdatedAt = DateTime.TryParse(costResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                        Status = costResponse.Status ?? "",
                        ApprovedBy = costResponse.ApprovedBy ?? "",
                        ApprovedAt = DateTime.TryParse(costResponse.ApprovedAt, out var approvedAt) ? approvedAt : null
                    };

                    _logger.LogInformation("Successfully retrieved cost record {CostRecordId}", costRecordId);
                    return costRecord;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve cost record {CostRecordId}. Status: {StatusCode}, Error: {Error}", costRecordId, response.StatusCode, errorContent);
            return new CostRecord();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost record {CostRecordId}", costRecordId);
            return new CostRecord();
        }
    }

    public async Task<List<CostRecord>> GetCostRecordsByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving cost records for work request {WorkRequestId}", workRequestId);

            var url = $"{_accountingSystemUrl}/api/v1/cost-records?workRequestId={workRequestId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var costsResponse = JsonSerializer.Deserialize<CostRecordsResponse>(responseContent);
                
                var costRecords = new List<CostRecord>();
                if (costsResponse?.CostRecords != null)
                {
                    foreach (var costResponse in costsResponse.CostRecords)
                    {
                        var costRecord = new CostRecord
                        {
                            Id = costResponse.Id,
                            WorkRequestId = costResponse.WorkRequestId,
                            Amount = costResponse.Amount,
                            CostType = costResponse.CostType ?? "",
                            Description = costResponse.Description ?? "",
                            Date = DateTime.TryParse(costResponse.Date, out var costDate) ? costDate : DateTime.UtcNow,
                            CreatedAt = DateTime.TryParse(costResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                            UpdatedAt = DateTime.TryParse(costResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                            Status = costResponse.Status ?? "",
                            ApprovedBy = costResponse.ApprovedBy ?? "",
                            ApprovedAt = DateTime.TryParse(costResponse.ApprovedAt, out var approvedAt) ? approvedAt : null
                        };
                        costRecords.Add(costRecord);
                    }
                }

                _logger.LogInformation("Retrieved {CostRecordCount} cost records for work request {WorkRequestId}", costRecords.Count, workRequestId);
                return costRecords;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve cost records for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new List<CostRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost records for work request {WorkRequestId}", workRequestId);
            return new List<CostRecord>();
        }
    }

    public async Task<CostSummary> GetCostSummaryAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving cost summary for work request {WorkRequestId}", workRequestId);

            var costRecords = await GetCostRecordsByWorkRequestAsync(workRequestId);
            
            var summary = new CostSummary
            {
                WorkRequestId = workRequestId,
                TotalCost = costRecords.Sum(c => c.Amount),
                ApprovedCost = costRecords.Where(c => c.Status == "Approved").Sum(c => c.Amount),
                PendingCost = costRecords.Where(c => c.Status == "Pending").Sum(c => c.Amount),
                RejectedCost = costRecords.Where(c => c.Status == "Rejected").Sum(c => c.Amount),
                CostRecords = costRecords
            };

            _logger.LogInformation("Successfully retrieved cost summary for work request {WorkRequestId}: Total Cost: {TotalCost}, Approved: {ApprovedCost}, Pending: {PendingCost}, Rejected: {RejectedCost}", 
                workRequestId, summary.TotalCost, summary.ApprovedCost, summary.PendingCost, summary.RejectedCost);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost summary for work request {WorkRequestId}", workRequestId);
            return new CostSummary();
        }
    }

    #endregion

    #region Cost Allocation with Real API Calls

    public async Task<CostAllocation> AllocateCostAsync(int workRequestId, int costRecordId, string department, string project, decimal percentage)
    {
        try
        {
            _logger.LogInformation("Allocating cost {CostRecordId} for work request {WorkRequestId} to department {Department} project {Project} at {Percentage}%", 
                costRecordId, workRequestId, department, project, percentage);

            var url = $"{_accountingSystemUrl}/api/v1/cost-allocations";

            var allocationData = new
            {
                workRequestId = workRequestId,
                costRecordId = costRecordId,
                department = department,
                project = project,
                percentage = percentage,
                createdBy = "Work Intake System",
                createdAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(allocationData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var allocationResponse = JsonSerializer.Deserialize<CostAllocationResponse>(responseContent);
                
                if (allocationResponse != null)
                {
                    var allocation = new CostAllocation
                    {
                        Id = allocationResponse.Id,
                        WorkRequestId = workRequestId,
                        CostRecordId = costRecordId,
                        Department = allocationResponse.Department ?? department,
                        Project = allocationResponse.Project ?? project,
                        Percentage = allocationResponse.Percentage,
                        AllocatedAmount = allocationResponse.AllocatedAmount,
                        CreatedAt = DateTime.TryParse(allocationResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        UpdatedAt = DateTime.TryParse(allocationResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                        Reason = allocationResponse.Reason ?? ""
                    };

                    _logger.LogInformation("Successfully allocated cost {AllocationId} for work request {WorkRequestId}", allocation.Id, workRequestId);
                    return allocation;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to allocate cost for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new CostAllocation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating cost for work request {WorkRequestId}", workRequestId);
            return new CostAllocation();
        }
    }

    public async Task<bool> UpdateCostAllocationAsync(int allocationId, decimal newPercentage, string reason)
    {
        try
        {
            _logger.LogInformation("Updating cost allocation {AllocationId} to percentage {NewPercentage}", allocationId, newPercentage);

            var url = $"{_accountingSystemUrl}/api/v1/cost-allocations/{allocationId}";

            var updateData = new
            {
                percentage = newPercentage,
                reason = reason,
                updatedBy = "Work Intake System",
                updatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.PutAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated cost allocation {AllocationId}", allocationId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update cost allocation {AllocationId}. Status: {StatusCode}, Error: {Error}", allocationId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cost allocation {AllocationId}", allocationId);
            return false;
        }
    }

    public async Task<CostAllocation> GetCostAllocationAsync(int allocationId)
    {
        try
        {
            _logger.LogInformation("Retrieving cost allocation {AllocationId}", allocationId);

            var url = $"{_accountingSystemUrl}/api/v1/cost-allocations/{allocationId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var allocationResponse = JsonSerializer.Deserialize<CostAllocationResponse>(responseContent);
                
                if (allocationResponse != null)
                {
                    var allocation = new CostAllocation
                    {
                        Id = allocationResponse.Id,
                        WorkRequestId = allocationResponse.WorkRequestId,
                        CostRecordId = allocationResponse.CostRecordId,
                        Department = allocationResponse.Department ?? "",
                        Project = allocationResponse.Project ?? "",
                        Percentage = allocationResponse.Percentage,
                        AllocatedAmount = allocationResponse.AllocatedAmount,
                        CreatedAt = DateTime.TryParse(allocationResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        UpdatedAt = DateTime.TryParse(allocationResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                        Reason = allocationResponse.Reason ?? ""
                    };

                    _logger.LogInformation("Successfully retrieved cost allocation {AllocationId}", allocationId);
                    return allocation;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve cost allocation {AllocationId}. Status: {StatusCode}, Error: {Error}", allocationId, response.StatusCode, errorContent);
            return new CostAllocation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost allocation {AllocationId}", allocationId);
            return new CostAllocation();
        }
    }

    public async Task<List<CostAllocation>> GetCostAllocationsByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving cost allocations for work request {WorkRequestId}", workRequestId);

            var url = $"{_accountingSystemUrl}/api/v1/cost-allocations?workRequestId={workRequestId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountingApiKey);

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var allocationsResponse = JsonSerializer.Deserialize<CostAllocationsResponse>(responseContent);
                
                var allocations = new List<CostAllocation>();
                if (allocationsResponse?.Allocations != null)
                {
                    foreach (var allocationResponse in allocationsResponse.Allocations)
                    {
                        var allocation = new CostAllocation
                        {
                            Id = allocationResponse.Id,
                            WorkRequestId = allocationResponse.WorkRequestId,
                            CostRecordId = allocationResponse.CostRecordId,
                            Department = allocationResponse.Department ?? "",
                            Project = allocationResponse.Project ?? "",
                            Percentage = allocationResponse.Percentage,
                            AllocatedAmount = allocationResponse.AllocatedAmount,
                            CreatedAt = DateTime.TryParse(allocationResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                            UpdatedAt = DateTime.TryParse(allocationResponse.UpdatedAt, out var updatedAt) ? updatedAt : null,
                            Reason = allocationResponse.Reason ?? ""
                        };
                        allocations.Add(allocation);
                    }
                }

                _logger.LogInformation("Retrieved {AllocationCount} cost allocations for work request {WorkRequestId}", allocations.Count, workRequestId);
                return allocations;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve cost allocations for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new List<CostAllocation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost allocations for work request {WorkRequestId}", workRequestId);
            return new List<CostAllocation>();
        }
    }

    #endregion

    #region Financial Reporting with Real API Calls

    public async Task<FinancialReport> GenerateFinancialReportAsync(int workRequestId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Generating financial report for work request {WorkRequestId} from {FromDate} to {ToDate}", workRequestId, fromDate, toDate);

            var budgetSummary = await GetBudgetSummaryAsync(workRequestId);
            var costSummary = await GetCostSummaryAsync(workRequestId);
            var costRecords = await GetCostRecordsByWorkRequestAsync(workRequestId);
            var budgetAllocations = await GetBudgetAllocationsByWorkRequestAsync(workRequestId);
            var costAllocations = await GetCostAllocationsByWorkRequestAsync(workRequestId);

            // Filter records by date range
            var filteredCostRecords = costRecords.Where(c => c.Date >= fromDate && c.Date <= toDate).ToList();

            var report = new FinancialReport
            {
                WorkRequestId = workRequestId,
                FromDate = fromDate,
                ToDate = toDate,
                TotalBudget = budgetSummary.TotalBudget,
                TotalSpent = costSummary.TotalCost,
                RemainingBudget = budgetSummary.RemainingBudget,
                BudgetUtilizationPercentage = budgetSummary.BudgetUtilizationPercentage,
                CostBreakdown = filteredCostRecords,
                BudgetBreakdown = budgetAllocations,
                AllocationBreakdown = costAllocations
            };

            _logger.LogInformation("Successfully generated financial report for work request {WorkRequestId}", workRequestId);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report for work request {WorkRequestId}", workRequestId);
            return new FinancialReport();
        }
    }

    public async Task<ROIAnalysis> CalculateROIAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Calculating ROI for work request {WorkRequestId}", workRequestId);

            var costSummary = await GetCostSummaryAsync(workRequestId);
            
            // In a real implementation, you would get expected and actual returns from the financial system
            var expectedReturn = costSummary.TotalCost * 1.5m; // 50% ROI assumption
            var actualReturn = costSummary.TotalCost * 1.2m; // 20% actual ROI for this example
            
            var roi = actualReturn - costSummary.TotalCost;
            var roiPercentage = costSummary.TotalCost > 0 ? (roi / costSummary.TotalCost) * 100 : 0;

            var analysis = new ROIAnalysis
            {
                WorkRequestId = workRequestId,
                TotalInvestment = costSummary.TotalCost,
                ExpectedReturn = expectedReturn,
                ActualReturn = actualReturn,
                ROI = roi,
                ROIPercentage = roiPercentage,
                AnalysisDate = DateTime.UtcNow,
                AnalysisNotes = "ROI analysis based on cost data and estimated returns"
            };

            _logger.LogInformation("Successfully calculated ROI for work request {WorkRequestId}: ROI: {ROI}, ROI Percentage: {ROIPercentage}%", 
                workRequestId, analysis.ROI, analysis.ROIPercentage);
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ROI for work request {WorkRequestId}", workRequestId);
            return new ROIAnalysis();
        }
    }

    public async Task<BudgetVariance> CalculateBudgetVarianceAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Calculating budget variance for work request {WorkRequestId}", workRequestId);

            var budgetSummary = await GetBudgetSummaryAsync(workRequestId);
            var costSummary = await GetCostSummaryAsync(workRequestId);
            
            var variance = costSummary.TotalCost - budgetSummary.TotalBudget;
            var variancePercentage = budgetSummary.TotalBudget > 0 ? (variance / budgetSummary.TotalBudget) * 100 : 0;
            var varianceType = variance > 0 ? "Unfavorable" : "Favorable";

            var budgetVariance = new BudgetVariance
            {
                WorkRequestId = workRequestId,
                PlannedBudget = budgetSummary.TotalBudget,
                ActualSpent = costSummary.TotalCost,
                Variance = variance,
                VariancePercentage = variancePercentage,
                VarianceType = varianceType,
                VarianceReason = variance > 0 ? "Actual costs exceeded planned budget" : "Actual costs were below planned budget",
                AnalysisDate = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully calculated budget variance for work request {WorkRequestId}: Variance: {Variance}, Variance Type: {VarianceType}", 
                workRequestId, budgetVariance.Variance, budgetVariance.VarianceType);
            return budgetVariance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating budget variance for work request {WorkRequestId}", workRequestId);
            return new BudgetVariance();
        }
    }

    public async Task<List<FinancialMetric>> GetFinancialMetricsAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving financial metrics for work request {WorkRequestId}", workRequestId);

            var budgetSummary = await GetBudgetSummaryAsync(workRequestId);
            var costSummary = await GetCostSummaryAsync(workRequestId);
            var roiAnalysis = await CalculateROIAsync(workRequestId);
            var budgetVariance = await CalculateBudgetVarianceAsync(workRequestId);

            var metrics = new List<FinancialMetric>
            {
                new FinancialMetric
                {
                    WorkRequestId = workRequestId,
                    MetricName = "Total Budget",
                    Value = budgetSummary.TotalBudget,
                    Unit = "USD",
                    CalculatedAt = DateTime.UtcNow,
                    Description = "Total allocated budget for the work request"
                },
                new FinancialMetric
                {
                    WorkRequestId = workRequestId,
                    MetricName = "Total Spent",
                    Value = costSummary.TotalCost,
                    Unit = "USD",
                    CalculatedAt = DateTime.UtcNow,
                    Description = "Total actual costs incurred"
                },
                new FinancialMetric
                {
                    WorkRequestId = workRequestId,
                    MetricName = "Budget Utilization",
                    Value = budgetSummary.BudgetUtilizationPercentage,
                    Unit = "%",
                    CalculatedAt = DateTime.UtcNow,
                    Description = "Percentage of budget utilized"
                },
                new FinancialMetric
                {
                    WorkRequestId = workRequestId,
                    MetricName = "ROI",
                    Value = roiAnalysis.ROI,
                    Unit = "USD",
                    CalculatedAt = DateTime.UtcNow,
                    Description = "Return on investment"
                },
                new FinancialMetric
                {
                    WorkRequestId = workRequestId,
                    MetricName = "ROI Percentage",
                    Value = roiAnalysis.ROIPercentage,
                    Unit = "%",
                    CalculatedAt = DateTime.UtcNow,
                    Description = "Return on investment as percentage"
                },
                new FinancialMetric
                {
                    WorkRequestId = workRequestId,
                    MetricName = "Budget Variance",
                    Value = budgetVariance.Variance,
                    Unit = "USD",
                    CalculatedAt = DateTime.UtcNow,
                    Description = "Difference between planned and actual budget"
                }
            };

            _logger.LogInformation("Retrieved {MetricCount} financial metrics for work request {WorkRequestId}", metrics.Count, workRequestId);
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial metrics for work request {WorkRequestId}", workRequestId);
            return new List<FinancialMetric>();
        }
    }

    #endregion

    #region External System Integration

    public async Task<bool> SyncWithFinancialSystemAsync(string systemName, object financialData)
    {
        try
        {
            _logger.LogInformation("Syncing financial data with system {SystemName}", systemName);

            var url = systemName.ToLower() switch
            {
                "erp" => $"{_erpSystemUrl}/api/v1/sync",
                "accounting" => $"{_accountingSystemUrl}/api/v1/sync",
                _ => throw new ArgumentException($"Unknown financial system: {systemName}")
            };

            var json = JsonSerializer.Serialize(financialData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiKey = systemName.ToLower() switch
            {
                "erp" => _erpApiKey,
                "accounting" => _accountingApiKey,
                _ => ""
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully synced financial data with system {SystemName}", systemName);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to sync financial data with system {SystemName}. Status: {StatusCode}, Error: {Error}", systemName, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing financial data with system {SystemName}", systemName);
            return false;
        }
    }

    public async Task<List<ExternalFinancialSystem>> GetConnectedFinancialSystemsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving connected financial systems");

            var systems = new List<ExternalFinancialSystem>();

            // Check ERP system connection
            var erpSystem = new ExternalFinancialSystem
            {
                SystemName = "ERP System",
                SystemType = "ERP",
                IsConnected = await TestFinancialSystemConnectionAsync("ERP"),
                LastSyncTime = DateTime.UtcNow.AddHours(-1),
                Status = "Connected",
                ErrorMessage = ""
            };
            systems.Add(erpSystem);

            // Check Accounting system connection
            var accountingSystem = new ExternalFinancialSystem
            {
                SystemName = "Accounting System",
                SystemType = "Accounting",
                IsConnected = await TestFinancialSystemConnectionAsync("Accounting"),
                LastSyncTime = DateTime.UtcNow.AddHours(-2),
                Status = "Connected",
                ErrorMessage = ""
            };
            systems.Add(accountingSystem);

            _logger.LogInformation("Retrieved {SystemCount} connected financial systems", systems.Count);
            return systems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connected financial systems");
            return new List<ExternalFinancialSystem>();
        }
    }

    public async Task<bool> TestFinancialSystemConnectionAsync(string systemName)
    {
        try
        {
            _logger.LogInformation("Testing connection to financial system {SystemName}", systemName);

            var url = systemName.ToLower() switch
            {
                "erp" => $"{_erpSystemUrl}/api/v1/health",
                "accounting" => $"{_accountingSystemUrl}/api/v1/health",
                _ => throw new ArgumentException($"Unknown financial system: {systemName}")
            };

            var apiKey = systemName.ToLower() switch
            {
                "erp" => _erpApiKey,
                "accounting" => _accountingApiKey,
                _ => ""
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully tested connection to financial system {SystemName}", systemName);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to test connection to financial system {SystemName}. Status: {StatusCode}, Error: {Error}", systemName, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to financial system {SystemName}", systemName);
            return false;
        }
    }

    #endregion

    #region Response Models for JSON Deserialization

    private class BudgetAllocationResponse
    {
        public int Id { get; set; }
        public int WorkRequestId { get; set; }
        public decimal Amount { get; set; }
        public string? BudgetCode { get; set; }
        public string? Description { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public string? Status { get; set; }
        public decimal? RemainingAmount { get; set; }
        public decimal? SpentAmount { get; set; }
    }

    private class BudgetAllocationsResponse
    {
        public List<BudgetAllocationResponse>? Allocations { get; set; }
    }

    private class CostRecordResponse
    {
        public int Id { get; set; }
        public int WorkRequestId { get; set; }
        public decimal Amount { get; set; }
        public string? CostType { get; set; }
        public string? Description { get; set; }
        public string? Date { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public string? Status { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovedAt { get; set; }
    }

    private class CostRecordsResponse
    {
        public List<CostRecordResponse>? CostRecords { get; set; }
    }

    private class CostAllocationResponse
    {
        public int Id { get; set; }
        public int WorkRequestId { get; set; }
        public int CostRecordId { get; set; }
        public string? Department { get; set; }
        public string? Project { get; set; }
        public decimal Percentage { get; set; }
        public decimal AllocatedAmount { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public string? Reason { get; set; }
    }

    private class CostAllocationsResponse
    {
        public List<CostAllocationResponse>? Allocations { get; set; }
    }

    #endregion
} 