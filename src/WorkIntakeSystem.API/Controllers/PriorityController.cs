using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Services;
using System.Security.Claims;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PriorityController : ControllerBase
{
    private readonly IPriorityRepository _priorityRepository;
    private readonly IWorkRequestRepository _workRequestRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPriorityCalculationService _priorityCalculationService;
    private readonly IMapper _mapper;
    private readonly ILogger<PriorityController> _logger;

    public PriorityController(
        IPriorityRepository priorityRepository,
        IWorkRequestRepository workRequestRepository,
        IDepartmentRepository departmentRepository,
        IPriorityCalculationService priorityCalculationService,
        IMapper mapper,
        ILogger<PriorityController> logger)
    {
        _priorityRepository = priorityRepository;
        _workRequestRepository = workRequestRepository;
        _departmentRepository = departmentRepository;
        _priorityCalculationService = priorityCalculationService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Submit a priority vote for a work request
    /// </summary>
    [HttpPost("vote")]
    public async Task<ActionResult<PriorityVoteResponse>> SubmitVote([FromBody] PriorityVoteRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            // Validate work request exists
            var workRequest = await _workRequestRepository.GetByIdAsync(request.WorkRequestId);
            if (workRequest == null)
            {
                return NotFound($"Work request with ID {request.WorkRequestId} not found");
            }

            // Get user's department
            var user = await GetCurrentUserAsync();
            if (user?.DepartmentId == null)
            {
                return BadRequest("User must be associated with a department to vote");
            }

            // Check if department has already voted
            var existingVote = await _priorityRepository.GetByWorkRequestAndDepartmentAsync(
                request.WorkRequestId, user.DepartmentId.Value);

            if (existingVote != null)
            {
                return BadRequest("Department has already voted on this work request");
            }

            // Create new priority vote
            var priority = new Priority
            {
                WorkRequestId = request.WorkRequestId,
                DepartmentId = user.DepartmentId.Value,
                VotedById = currentUserId,
                Vote = request.Vote,
                BusinessValueScore = request.BusinessValueScore,
                StrategicAlignment = request.StrategicAlignment,
                Comments = request.Comments,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                CreatedBy = currentUserName,
                ModifiedBy = currentUserName
            };

            await _priorityRepository.CreateAsync(priority);

            // Recalculate work request priority
            await _priorityCalculationService.UpdatePriorityAsync(request.WorkRequestId);

            _logger.LogInformation("Priority vote submitted for work request {WorkRequestId} by department {DepartmentId}", 
                request.WorkRequestId, user.DepartmentId);

            return Ok(new PriorityVoteResponse
            {
                Success = true,
                Message = "Vote submitted successfully",
                NewPriorityScore = workRequest.Priority
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting priority vote for work request {WorkRequestId}", request.WorkRequestId);
            return StatusCode(500, "An error occurred while submitting the vote");
        }
    }

    /// <summary>
    /// Update an existing priority vote
    /// </summary>
    [HttpPut("vote/{workRequestId}")]
    public async Task<ActionResult<PriorityVoteResponse>> UpdateVote(int workRequestId, [FromBody] PriorityVoteRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            var user = await GetCurrentUserAsync();
            if (user?.DepartmentId == null)
            {
                return BadRequest("User must be associated with a department to vote");
            }

            // Get existing vote
            var existingVote = await _priorityRepository.GetByWorkRequestAndDepartmentAsync(workRequestId, user.DepartmentId.Value);
            if (existingVote == null)
            {
                return NotFound("No vote found for this work request and department");
            }

            // Update vote
            existingVote.Vote = request.Vote;
            existingVote.BusinessValueScore = request.BusinessValueScore;
            existingVote.StrategicAlignment = request.StrategicAlignment;
            existingVote.Comments = request.Comments;
            existingVote.ModifiedDate = DateTime.UtcNow;
            existingVote.ModifiedBy = currentUserName;

            await _priorityRepository.UpdateAsync(existingVote);

            // Recalculate work request priority
            await _priorityCalculationService.UpdatePriorityAsync(workRequestId);

            var workRequest = await _workRequestRepository.GetByIdAsync(workRequestId);

            return Ok(new PriorityVoteResponse
            {
                Success = true,
                Message = "Vote updated successfully",
                NewPriorityScore = workRequest?.Priority ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating priority vote for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while updating the vote");
        }
    }

    /// <summary>
    /// Get voting status for a work request
    /// </summary>
    [HttpGet("status/{workRequestId}")]
    public async Task<ActionResult<PriorityVotingStatus>> GetVotingStatus(int workRequestId)
    {
        try
        {
            var workRequest = await _workRequestRepository.GetByIdAsync(workRequestId);
            if (workRequest == null)
            {
                return NotFound($"Work request with ID {workRequestId} not found");
            }

            var votes = await _priorityRepository.GetByWorkRequestIdAsync(workRequestId);
            var departments = await _departmentRepository.GetAllAsync();

            var votingStatus = new PriorityVotingStatus
            {
                WorkRequestId = workRequestId,
                WorkRequestTitle = workRequest.Title,
                TotalDepartments = departments.Count(),
                VotedDepartments = votes.Count(),
                PendingDepartments = departments.Count() - votes.Count(),
                CurrentPriorityScore = workRequest.Priority,
                CurrentPriorityLevel = workRequest.PriorityLevel,
                Votes = votes.Select(v => new VoteDetail
                {
                    DepartmentId = v.DepartmentId,
                    DepartmentName = departments.FirstOrDefault(d => d.Id == v.DepartmentId)?.Name ?? "Unknown",
                    Vote = v.Vote,
                    BusinessValueScore = v.BusinessValueScore,
                    StrategicAlignment = v.StrategicAlignment,
                    Comments = v.Comments,
                    VotedBy = v.VotedBy?.Name ?? "Unknown",
                    VotedDate = v.CreatedDate
                }).ToList()
            };

            return Ok(votingStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voting status for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, "An error occurred while retrieving voting status");
        }
    }

    /// <summary>
    /// Get pending votes for a department
    /// </summary>
    [HttpGet("pending/{departmentId}")]
    public async Task<ActionResult<IEnumerable<PendingVote>>> GetPendingVotes(int departmentId)
    {
        try
        {
            var pendingVotes = await _priorityRepository.GetPendingVotesForDepartmentAsync(departmentId);
            var workRequests = await _workRequestRepository.GetByIdsAsync(pendingVotes.Select(pv => pv.WorkRequestId));

            var result = pendingVotes.Select(pv =>
            {
                var workRequest = workRequests.FirstOrDefault(wr => wr.Id == pv.WorkRequestId);
                return new PendingVote
                {
                    WorkRequestId = pv.WorkRequestId,
                    Title = workRequest?.Title ?? "Unknown",
                    Category = workRequest?.Category ?? WorkCategory.Other,
                    PriorityLevel = workRequest?.PriorityLevel ?? PriorityLevel.Low,
                    CreatedDate = workRequest?.CreatedDate ?? DateTime.UtcNow,
                    SubmitterName = workRequest?.Submitter?.Name ?? "Unknown",
                    DaysSinceCreation = (DateTime.UtcNow - (workRequest?.CreatedDate ?? DateTime.UtcNow)).Days
                };
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending votes for department {DepartmentId}", departmentId);
            return StatusCode(500, "An error occurred while retrieving pending votes");
        }
    }

    /// <summary>
    /// Get department voting patterns
    /// </summary>
    [HttpGet("patterns/{departmentId}")]
    public async Task<ActionResult<DepartmentVotingPattern>> GetDepartmentVotingPattern(int departmentId)
    {
        try
        {
            var votes = await _priorityRepository.GetByDepartmentIdAsync(departmentId);
            var department = await _departmentRepository.GetByIdAsync(departmentId);

            if (department == null)
            {
                return NotFound($"Department with ID {departmentId} not found");
            }

            var pattern = new DepartmentVotingPattern
            {
                DepartmentId = departmentId,
                DepartmentName = department.Name,
                TotalVotes = votes.Count(),
                VoteDistribution = votes.GroupBy(v => v.Vote)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageBusinessValueScore = votes.Any() ? votes.Average(v => v.BusinessValueScore) : 0,
                AverageStrategicAlignment = votes.Any() ? votes.Average(v => v.StrategicAlignment) : 0,
                RecentVotes = votes.OrderByDescending(v => v.CreatedDate)
                    .Take(10)
                    .Select(v => new RecentVote
                    {
                        WorkRequestId = v.WorkRequestId,
                        Vote = v.Vote,
                        BusinessValueScore = v.BusinessValueScore,
                        StrategicAlignment = v.StrategicAlignment,
                        VotedDate = v.CreatedDate
                    }).ToList()
            };

            return Ok(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voting pattern for department {DepartmentId}", departmentId);
            return StatusCode(500, "An error occurred while retrieving voting pattern");
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        // This would typically come from a user repository
        // For now, return a mock user
        return new User
        {
            Id = userId,
            Name = GetCurrentUserName(),
            DepartmentId = 1 // Mock department ID
        };
    }
}

// DTOs
public class PriorityVoteRequest
{
    public int WorkRequestId { get; set; }
    public PriorityVote Vote { get; set; }
    public decimal BusinessValueScore { get; set; } = 0.5m;
    public decimal StrategicAlignment { get; set; } = 0.5m;
    public string? Comments { get; set; }
}

public class PriorityVoteResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal NewPriorityScore { get; set; }
}

public class PriorityVotingStatus
{
    public int WorkRequestId { get; set; }
    public string WorkRequestTitle { get; set; } = string.Empty;
    public int TotalDepartments { get; set; }
    public int VotedDepartments { get; set; }
    public int PendingDepartments { get; set; }
    public decimal CurrentPriorityScore { get; set; }
    public PriorityLevel CurrentPriorityLevel { get; set; }
    public List<VoteDetail> Votes { get; set; } = new();
}

public class VoteDetail
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public PriorityVote Vote { get; set; }
    public decimal BusinessValueScore { get; set; }
    public decimal StrategicAlignment { get; set; }
    public string? Comments { get; set; }
    public string VotedBy { get; set; } = string.Empty;
    public DateTime VotedDate { get; set; }
}

public class PendingVote
{
    public int WorkRequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public WorkCategory Category { get; set; }
    public PriorityLevel PriorityLevel { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public int DaysSinceCreation { get; set; }
}

public class DepartmentVotingPattern
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public Dictionary<PriorityVote, int> VoteDistribution { get; set; } = new();
    public decimal AverageBusinessValueScore { get; set; }
    public decimal AverageStrategicAlignment { get; set; }
    public List<RecentVote> RecentVotes { get; set; } = new();
}

public class RecentVote
{
    public int WorkRequestId { get; set; }
    public PriorityVote Vote { get; set; }
    public decimal BusinessValueScore { get; set; }
    public decimal StrategicAlignment { get; set; }
    public DateTime VotedDate { get; set; }
} 