using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class ConfigurationChangeRequest : BaseEntity
{
    [Required]
    public int ConfigurationId { get; set; }
    
    [Required]
    public string RequestedValue { get; set; } = string.Empty;
    
    [Required]
    public string ChangeReason { get; set; } = string.Empty;
    
    [Required]
    public int RequestedById { get; set; }
    
    [Required]
    public DateTime RequestedDate { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Implemented, RolledBack
    
    public int? ApprovedById { get; set; }
    
    public DateTime? ApprovedDate { get; set; }
    
    public string? RejectedReason { get; set; }
    
    public DateTime? ImplementedDate { get; set; }
    
    public DateTime? RollbackDate { get; set; }
    
    public string? ImplementationNotes { get; set; }
    
    // Navigation properties
    [ForeignKey("ConfigurationId")]
    public virtual SystemConfiguration Configuration { get; set; } = null!;
    
    [ForeignKey("RequestedById")]
    public virtual User RequestedBy { get; set; } = null!;
    
    [ForeignKey("ApprovedById")]
    public virtual User? ApprovedBy { get; set; }
} 