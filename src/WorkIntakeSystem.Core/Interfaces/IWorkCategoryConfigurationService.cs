using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IWorkCategoryConfigurationService
{
    // CRUD Operations
    Task<WorkCategoryConfiguration> GetByIdAsync(int id);
    Task<IEnumerable<WorkCategoryConfiguration>> GetAllAsync();
    Task<IEnumerable<WorkCategoryConfiguration>> GetByBusinessVerticalAsync(int businessVerticalId);
    Task<WorkCategoryConfiguration> CreateAsync(WorkCategoryConfiguration configuration);
    Task UpdateAsync(WorkCategoryConfiguration configuration);
    Task DeleteAsync(int id);

    // Dynamic Form Management
    Task<object> GenerateDynamicFormAsync(int categoryId);
    Task<bool> ValidateFormDataAsync(int categoryId, object formData);

    // Approval Matrix Management
    Task<object> GetApprovalMatrixAsync(int categoryId);
    Task UpdateApprovalMatrixAsync(int categoryId, object approvalMatrix);

    // Validation Rules Management
    Task<object> GetValidationRulesAsync(int categoryId);
    Task UpdateValidationRulesAsync(int categoryId, object validationRules);

    // Workflow Configuration
    Task<object> GetWorkflowConfigurationAsync(int categoryId);
    Task UpdateWorkflowConfigurationAsync(int categoryId, object workflowConfig);

    // Advanced Features
    Task<IEnumerable<WorkCategoryConfiguration>> GetActiveCategoriesAsync();
    Task<bool> IsCategoryActiveAsync(int categoryId);
    Task<IEnumerable<string>> GetCategoryFieldNamesAsync(int categoryId);
    Task<object> GetCategoryTemplateAsync(int categoryId);
} 