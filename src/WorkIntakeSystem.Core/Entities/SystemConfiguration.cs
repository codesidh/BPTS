using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class SystemConfiguration : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string ConfigurationKey { get; set; } = string.Empty;
    
    [Required]
    public string ConfigurationValue { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string DataType { get; set; } = "String";
    
    public int? BusinessVerticalId { get; set; } // Nullable for global settings
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsEditable { get; set; } = true;
    
    public int Version { get; set; } = 1;
    
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpirationDate { get; set; }
    
    [MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;
    
    public int? PreviousVersionId { get; set; }
    
    [MaxLength(100)]
    public string ApprovedBy { get; set; } = string.Empty;
    
    public DateTime? ApprovalDate { get; set; }
    
    // Navigation properties
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical? BusinessVertical { get; set; }
    
    [ForeignKey("PreviousVersionId")]
    public virtual SystemConfiguration? PreviousVersion { get; set; }
    
    public virtual ICollection<SystemConfiguration> NextVersions { get; set; } = new List<SystemConfiguration>();
}