using System.ComponentModel.DataAnnotations;

namespace WorkIntakeSystem.Core.Entities;

public class EventStore
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string AggregateId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    public string EventData { get; set; } = string.Empty; // JSON data
    
    public int EventVersion { get; set; } = 1;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string CorrelationId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string CausationId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public string Metadata { get; set; } = "{}"; // JSON metadata
    
    // Navigation properties
    public virtual WorkRequest? WorkRequest { get; set; }
}