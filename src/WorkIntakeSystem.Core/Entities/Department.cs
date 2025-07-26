using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int BusinessVerticalId { get; set; }
    
    public int DisplayOrder { get; set; }
    
    [MaxLength(10)]
    public string DepartmentCode { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal VotingWeight { get; set; } = 1.0m; // For priority calculations
    
    public int ResourceCapacity { get; set; } = 100;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal CurrentUtilization { get; set; } = 0.0m;
    
    public string SkillMatrix { get; set; } = "{}"; // JSON field for skill tracking
    
    // Navigation properties
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical BusinessVertical { get; set; } = null!;
    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Priority> Priorities { get; set; } = new List<Priority>();
    public virtual ICollection<CapabilityDepartmentMapping> CapabilityMappings { get; set; } = new List<CapabilityDepartmentMapping>();
}