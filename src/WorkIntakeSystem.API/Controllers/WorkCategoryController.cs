using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Authorization;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkCategoryController : ControllerBase
{
    private readonly IWorkCategoryConfigurationService _workCategoryService;
    private readonly IConfigurationService _configurationService;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkCategoryController> _logger;

    public WorkCategoryController(
        IWorkCategoryConfigurationService workCategoryService,
        IConfigurationService configurationService,
        IMapper mapper,
        ILogger<WorkCategoryController> logger)
    {
        _workCategoryService = workCategoryService;
        _configurationService = configurationService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all work category configurations
    /// </summary>
    [HttpGet]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<IEnumerable<WorkCategoryConfiguration>>> GetWorkCategories()
    {
        try
        {
            var categories = await _workCategoryService.GetAllAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work categories");
            return StatusCode(500, "An error occurred while retrieving work categories");
        }
    }

    /// <summary>
    /// Get work category configuration by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<WorkCategoryConfiguration>> GetWorkCategory(int id)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the work category");
        }
    }

    /// <summary>
    /// Get work categories by business vertical
    /// </summary>
    [HttpGet("business-vertical/{businessVerticalId}")]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<IEnumerable<WorkCategoryConfiguration>>> GetWorkCategoriesByBusinessVertical(int businessVerticalId)
    {
        try
        {
            var categories = await _workCategoryService.GetByBusinessVerticalAsync(businessVerticalId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work categories for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving work categories");
        }
    }

    /// <summary>
    /// Create a new work category configuration
    /// </summary>
    [HttpPost]
    [RequirePermission("WorkCategory:Create")]
    public async Task<ActionResult<WorkCategoryConfiguration>> CreateWorkCategory(WorkCategoryConfiguration categoryConfig)
    {
        try
        {
            categoryConfig.CreatedBy = GetCurrentUserName();
            categoryConfig.CreatedDate = DateTime.UtcNow;
            categoryConfig.IsActive = true;

            var createdCategory = await _workCategoryService.CreateAsync(categoryConfig);
            
            _logger.LogInformation("Work category configuration created: {CategoryId} by {User}", 
                createdCategory.Id, GetCurrentUserName());

            return CreatedAtAction(nameof(GetWorkCategory), new { id = createdCategory.Id }, createdCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work category configuration");
            return StatusCode(500, "An error occurred while creating the work category configuration");
        }
    }

    /// <summary>
    /// Update a work category configuration
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("WorkCategory:Update")]
    public async Task<IActionResult> UpdateWorkCategory(int id, WorkCategoryConfiguration updateConfig)
    {
        try
        {
            var existingCategory = await _workCategoryService.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            updateConfig.Id = id;
            updateConfig.ModifiedBy = GetCurrentUserName();
            updateConfig.ModifiedDate = DateTime.UtcNow;

            await _workCategoryService.UpdateAsync(updateConfig);
            
            _logger.LogInformation("Work category configuration updated: {CategoryId} by {User}", 
                id, GetCurrentUserName());

            return Ok(new { message = "Work category configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the work category configuration");
        }
    }

    /// <summary>
    /// Delete a work category configuration
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("WorkCategory:Delete")]
    public async Task<IActionResult> DeleteWorkCategory(int id)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            await _workCategoryService.DeleteAsync(id);
            
            _logger.LogInformation("Work category configuration deleted: {CategoryId} by {User}", 
                id, GetCurrentUserName());

            return Ok(new { message = "Work category configuration deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work category {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the work category configuration");
        }
    }

    /// <summary>
    /// Generate dynamic form for a work category
    /// </summary>
    [HttpGet("{id}/dynamic-form")]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<object>> GetDynamicForm(int id)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            var dynamicForm = await _workCategoryService.GenerateDynamicFormAsync(id);
            return Ok(dynamicForm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dynamic form for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while generating the dynamic form");
        }
    }

    /// <summary>
    /// Get approval matrix for a work category
    /// </summary>
    [HttpGet("{id}/approval-matrix")]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<object>> GetApprovalMatrix(int id)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            var approvalMatrix = await _workCategoryService.GetApprovalMatrixAsync(id);
            return Ok(approvalMatrix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approval matrix for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the approval matrix");
        }
    }

    /// <summary>
    /// Update approval matrix for a work category
    /// </summary>
    [HttpPut("{id}/approval-matrix")]
    [RequirePermission("WorkCategory:Update")]
    public async Task<IActionResult> UpdateApprovalMatrix(int id, [FromBody] object approvalMatrix)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            await _workCategoryService.UpdateApprovalMatrixAsync(id, approvalMatrix);
            
            _logger.LogInformation("Approval matrix updated for category {CategoryId} by {User}", 
                id, GetCurrentUserName());

            return Ok(new { message = "Approval matrix updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating approval matrix for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the approval matrix");
        }
    }

    /// <summary>
    /// Get validation rules for a work category
    /// </summary>
    [HttpGet("{id}/validation-rules")]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<object>> GetValidationRules(int id)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            var validationRules = await _workCategoryService.GetValidationRulesAsync(id);
            return Ok(validationRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving validation rules for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the validation rules");
        }
    }

    /// <summary>
    /// Update validation rules for a work category
    /// </summary>
    [HttpPut("{id}/validation-rules")]
    [RequirePermission("WorkCategory:Update")]
    public async Task<IActionResult> UpdateValidationRules(int id, [FromBody] object validationRules)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            await _workCategoryService.UpdateValidationRulesAsync(id, validationRules);
            
            _logger.LogInformation("Validation rules updated for category {CategoryId} by {User}", 
                id, GetCurrentUserName());

            return Ok(new { message = "Validation rules updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating validation rules for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the validation rules");
        }
    }

    /// <summary>
    /// Get workflow configuration for a work category
    /// </summary>
    [HttpGet("{id}/workflow")]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<object>> GetWorkflowConfiguration(int id)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            var workflowConfig = await _workCategoryService.GetWorkflowConfigurationAsync(id);
            return Ok(workflowConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow configuration for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the workflow configuration");
        }
    }

    /// <summary>
    /// Update workflow configuration for a work category
    /// </summary>
    [HttpPut("{id}/workflow")]
    [RequirePermission("WorkCategory:Update")]
    public async Task<IActionResult> UpdateWorkflowConfiguration(int id, [FromBody] object workflowConfig)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            await _workCategoryService.UpdateWorkflowConfigurationAsync(id, workflowConfig);
            
            _logger.LogInformation("Workflow configuration updated for category {CategoryId} by {User}", 
                id, GetCurrentUserName());

            return Ok(new { message = "Workflow configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow configuration for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the workflow configuration");
        }
    }

    /// <summary>
    /// Validate form data against category rules
    /// </summary>
    [HttpPost("{id}/validate-form")]
    [RequirePermission("WorkCategory:Read")]
    public async Task<ActionResult<object>> ValidateFormData(int id, [FromBody] object formData)
    {
        try
        {
            var category = await _workCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Work category configuration with ID {id} not found");
            }

            var validationResult = await _workCategoryService.ValidateFormDataAsync(id, formData);
            return Ok(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating form data for category {CategoryId}", id);
            return StatusCode(500, "An error occurred while validating the form data");
        }
    }

    private string GetCurrentUserName()
    {
        var userNameClaim = User.FindFirst("Name");
        return userNameClaim?.Value ?? "Unknown User";
    }
} 