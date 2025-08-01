using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class WorkflowTransition : BaseEntity
{
    [Required]
    public int FromStageId { get; set; }
    
    [Required]
    public int ToStageId { get; set; }
    
    public int? BusinessVerticalId { get; set; } // null for global transitions
    
    [Required]
    [MaxLength(100)]
    public string TransitionName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? RequiredRole { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Column(TypeName = "nvarchar(max)")]
    public string ConditionScript { get; set; } = "{}"; // JSON conditions for transition
    
    [Required]
    public bool NotificationRequired { get; set; } = false;
    
    [Column(TypeName = "nvarchar(max)")]
    public string NotificationTemplate { get; set; } = "{}"; // JSON notification template
    
    [MaxLength(100)]
    public string? EventSourceId { get; set; }
    
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    public int? AutoTransitionDelayMinutes { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string ValidationRules { get; set; } = "{}"; // JSON validation rules
    
    // Navigation properties
    [ForeignKey("FromStageId")]
    public virtual WorkflowStageConfiguration FromStage { get; set; } = null!;
    
    [ForeignKey("ToStageId")]
    public virtual WorkflowStageConfiguration ToStage { get; set; } = null!;
    
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical? BusinessVertical { get; set; }
} 