using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class SecurityEvent
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(45)]
    public string IPAddress { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string SessionId { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Resource { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string Details { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";
    
    public bool IsSuspicious { get; set; }
    
    [MaxLength(50)]
    public string CorrelationId { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string Metadata { get; set; } = "{}";
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
