using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class BusinessCapability : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Owner { get; set; } = string.Empty;
    
    [Required]
    public int BusinessVerticalId { get; set; }
    
    [MaxLength(100)]
    public string TechnicalOwner { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string SubCategory { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    public string Configuration { get; set; } = "{}"; // JSON field for capability-specific settings
    
    public int Version { get; set; } = 1;
    
    public string DependencyMap { get; set; } = "{}"; // JSON field for capability dependencies
    
    public string ResourceRequirements { get; set; } = "{}"; // JSON field for resource planning
    
    // Navigation properties
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical BusinessVertical { get; set; } = null!;
    
    public virtual ICollection<WorkRequest> WorkRequests { get; set; } = new List<WorkRequest>();
    public virtual ICollection<CapabilityDepartmentMapping> DepartmentMappings { get; set; } = new List<CapabilityDepartmentMapping>();
}