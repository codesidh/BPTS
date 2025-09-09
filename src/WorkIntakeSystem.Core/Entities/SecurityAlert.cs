using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class SecurityAlert
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string AlertType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";
    
    [MaxLength(20)]
    public string Status { get; set; } = "Active";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? AcknowledgedAt { get; set; }
    
    [MaxLength(100)]
    public string AcknowledgedBy { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string Metadata { get; set; } = "{}";
    
    public DateTime? ExpiresAt { get; set; }
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string ActionRequired { get; set; } = string.Empty;
    
    public bool IsEscalated { get; set; }
    
    public DateTime? EscalatedAt { get; set; }
    
    [MaxLength(100)]
    public string EscalatedTo { get; set; } = string.Empty;
}
