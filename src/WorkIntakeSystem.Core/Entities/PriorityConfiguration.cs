using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class PriorityConfiguration : BaseEntity
{
    [Required]
    public int BusinessVerticalId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PriorityName { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(3,2)")]
    public decimal MinScore { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(3,2)")]
    public decimal MaxScore { get; set; }
    
    [MaxLength(20)]
    public string ColorCode { get; set; } = "#000000";
    
    [MaxLength(50)]
    public string IconClass { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Column(TypeName = "nvarchar(max)")]
    public string EscalationRules { get; set; } = "{}"; // JSON escalation rules
    
    [Column(TypeName = "nvarchar(max)")]
    public string TimeDecayConfiguration { get; set; } = "{}"; // JSON time decay settings
    
    [Column(TypeName = "nvarchar(max)")]
    public string BusinessValueWeights { get; set; } = "{}"; // JSON business value weights
    
    [Column(TypeName = "nvarchar(max)")]
    public string CapacityFactors { get; set; } = "{}"; // JSON capacity adjustment factors
    
    [Column(TypeName = "nvarchar(max)")]
    public string AutoAdjustmentRules { get; set; } = "{}"; // JSON auto-adjustment rules
    
    public int? SLAHours { get; set; }
    
    public int? EscalationThresholdHours { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string NotificationSettings { get; set; } = "{}"; // JSON notification settings
    
    // Navigation properties
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical BusinessVertical { get; set; } = null!;
} 