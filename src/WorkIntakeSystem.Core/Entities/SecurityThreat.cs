using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class SecurityThreat
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ThreatType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";
    
    [MaxLength(20)]
    public string Status { get; set; } = "Active";
    
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(45)]
    public string SourceIP { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string TargetResource { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string MitigationSteps { get; set; } = string.Empty;
    
    public bool IsResolved { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(100)]
    public string ResolvedBy { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string ResolutionNotes { get; set; } = string.Empty;
}
