using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.API.DTOs;

public class WorkRequestDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkCategory Category { get; set; }
    public int BusinessVerticalId { get; set; }
    public string BusinessVerticalName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int SubmitterId { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public WorkflowStage CurrentStage { get; set; }
    public WorkStatus Status { get; set; }
    public decimal Priority { get; set; }
    public PriorityLevel PriorityLevel { get; set; }
    public int? CapabilityId { get; set; }
    public string? CapabilityName { get; set; }
    public int EstimatedEffort { get; set; }
    public int ActualEffort { get; set; }
    public decimal BusinessValue { get; set; }
    public decimal TimeDecayFactor { get; set; }
    public decimal CapacityAdjustment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
    public List<PriorityVoteDto> PriorityVotes { get; set; } = new();
}

public class CreateWorkRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkCategory Category { get; set; }
    public int BusinessVerticalId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime? TargetDate { get; set; }
    public int? CapabilityId { get; set; }
    public int EstimatedEffort { get; set; }
    public decimal BusinessValue { get; set; } = 0.5m;
}

public class UpdateWorkRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkCategory Category { get; set; }
    public DateTime? TargetDate { get; set; }
    public WorkflowStage CurrentStage { get; set; }
    public WorkStatus Status { get; set; }
    public int? CapabilityId { get; set; }
    public int EstimatedEffort { get; set; }
    public int ActualEffort { get; set; }
    public decimal BusinessValue { get; set; }
}

public class PriorityVoteDto
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public PriorityVote Vote { get; set; }
    public decimal Weight { get; set; }
    public int VotedById { get; set; }
    public string VotedByName { get; set; } = string.Empty;
    public DateTime VotedDate { get; set; }
    public string Comments { get; set; } = string.Empty;
    public decimal BusinessValueScore { get; set; }
    public decimal StrategicAlignment { get; set; }
    public string ResourceImpactAssessment { get; set; } = string.Empty;
}

public class CreatePriorityVoteDto
{
    public int WorkRequestId { get; set; }
    public int DepartmentId { get; set; }
    public PriorityVote Vote { get; set; }
    public string Comments { get; set; } = string.Empty;
    public decimal BusinessValueScore { get; set; } = 0.5m;
    public decimal StrategicAlignment { get; set; } = 0.5m;
    public string ResourceImpactAssessment { get; set; } = string.Empty;
}

public class ValidateTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
}