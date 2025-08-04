using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.Infrastructure.Services;

public class WorkCategoryConfigurationService : IWorkCategoryConfigurationService
{
    private readonly WorkIntakeDbContext _context;
    private readonly ILogger<WorkCategoryConfigurationService> _logger;

    public WorkCategoryConfigurationService(
        WorkIntakeDbContext context,
        ILogger<WorkCategoryConfigurationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkCategoryConfiguration> GetByIdAsync(int id)
    {
        return await _context.WorkCategoryConfigurations
            .Include(wc => wc.BusinessVertical)
            .FirstOrDefaultAsync(wc => wc.Id == id);
    }

    public async Task<IEnumerable<WorkCategoryConfiguration>> GetAllAsync()
    {
        return await _context.WorkCategoryConfigurations
            .Include(wc => wc.BusinessVertical)
            .OrderBy(wc => wc.CategoryName)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkCategoryConfiguration>> GetByBusinessVerticalAsync(int businessVerticalId)
    {
        return await _context.WorkCategoryConfigurations
            .Include(wc => wc.BusinessVertical)
            .Where(wc => wc.BusinessVerticalId == businessVerticalId && wc.IsActive)
            .OrderBy(wc => wc.CategoryName)
            .ToListAsync();
    }

    public async Task<WorkCategoryConfiguration> CreateAsync(WorkCategoryConfiguration configuration)
    {
        configuration.CreatedDate = DateTime.UtcNow;
        configuration.ModifiedDate = DateTime.UtcNow;
        configuration.IsActive = true;

        _context.WorkCategoryConfigurations.Add(configuration);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Work category configuration created: {CategoryId} - {CategoryName}", 
            configuration.Id, configuration.CategoryName);

        return configuration;
    }

    public async Task UpdateAsync(WorkCategoryConfiguration configuration)
    {
        var existing = await _context.WorkCategoryConfigurations.FindAsync(configuration.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {configuration.Id} not found");
        }

        existing.ModifiedDate = DateTime.UtcNow;
        existing.ModifiedBy = configuration.ModifiedBy;
        existing.CategoryName = configuration.CategoryName;
        existing.Description = configuration.Description;
        existing.RequiredFields = configuration.RequiredFields;
        existing.ApprovalMatrix = configuration.ApprovalMatrix;
        existing.ValidationRules = configuration.ValidationRules;
        existing.BusinessVerticalId = configuration.BusinessVerticalId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Work category configuration updated: {CategoryId}", configuration.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var configuration = await _context.WorkCategoryConfigurations.FindAsync(id);
        if (configuration != null)
        {
            configuration.IsActive = false;
            configuration.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Work category configuration deleted: {CategoryId}", id);
        }
    }

    public async Task<object> GenerateDynamicFormAsync(int categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            var formFields = JsonSerializer.Deserialize<List<object>>(category.RequiredFields ?? "[]");
            var dynamicForm = new
            {
                CategoryId = categoryId,
                CategoryName = category.CategoryName,
                Fields = formFields,
                ValidationRules = JsonSerializer.Deserialize<object>(category.ValidationRules ?? "{}"),
                ApprovalMatrix = JsonSerializer.Deserialize<object>(category.ApprovalMatrix ?? "{}")
            };

            return dynamicForm;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dynamic form for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<bool> ValidateFormDataAsync(int categoryId, object formData)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            var validationRules = JsonSerializer.Deserialize<Dictionary<string, object>>(category.ValidationRules ?? "{}");
            var formDataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(formData));

            foreach (var rule in validationRules)
            {
                if (!ValidateField(rule.Key, rule.Value, formDataDict))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating form data for category {CategoryId}", categoryId);
            return false;
        }
    }

    public async Task<object> GetApprovalMatrixAsync(int categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            return JsonSerializer.Deserialize<object>(category.ApprovalMatrix ?? "{}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approval matrix for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task UpdateApprovalMatrixAsync(int categoryId, object approvalMatrix)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            category.ApprovalMatrix = JsonSerializer.Serialize(approvalMatrix);
            category.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Approval matrix updated for category {CategoryId}", categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating approval matrix for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<object> GetValidationRulesAsync(int categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            return JsonSerializer.Deserialize<object>(category.ValidationRules ?? "{}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving validation rules for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task UpdateValidationRulesAsync(int categoryId, object validationRules)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            category.ValidationRules = JsonSerializer.Serialize(validationRules);
            category.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Validation rules updated for category {CategoryId}", categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating validation rules for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<object> GetWorkflowConfigurationAsync(int categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            return JsonSerializer.Deserialize<object>("{}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow configuration for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task UpdateWorkflowConfigurationAsync(int categoryId, object workflowConfig)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            category.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Workflow configuration updated for category {CategoryId}", categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow configuration for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkCategoryConfiguration>> GetActiveCategoriesAsync()
    {
        return await _context.WorkCategoryConfigurations
            .Include(wc => wc.BusinessVertical)
            .Where(wc => wc.IsActive)
            .OrderBy(wc => wc.CategoryName)
            .ToListAsync();
    }

    public async Task<bool> IsCategoryActiveAsync(int categoryId)
    {
        var category = await _context.WorkCategoryConfigurations.FindAsync(categoryId);
        return category?.IsActive ?? false;
    }

    public async Task<IEnumerable<string>> GetCategoryFieldNamesAsync(int categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            var formFields = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(category.RequiredFields ?? "[]");
            return formFields.Select(field => field["name"]?.ToString() ?? "").Where(name => !string.IsNullOrEmpty(name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving field names for category {CategoryId}", categoryId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<object> GetCategoryTemplateAsync(int categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Work category configuration with ID {categoryId} not found");
        }

        try
        {
            var template = new
            {
                CategoryId = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                RequiredFields = JsonSerializer.Deserialize<object>(category.RequiredFields ?? "[]"),
                ValidationRules = JsonSerializer.Deserialize<object>(category.ValidationRules ?? "{}"),
                ApprovalMatrix = JsonSerializer.Deserialize<object>(category.ApprovalMatrix ?? "{}"),
                BusinessVerticalId = category.BusinessVerticalId
            };

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template for category {CategoryId}", categoryId);
            throw;
        }
    }

    private bool ValidateField(string fieldName, object rule, Dictionary<string, object> formData)
    {
        if (!formData.ContainsKey(fieldName))
        {
            return false;
        }

        var fieldValue = formData[fieldName];
        var ruleDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(rule));

        foreach (var validation in ruleDict)
        {
            switch (validation.Key.ToLower())
            {
                case "required":
                    if (validation.Value.ToString() == "true" && (fieldValue == null || fieldValue.ToString() == ""))
                        return false;
                    break;
                case "minlength":
                    if (fieldValue?.ToString().Length < int.Parse(validation.Value.ToString()))
                        return false;
                    break;
                case "maxlength":
                    if (fieldValue?.ToString().Length > int.Parse(validation.Value.ToString()))
                        return false;
                    break;
                case "pattern":
                    // Add regex pattern validation if needed
                    break;
            }
        }

        return true;
    }
} 