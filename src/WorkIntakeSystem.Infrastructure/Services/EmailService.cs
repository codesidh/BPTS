using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _emailSettings = new EmailSettings
        {
            SmtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
            SmtpPort = configuration.GetValue<int>("EmailSettings:SmtpPort", 587),
            SmtpUsername = configuration["EmailSettings:SmtpUsername"] ?? "",
            SmtpPassword = configuration["EmailSettings:SmtpPassword"] ?? "",
            EnableSsl = configuration.GetValue<bool>("EmailSettings:EnableSsl", true),
            FromEmail = configuration["EmailSettings:FromEmail"] ?? "noreply@workintakesystem.com",
            FromName = configuration["EmailSettings:FromName"] ?? "Work Intake System",
            AppUrl = configuration["EmailSettings:AppUrl"] ?? "https://localhost:3000"
        };
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
    {
        try
        {
            var subject = "Password Reset Request - Work Intake System";
            var resetUrl = $"{_emailSettings.AppUrl}/reset-password?token={resetToken}";
            
            var message = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {userName},</p>
                    <p>You have requested to reset your password for the Work Intake System.</p>
                    <p>Please click the link below to reset your password:</p>
                    <p><a href='{resetUrl}'>Reset Password</a></p>
                    <p>If you didn't request this password reset, please ignore this email.</p>
                    <p>This link will expire in 1 hour.</p>
                    <br>
                    <p>Best regards,<br>Work Intake System Team</p>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, message, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var subject = "Welcome to Work Intake System";
            var message = $@"
                <html>
                <body>
                    <h2>Welcome to Work Intake System!</h2>
                    <p>Hello {userName},</p>
                    <p>Your account has been successfully created. You can now log in to the system.</p>
                    <p>If you have any questions, please contact your system administrator.</p>
                    <br>
                    <p>Best regards,<br>Work Intake System Team</p>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, message, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendNotificationEmailAsync(string email, string subject, string message)
    {
        try
        {
            return await SendEmailAsync(email, subject, message, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(List<string> emails, string subject, string message)
    {
        try
        {
            var tasks = emails.Select(email => SendEmailAsync(email, subject, message, false));
            var results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send bulk email to {Count} recipients", emails.Count);
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string email, string subject, string message, bool isHtml)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            return false;
        }
    }
} 