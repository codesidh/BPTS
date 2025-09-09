using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class ComplianceViolation
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int Framework { get; set; } // Enum as int
    
    [Required]
    [MaxLength(200)]
    public string Requirement { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";
    
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(200)]
    public string Resource { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    
    public bool IsResolved { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(100)]
    public string ResolvedBy { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string ResolutionNotes { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string Evidence { get; set; } = string.Empty;
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
