using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class EventSnapshot : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string AggregateId { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string SnapshotData { get; set; } = string.Empty; // JSON field for aggregate state
    
    [Required]
    public int Version { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    [Required]
    public int CreatedById { get; set; }
    
    [MaxLength(100)]
    public string? SnapshotType { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; } // JSON field for additional metadata
    
    // Navigation properties
    [ForeignKey("CreatedById")]
    public virtual User CreatedBy { get; set; } = null!;
} 