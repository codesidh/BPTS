using System.ComponentModel.DataAnnotations;

namespace WorkIntakeSystem.Core.Entities;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime ModifiedDate { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public string ModifiedBy { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}