using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class AuditTrail
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int WorkRequestId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    public string? OldValue { get; set; }
    
    public string? NewValue { get; set; }
    
    [Required]
    public int ChangedById { get; set; }
    
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string Comments { get; set; } = string.Empty;
    
    public int? EventId { get; set; }
    
    [MaxLength(50)]
    public string CorrelationId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string SessionId { get; set; } = string.Empty;
    
    [MaxLength(45)]
    public string IPAddress { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;
    
    public string SecurityContext { get; set; } = "{}"; // JSON field for security information
    
    // Navigation properties
    [ForeignKey("WorkRequestId")]
    public virtual WorkRequest WorkRequest { get; set; } = null!;
    
    [ForeignKey("ChangedById")]
    public virtual User ChangedBy { get; set; } = null!;
}