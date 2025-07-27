using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class ExternalIntegrationService : IExternalIntegrationService
    {
        private readonly WorkIntakeDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfigurationService _configurationService;

        public ExternalIntegrationService(WorkIntakeDbContext context, HttpClient httpClient, IConfigurationService configurationService)
        {
            _context = context;
            _httpClient = httpClient;
            _configurationService = configurationService;
        }

        public async Task<bool> SyncWithExternalSystemAsync(string systemName, string endpoint, object data)
        {
            var startTime = DateTime.UtcNow;
            var success = false;
            string? errorMessage = null;

            try
            {
                var config = await GetExternalSystemConfigurationAsync(systemName);
                if (config == null || !config.IsEnabled)
                {
                    throw new InvalidOperationException($"External system {systemName} is not configured or disabled");
                }

                var url = $"{config.BaseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.Secret}");

                var response = await _httpClient.PostAsync(url, content);
                success = response.IsSuccessStatusCode;

                if (!success)
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                success = false;
            }
            finally
            {
                var duration = DateTime.UtcNow - startTime;
                await LogIntegrationEventAsync(systemName, "sync", data, success, errorMessage);
            }

            return success;
        }

        public async Task<List<ExternalSystemStatus>> GetExternalSystemStatusAsync()
        {
            var statuses = new List<ExternalSystemStatus>();
            var systems = await GetConfiguredExternalSystemsAsync();

            foreach (var system in systems)
            {
                var status = new ExternalSystemStatus
                {
                    SystemName = system.SystemName,
                    IsConnected = false,
                    LastSyncTime = DateTime.UtcNow.AddHours(-1), // Placeholder
                    Status = "Unknown"
                };

                try
                {
                    // Test connection to external system
                    var testResult = await TestExternalSystemConnectionAsync(system);
                    status.IsConnected = testResult;
                    status.Status = testResult ? "Connected" : "Disconnected";
                }
                catch (Exception ex)
                {
                    status.Status = "Error";
                    status.ErrorMessage = ex.Message;
                }

                statuses.Add(status);
            }

            return statuses;
        }

        public async Task<IntegrationLog> LogIntegrationEventAsync(string systemName, string eventType, object data, bool success, string? errorMessage = null)
        {
            var log = new IntegrationLog
            {
                SystemName = systemName,
                EventType = eventType,
                Data = JsonSerializer.Serialize(data),
                Success = success,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow,
                Duration = TimeSpan.Zero // Would be calculated from start/end times
            };

            // In a real implementation, this would be saved to a database table
            // For now, we'll just return the log object
            return log;
        }

        public async Task<List<IntegrationLog>> GetIntegrationLogsAsync(string? systemName = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // This would query the integration logs table
            // For now, return empty list
            return new List<IntegrationLog>();
        }

        private async Task<ExternalSystemConfiguration?> GetExternalSystemConfigurationAsync(string systemName)
        {
            // This would retrieve configuration from database or configuration service
            // For now, return a placeholder configuration
            return new ExternalSystemConfiguration
            {
                SystemName = systemName,
                BaseUrl = await _configurationService.GetValueAsync($"ExternalSystems:{systemName}:BaseUrl") ?? "https://api.example.com",
                ApiKey = await _configurationService.GetValueAsync($"ExternalSystems:{systemName}:ApiKey") ?? "",
                Secret = await _configurationService.GetValueAsync($"ExternalSystems:{systemName}:Secret") ?? "",
                IsEnabled = await GetIsEnabledAsync(systemName)
            };
        }

        private async Task<List<ExternalSystemConfiguration>> GetConfiguredExternalSystemsAsync()
        {
            // This would retrieve all configured external systems
            // For now, return placeholder systems
            return new List<ExternalSystemConfiguration>
            {
                new ExternalSystemConfiguration { SystemName = "AzureDevOps", IsEnabled = true },
                new ExternalSystemConfiguration { SystemName = "Jira", IsEnabled = true },
                new ExternalSystemConfiguration { SystemName = "Teams", IsEnabled = true },
                new ExternalSystemConfiguration { SystemName = "Slack", IsEnabled = false }
            };
        }

        private async Task<bool> GetIsEnabledAsync(string systemName)
        {
            try
            {
                var value = await _configurationService.GetValueAsync($"ExternalSystems:{systemName}:IsEnabled");
                if (value == null) return false;
                
                if (bool.TryParse(value, out bool result))
                    return result;
                    
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TestExternalSystemConnectionAsync(ExternalSystemConfiguration config)
        {
            try
            {
                var url = $"{config.BaseUrl.TrimEnd('/')}/health";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);

                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ProjectManagementIntegration : IProjectManagementIntegration
    {
        private readonly IExternalIntegrationService _integrationService;
        private readonly WorkIntakeDbContext _context;

        public ProjectManagementIntegration(IExternalIntegrationService integrationService, WorkIntakeDbContext context)
        {
            _integrationService = integrationService;
            _context = context;
        }

        public async Task<bool> CreateProjectAsync(WorkRequest workRequest)
        {
            var projectData = new
            {
                name = workRequest.Title,
                description = workRequest.Description,
                category = workRequest.Category.ToString(),
                priority = workRequest.PriorityLevel.ToString(),
                estimatedEffort = workRequest.EstimatedEffort,
                targetDate = workRequest.TargetDate,
                department = workRequest.Department?.Name,
                businessVertical = workRequest.BusinessVertical?.Name
            };

            return await _integrationService.SyncWithExternalSystemAsync("AzureDevOps", "projects", projectData);
        }

        public async Task<bool> UpdateProjectAsync(int workRequestId, object projectData)
        {
            return await _integrationService.SyncWithExternalSystemAsync("AzureDevOps", $"projects/{workRequestId}", projectData);
        }

        public async Task<bool> SyncProjectStatusAsync(int workRequestId)
        {
            var workRequest = await _context.WorkRequests.FindAsync(workRequestId);
            if (workRequest == null) return false;

            var statusData = new
            {
                status = workRequest.Status.ToString(),
                currentStage = workRequest.CurrentStage.ToString(),
                actualDate = workRequest.ActualDate,
                actualEffort = workRequest.ActualEffort
            };

            return await _integrationService.SyncWithExternalSystemAsync("AzureDevOps", $"projects/{workRequestId}/status", statusData);
        }

        public async Task<List<ProjectTask>> GetProjectTasksAsync(int workRequestId)
        {
            // This would fetch tasks from the external project management system
            // For now, return placeholder data
            return new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = "task-1",
                    Title = "Requirements Analysis",
                    Description = "Analyze and document requirements",
                    Status = "In Progress",
                    DueDate = DateTime.UtcNow.AddDays(7),
                    AssignedTo = "John Doe",
                    EstimatedHours = 16,
                    ActualHours = 8
                },
                new ProjectTask
                {
                    Id = "task-2",
                    Title = "Design Implementation",
                    Description = "Implement system design",
                    Status = "Not Started",
                    DueDate = DateTime.UtcNow.AddDays(14),
                    AssignedTo = "Jane Smith",
                    EstimatedHours = 24,
                    ActualHours = 0
                }
            };
        }
    }

    public class CalendarIntegration : ICalendarIntegration
    {
        private readonly IExternalIntegrationService _integrationService;

        public CalendarIntegration(IExternalIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        public async Task<bool> CreateCalendarEventAsync(WorkRequest workRequest, DateTime startDate, DateTime endDate)
        {
            var eventData = new
            {
                subject = $"Work Request: {workRequest.Title}",
                body = new
                {
                    contentType = "HTML",
                    content = $"<p><strong>Description:</strong> {workRequest.Description}</p><p><strong>Category:</strong> {workRequest.Category}</p><p><strong>Priority:</strong> {workRequest.PriorityLevel}</p>"
                },
                start = new { dateTime = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), timeZone = "UTC" },
                end = new { dateTime = endDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), timeZone = "UTC" },
                attendees = new[]
                {
                    new { emailAddress = new { address = "team@company.com" }, type = "required" }
                }
            };

            return await _integrationService.SyncWithExternalSystemAsync("Teams", "calendar/events", eventData);
        }

        public async Task<bool> UpdateCalendarEventAsync(int workRequestId, DateTime startDate, DateTime endDate)
        {
            var eventData = new
            {
                start = new { dateTime = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), timeZone = "UTC" },
                end = new { dateTime = endDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), timeZone = "UTC" }
            };

            return await _integrationService.SyncWithExternalSystemAsync("Teams", $"calendar/events/{workRequestId}", eventData);
        }

        public async Task<bool> DeleteCalendarEventAsync(int workRequestId)
        {
            return await _integrationService.SyncWithExternalSystemAsync("Teams", $"calendar/events/{workRequestId}/delete", new { });
        }

        public async Task<List<CalendarEvent>> GetCalendarEventsAsync(DateTime fromDate, DateTime toDate)
        {
            // This would fetch calendar events from the external system
            // For now, return placeholder data
            return new List<CalendarEvent>
            {
                new CalendarEvent
                {
                    Id = "event-1",
                    Title = "Work Request Review Meeting",
                    Description = "Review of pending work requests",
                    StartDate = fromDate.AddDays(1).AddHours(10),
                    EndDate = fromDate.AddDays(1).AddHours(11),
                    Location = "Conference Room A",
                    Attendees = new List<string> { "john.doe@company.com", "jane.smith@company.com" },
                    IsAllDay = false
                }
            };
        }
    }

    public class NotificationIntegration : INotificationIntegration
    {
        private readonly IExternalIntegrationService _integrationService;

        public NotificationIntegration(IExternalIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        public async Task<bool> SendNotificationAsync(string recipient, string subject, string message, NotificationType type)
        {
            var notificationData = new
            {
                recipient = recipient,
                subject = subject,
                message = message,
                type = type.ToString(),
                timestamp = DateTime.UtcNow
            };

            var systemName = type switch
            {
                NotificationType.Email => "EmailService",
                NotificationType.Teams => "Teams",
                NotificationType.Slack => "Slack",
                NotificationType.SMS => "SMSService",
                NotificationType.Push => "PushNotificationService",
                _ => "EmailService"
            };

            return await _integrationService.SyncWithExternalSystemAsync(systemName, "notifications/send", notificationData);
        }

        public async Task<bool> SendBulkNotificationAsync(List<string> recipients, string subject, string message, NotificationType type)
        {
            var notificationData = new
            {
                recipients = recipients,
                subject = subject,
                message = message,
                type = type.ToString(),
                timestamp = DateTime.UtcNow
            };

            var systemName = type switch
            {
                NotificationType.Email => "EmailService",
                NotificationType.Teams => "Teams",
                NotificationType.Slack => "Slack",
                NotificationType.SMS => "SMSService",
                NotificationType.Push => "PushNotificationService",
                _ => "EmailService"
            };

            return await _integrationService.SyncWithExternalSystemAsync(systemName, "notifications/bulk-send", notificationData);
        }

        public async Task<NotificationStatus> GetNotificationStatusAsync(string notificationId)
        {
            // This would fetch notification status from the external system
            // For now, return placeholder data
            return new NotificationStatus
            {
                Id = notificationId,
                Status = "Delivered",
                SentTime = DateTime.UtcNow.AddMinutes(-5),
                DeliveredTime = DateTime.UtcNow.AddMinutes(-4),
                ReadTime = null
            };
        }
    }
} 