using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Entities;

public class Priority : BaseEntity
{
    [Required]
    public int WorkRequestId { get; set; }
    
    [Required]
    public int DepartmentId { get; set; }
    
    [Required]
    public PriorityVote Vote { get; set; }
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal Weight { get; set; } = 1.0m; // Department weight for calculations
    
    [Required]
    public int VotedById { get; set; }
    
    public DateTime VotedDate { get; set; }
    
    [MaxLength(500)]
    public string Comments { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal BusinessValueScore { get; set; } = 0.5m; // 0-1 scale
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal StrategicAlignment { get; set; } = 0.5m; // 0-1 scale
    
    [MaxLength(1000)]
    public string ResourceImpactAssessment { get; set; } = string.Empty;
    
    // Navigation properties
    [ForeignKey("WorkRequestId")]
    public virtual WorkRequest WorkRequest { get; set; } = null!;
    
    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; } = null!;
    
    [ForeignKey("VotedById")]
    public virtual User VotedBy { get; set; } = null!;
}