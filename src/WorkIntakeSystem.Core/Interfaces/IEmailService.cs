using System.Threading.Tasks;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName);
    Task<bool> SendWelcomeEmailAsync(string email, string userName);
    Task<bool> SendNotificationEmailAsync(string email, string subject, string message);
    Task<bool> SendBulkEmailAsync(List<string> emails, string subject, string message);
}

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string AppUrl { get; set; } = string.Empty;
} 