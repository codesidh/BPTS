using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class WorkflowStageConfiguration : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int Order { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public int? BusinessVerticalId { get; set; } // null for global stages
    
    [Column(TypeName = "nvarchar(max)")]
    public string RequiredRoles { get; set; } = "[]"; // JSON array of required roles
    
    [Required]
    public bool ApprovalRequired { get; set; } = false;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [MaxLength(50)]
    public string StageType { get; set; } = "Standard"; // Standard, Approval, Notification, Auto
    
    public int? SLAHours { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string NotificationTemplate { get; set; } = "{}"; // JSON template
    
    [Required]
    public bool AutoTransition { get; set; } = false;
    
    [Column(TypeName = "nvarchar(max)")]
    public string AllowedTransitions { get; set; } = "[]"; // JSON array of allowed next stages
    
    [Column(TypeName = "nvarchar(max)")]
    public string ValidationRules { get; set; } = "{}"; // JSON validation rules
    
    public int Version { get; set; } = 1;
    
    public DateTime? EffectiveDate { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string ChangeHistory { get; set; } = "[]"; // JSON array of changes
    
    // Navigation properties
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical? BusinessVertical { get; set; }
    
    public virtual ICollection<WorkflowTransition> FromTransitions { get; set; } = new List<WorkflowTransition>();
    public virtual ICollection<WorkflowTransition> ToTransitions { get; set; } = new List<WorkflowTransition>();
} 