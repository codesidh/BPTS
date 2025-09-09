using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class SecurityIncident
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";
    
    [MaxLength(20)]
    public string Status { get; set; } = "Open";
    
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(100)]
    public string AssignedTo { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string ResolutionNotes { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Impact { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string AffectedResources { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string IncidentType { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Priority { get; set; } = "Medium";
    
    [Column(TypeName = "nvarchar(max)")]
    public string InvestigationNotes { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastUpdated { get; set; }
    
    [MaxLength(100)]
    public string LastUpdatedBy { get; set; } = string.Empty;
}
