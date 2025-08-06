using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    [MaxLength(255)]
    public string? PasswordSalt { get; set; }

    [Required]
    public int DepartmentId { get; set; }
    
    [Required]
    public int BusinessVerticalId { get; set; }
    
    public UserRole Role { get; set; } = UserRole.EndUser;
    
    public string SkillSet { get; set; } = "{}"; // JSON field for user skills
    
    public int Capacity { get; set; } = 40; // Hours per week
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal CurrentWorkload { get; set; } = 0.0m; // Current utilization percentage
    
    // Navigation properties
    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; } = null!;
    
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical BusinessVertical { get; set; } = null!;
    
    public virtual ICollection<WorkRequest> SubmittedRequests { get; set; } = new List<WorkRequest>();
    public virtual ICollection<Priority> PriorityVotes { get; set; } = new List<Priority>();
    public virtual ICollection<AuditTrail> AuditTrails { get; set; } = new List<AuditTrail>();
    
    // Windows Authentication fields
    [MaxLength(255)]
    public string? WindowsIdentity { get; set; }
    
    [MaxLength(255)]
    public string? ActiveDirectorySid { get; set; }
    
    public bool IsWindowsAuthenticated { get; set; } = false;
    
    public DateTime? LastAdSync { get; set; }
    
    public string? AdGroups { get; set; } // JSON field for AD groups
}