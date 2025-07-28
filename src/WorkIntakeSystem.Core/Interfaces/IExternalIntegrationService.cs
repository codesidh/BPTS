using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IExternalIntegrationService
    {
        Task<bool> SyncWithExternalSystemAsync(string systemName, string endpoint, object data);
        Task<List<ExternalSystemStatus>> GetExternalSystemStatusAsync();
        Task<IntegrationLog> LogIntegrationEventAsync(string systemName, string eventType, object data, bool success, string? errorMessage = null);
        Task<List<IntegrationLog>> GetIntegrationLogsAsync(string? systemName = null, DateTime? fromDate = null, DateTime? toDate = null);
    }

    public interface IProjectManagementIntegration
    {
        Task<bool> CreateProjectAsync(WorkRequest workRequest);
        Task<bool> UpdateProjectAsync(int workRequestId, object projectData);
        Task<bool> SyncProjectStatusAsync(int workRequestId);
        Task<List<ProjectTask>> GetProjectTasksAsync(int workRequestId);
    }

    public interface ICalendarIntegration
    {
        Task<bool> CreateCalendarEventAsync(WorkRequest workRequest, DateTime startDate, DateTime endDate);
        Task<bool> UpdateCalendarEventAsync(int workRequestId, DateTime startDate, DateTime endDate);
        Task<bool> DeleteCalendarEventAsync(int workRequestId);
        Task<List<CalendarEvent>> GetCalendarEventsAsync(DateTime fromDate, DateTime toDate);
    }

    public interface INotificationIntegration
    {
        Task<bool> SendNotificationAsync(string recipient, string subject, string message, NotificationType type);
        Task<bool> SendBulkNotificationAsync(List<string> recipients, string subject, string message, NotificationType type);
        Task<NotificationStatus> GetNotificationStatusAsync(string notificationId);
    }

    public class ExternalSystemStatus
    {
        public string SystemName { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public DateTime LastSyncTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    public class IntegrationLog
    {
        public int Id { get; set; }
        public string SystemName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class ProjectTask
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public int EstimatedHours { get; set; }
        public int ActualHours { get; set; }
    }

    public class CalendarEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public List<string> Attendees { get; set; } = new();
        public bool IsAllDay { get; set; }
    }

    public class NotificationStatus
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SentTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public DateTime? ReadTime { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public enum NotificationType
    {
        Email,
        SMS,
        Teams,
        Slack,
        Push
    }

    public class ExternalSystemConfiguration
    {
        public string SystemName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public Dictionary<string, string> Endpoints { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();
    }
} 