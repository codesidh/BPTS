using System.ComponentModel.DataAnnotations;

namespace WorkIntakeSystem.Core.Entities;

public class BusinessVertical : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public string Configuration { get; set; } = "{}"; // JSON field for vertical-specific settings
    
    public int Version { get; set; } = 1;
    
    public string ConfigurationHistory { get; set; } = "[]"; // JSON array of configuration changes
    
    // Navigation properties
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<WorkRequest> WorkRequests { get; set; } = new List<WorkRequest>();
    public virtual ICollection<BusinessCapability> BusinessCapabilities { get; set; } = new List<BusinessCapability>();
}