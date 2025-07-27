using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class CapabilityDepartmentMapping : BaseEntity
{
    [Required]
    public int CapabilityId { get; set; }
    
    [Required]
    public int DepartmentId { get; set; }
    
    [MaxLength(20)]
    public string AccessLevel { get; set; } = "View";
    
    public bool CanCreate { get; set; } = false;
    
    public bool CanModify { get; set; } = false;
    
    public bool CanView { get; set; } = true;
    
    public bool CanApprove { get; set; } = false;
    
    public DateTime EffectiveDate { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    
    public int Version { get; set; } = 1;
    
    // Navigation properties
    [ForeignKey("CapabilityId")]
    public virtual BusinessCapability Capability { get; set; } = null!;
    
    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; } = null!;
}