using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class WorkCategoryConfiguration : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int BusinessVerticalId { get; set; }
    
    public int? WorkflowTemplateId { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Column(TypeName = "nvarchar(max)")]
    public string RequiredFields { get; set; } = "{}"; // JSON field for dynamic form fields
    
    [Column(TypeName = "nvarchar(max)")]
    public string ApprovalMatrix { get; set; } = "{}"; // JSON field for approval workflow
    
    public int? SLAHours { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string ValidationRules { get; set; } = "{}"; // JSON field for validation rules
    
    [Column(TypeName = "nvarchar(max)")]
    public string NotificationTemplates { get; set; } = "{}"; // JSON field for notification templates
    
    public int DisplayOrder { get; set; } = 0;
    
    [Column(TypeName = "nvarchar(max)")]
    public string CustomFields { get; set; } = "{}"; // JSON field for category-specific fields
    
    // Navigation properties
    [ForeignKey("BusinessVerticalId")]
    public virtual BusinessVertical BusinessVertical { get; set; } = null!;
} 