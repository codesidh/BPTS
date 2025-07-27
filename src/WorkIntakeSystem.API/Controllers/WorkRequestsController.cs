using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WorkIntakeSystem.API.DTOs;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Services;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkRequestsController : ControllerBase
{
    private readonly IWorkRequestRepository _workRequestRepository;
    private readonly IPriorityCalculationService _priorityCalculationService;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkRequestsController> _logger;

    public WorkRequestsController(
        IWorkRequestRepository workRequestRepository,
        IPriorityCalculationService priorityCalculationService,
        IMapper mapper,
        ILogger<WorkRequestsController> logger)
    {
        _workRequestRepository = workRequestRepository;
        _priorityCalculationService = priorityCalculationService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all active work requests
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkRequestDto>>> GetWorkRequests()
    {
        try
        {
            var workRequests = await _workRequestRepository.GetAllActiveAsync();
            var workRequestDtos = _mapper.Map<IEnumerable<WorkRequestDto>>(workRequests);
            return Ok(workRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work requests");
            return StatusCode(500, "An error occurred while retrieving work requests");
        }
    }

    /// <summary>
    /// Get work request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkRequestDto>> GetWorkRequest(int id)
    {
        try
        {
            var workRequest = await _workRequestRepository.GetByIdAsync(id);
            if (workRequest == null)
            {
                return NotFound($"Work request with ID {id} not found");
            }

            var workRequestDto = _mapper.Map<WorkRequestDto>(workRequest);
            return Ok(workRequestDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work request {WorkRequestId}", id);
            return StatusCode(500, "An error occurred while retrieving the work request");
        }
    }

    /// <summary>
    /// Get work requests by business vertical
    /// </summary>
    [HttpGet("business-vertical/{businessVerticalId}")]
    public async Task<ActionResult<IEnumerable<WorkRequestDto>>> GetWorkRequestsByBusinessVertical(int businessVerticalId)
    {
        try
        {
            var workRequests = await _workRequestRepository.GetByBusinessVerticalAsync(businessVerticalId);
            var workRequestDtos = _mapper.Map<IEnumerable<WorkRequestDto>>(workRequests);
            return Ok(workRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work requests for business vertical {BusinessVerticalId}", businessVerticalId);
            return StatusCode(500, "An error occurred while retrieving work requests");
        }
    }

    /// <summary>
    /// Get work requests by department
    /// </summary>
    [HttpGet("department/{departmentId}")]
    public async Task<ActionResult<IEnumerable<WorkRequestDto>>> GetWorkRequestsByDepartment(int departmentId)
    {
        try
        {
            var workRequests = await _workRequestRepository.GetByDepartmentAsync(departmentId);
            var workRequestDtos = _mapper.Map<IEnumerable<WorkRequestDto>>(workRequests);
            return Ok(workRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work requests for department {DepartmentId}", departmentId);
            return StatusCode(500, "An error occurred while retrieving work requests");
        }
    }

    /// <summary>
    /// Get work requests by priority level
    /// </summary>
    [HttpGet("priority/{priorityLevel}")]
    public async Task<ActionResult<IEnumerable<WorkRequestDto>>> GetWorkRequestsByPriorityLevel(PriorityLevel priorityLevel)
    {
        try
        {
            var workRequests = await _workRequestRepository.GetByPriorityLevelAsync(priorityLevel);
            var workRequestDtos = _mapper.Map<IEnumerable<WorkRequestDto>>(workRequests);
            return Ok(workRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work requests for priority level {PriorityLevel}", priorityLevel);
            return StatusCode(500, "An error occurred while retrieving work requests");
        }
    }

    /// <summary>
    /// Get pending priority votes for a department
    /// </summary>
    [HttpGet("pending-votes/{departmentId}")]
    public async Task<ActionResult<IEnumerable<WorkRequestDto>>> GetPendingPriorityVotes(int departmentId)
    {
        try
        {
            var workRequests = await _workRequestRepository.GetPendingPriorityVotesAsync(departmentId);
            var workRequestDtos = _mapper.Map<IEnumerable<WorkRequestDto>>(workRequests);
            return Ok(workRequestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending votes for department {DepartmentId}", departmentId);
            return StatusCode(500, "An error occurred while retrieving pending votes");
        }
    }

    /// <summary>
    /// Create a new work request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkRequestDto>> CreateWorkRequest(CreateWorkRequestDto createDto)
    {
        try
        {
            var workRequest = _mapper.Map<WorkRequest>(createDto);
            workRequest.SubmitterId = GetCurrentUserId(); // Get from authentication context
            workRequest.CreatedBy = GetCurrentUserName();
            workRequest.ModifiedBy = GetCurrentUserName();

            var createdWorkRequest = await _workRequestRepository.CreateAsync(workRequest);
            
            // Calculate initial priority
            await _priorityCalculationService.UpdatePriorityAsync(createdWorkRequest.Id);
            
            var workRequestDto = _mapper.Map<WorkRequestDto>(createdWorkRequest);
            
            _logger.LogInformation("Work request {WorkRequestId} created by user {UserId}", 
                createdWorkRequest.Id, GetCurrentUserId());

            return CreatedAtAction(nameof(GetWorkRequest), 
                new { id = createdWorkRequest.Id }, workRequestDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work request");
            return StatusCode(500, "An error occurred while creating the work request");
        }
    }

    /// <summary>
    /// Update an existing work request
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkRequest(int id, UpdateWorkRequestDto updateDto)
    {
        try
        {
            var existingWorkRequest = await _workRequestRepository.GetByIdAsync(id);
            if (existingWorkRequest == null)
            {
                return NotFound($"Work request with ID {id} not found");
            }

            // Map updates to existing entity
            _mapper.Map(updateDto, existingWorkRequest);
            existingWorkRequest.ModifiedBy = GetCurrentUserName();

            await _workRequestRepository.UpdateAsync(existingWorkRequest);
            
            // Recalculate priority after update
            await _priorityCalculationService.UpdatePriorityAsync(id);

            _logger.LogInformation("Work request {WorkRequestId} updated by user {UserId}", 
                id, GetCurrentUserId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work request {WorkRequestId}", id);
            return StatusCode(500, "An error occurred while updating the work request");
        }
    }

    /// <summary>
    /// Delete a work request (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkRequest(int id)
    {
        try
        {
            var workRequest = await _workRequestRepository.GetByIdAsync(id);
            if (workRequest == null)
            {
                return NotFound($"Work request with ID {id} not found");
            }

            await _workRequestRepository.DeleteAsync(id);

            _logger.LogInformation("Work request {WorkRequestId} deleted by user {UserId}", 
                id, GetCurrentUserId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work request {WorkRequestId}", id);
            return StatusCode(500, "An error occurred while deleting the work request");
        }
    }

    /// <summary>
    /// Recalculate priority for a specific work request
    /// </summary>
    [HttpPost("{id}/recalculate-priority")]
    public async Task<IActionResult> RecalculatePriority(int id)
    {
        try
        {
            var workRequest = await _workRequestRepository.GetByIdAsync(id);
            if (workRequest == null)
            {
                return NotFound($"Work request with ID {id} not found");
            }

            await _priorityCalculationService.UpdatePriorityAsync(id);

            _logger.LogInformation("Priority recalculated for work request {WorkRequestId} by user {UserId}", 
                id, GetCurrentUserId());

            return Ok(new { message = "Priority recalculated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating priority for work request {WorkRequestId}", id);
            return StatusCode(500, "An error occurred while recalculating priority");
        }
    }

    /// <summary>
    /// Recalculate all priorities
    /// </summary>
    [HttpPost("recalculate-all-priorities")]
    public async Task<IActionResult> RecalculateAllPriorities()
    {
        try
        {
            await _priorityCalculationService.RecalculateAllPrioritiesAsync();

            _logger.LogInformation("All priorities recalculated by user {UserId}", GetCurrentUserId());

            return Ok(new { message = "All priorities recalculated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating all priorities");
            return StatusCode(500, "An error occurred while recalculating priorities");
        }
    }

    private int GetCurrentUserId()
    {
        // This should be implemented to get the current user ID from the authentication context
        // For now, returning a placeholder value
        return 1;
    }

    private string GetCurrentUserName()
    {
        // This should be implemented to get the current user name from the authentication context
        return User?.Identity?.Name ?? "System";
    }
}