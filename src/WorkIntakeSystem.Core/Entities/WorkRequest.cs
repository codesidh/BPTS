using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Entities;

public class WorkRequest : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public WorkCategory Category { get; set; }
    
    [Required]
    public int BusinessVerticalId { get; set; }
    
    [Required]
    public int DepartmentId { get; set; }
    
    [Required]
    public int SubmitterId { get; set; }
    
    public DateTime? TargetDate { get; set; }
    
    public DateTime? ActualDate { get; set; }
    
    public WorkflowStage CurrentStage { get; set; } = WorkflowStage.Intake;
    
    public WorkStatus Status { get; set; } = WorkStatus.Draft;
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal Priority { get; set; } = 0.0m; // Calculated priority score (0-1)
    
    public int? CapabilityId { get; set; }
    
    public int EstimatedEffort { get; set; } = 0; // Hours
    
    public int ActualEffort { get; set; } = 0; // Hours
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal BusinessValue { get; set; } = 0.5m; // 0-1 scale
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal TimeDecayFactor { get; set; } = 1.0m; // Time-based priority adjustment
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal CapacityAdjustment { get; set; } = 1.0m; // Resource availability factor
    
    public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.Low;
    
    // Navigation properties
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical BusinessVertical { get; set; } = null!;
    
    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; } = null!;
    
    [ForeignKey("SubmitterId")]
    public virtual User Submitter { get; set; } = null!;
    
    [ForeignKey("CapabilityId")]
    public virtual BusinessCapability? Capability { get; set; }
    
    public virtual ICollection<Priority> PriorityVotes { get; set; } = new List<Priority>();
    public virtual ICollection<AuditTrail> AuditTrails { get; set; } = new List<AuditTrail>();
    public virtual ICollection<EventStore> Events { get; set; } = new List<EventStore>();
}