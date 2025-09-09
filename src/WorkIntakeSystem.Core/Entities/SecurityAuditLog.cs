using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkIntakeSystem.Core.Entities;

public class SecurityAuditLog
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Resource { get; set; } = string.Empty;
    
    [MaxLength(45)]
    public string IPAddress { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    public string Result { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string Details { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string SessionId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string CorrelationId { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";
    
    [Column(TypeName = "nvarchar(max)")]
    public string RequestData { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string ResponseData { get; set; } = string.Empty;
    
    public int? Duration { get; set; } // in milliseconds
    
    [MaxLength(100)]
    public string Endpoint { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string HttpMethod { get; set; } = string.Empty;
    
    public int? StatusCode { get; set; }
}
