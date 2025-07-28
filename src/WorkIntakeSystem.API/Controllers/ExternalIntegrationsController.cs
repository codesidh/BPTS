using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List<string>

namespace WorkIntakeSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SystemAdministrator,BusinessExecutive")]
    public class ExternalIntegrationsController : ControllerBase
    {
        private readonly IExternalIntegrationService _integrationService;
        private readonly IProjectManagementIntegration _projectManagementIntegration;
        private readonly ICalendarIntegration _calendarIntegration;
        private readonly INotificationIntegration _notificationIntegration;

        public ExternalIntegrationsController(
            IExternalIntegrationService integrationService,
            IProjectManagementIntegration projectManagementIntegration,
            ICalendarIntegration calendarIntegration,
            INotificationIntegration notificationIntegration)
        {
            _integrationService = integrationService;
            _projectManagementIntegration = projectManagementIntegration;
            _calendarIntegration = calendarIntegration;
            _notificationIntegration = notificationIntegration;
        }

        /// <summary>
        /// Get status of all external system integrations
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetExternalSystemStatus()
        {
            try
            {
                var statuses = await _integrationService.GetExternalSystemStatusAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve external system status: {ex.Message}");
            }
        }

        /// <summary>
        /// Get integration logs
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetIntegrationLogs(
            [FromQuery] string? systemName = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var logs = await _integrationService.GetIntegrationLogsAsync(systemName, fromDate, toDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve integration logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Sync with external system
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> SyncWithExternalSystem(
            [FromBody] SyncRequest request)
        {
            try
            {
                var success = await _integrationService.SyncWithExternalSystemAsync(
                    request.SystemName, 
                    request.Endpoint, 
                    request.Data);

                return Ok(new { success, message = success ? "Sync completed successfully" : "Sync failed" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to sync with external system: {ex.Message}");
            }
        }

        /// <summary>
        /// Create project in external project management system
        /// </summary>
        [HttpPost("projects")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                // This would typically fetch the work request from the database
                // For now, create a placeholder work request
                var workRequest = new WorkIntakeSystem.Core.Entities.WorkRequest
                {
                    Id = request.WorkRequestId,
                    Title = request.Title,
                    Description = request.Description,
                    Category = request.Category,
                    PriorityLevel = request.PriorityLevel,
                    EstimatedEffort = request.EstimatedEffort,
                    TargetDate = request.TargetDate
                };

                var success = await _projectManagementIntegration.CreateProjectAsync(workRequest);
                return Ok(new { success, message = success ? "Project created successfully" : "Failed to create project" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create project: {ex.Message}");
            }
        }

        /// <summary>
        /// Get project tasks from external system
        /// </summary>
        [HttpGet("projects/{workRequestId}/tasks")]
        public async Task<IActionResult> GetProjectTasks(int workRequestId)
        {
            try
            {
                var tasks = await _projectManagementIntegration.GetProjectTasksAsync(workRequestId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve project tasks: {ex.Message}");
            }
        }

        /// <summary>
        /// Create calendar event
        /// </summary>
        [HttpPost("calendar/events")]
        public async Task<IActionResult> CreateCalendarEvent([FromBody] CreateCalendarEventRequest request)
        {
            try
            {
                var workRequest = new WorkIntakeSystem.Core.Entities.WorkRequest
                {
                    Id = request.WorkRequestId,
                    Title = request.Title,
                    Description = request.Description
                };

                var success = await _calendarIntegration.CreateCalendarEventAsync(
                    workRequest, 
                    request.StartDate, 
                    request.EndDate);

                return Ok(new { success, message = success ? "Calendar event created successfully" : "Failed to create calendar event" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create calendar event: {ex.Message}");
            }
        }

        /// <summary>
        /// Get calendar events
        /// </summary>
        [HttpGet("calendar/events")]
        public async Task<IActionResult> GetCalendarEvents(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var events = await _calendarIntegration.GetCalendarEventsAsync(fromDate, toDate);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve calendar events: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification
        /// </summary>
        [HttpPost("notifications")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                var success = await _notificationIntegration.SendNotificationAsync(
                    request.Recipient,
                    request.Subject,
                    request.Message,
                    request.Type);

                return Ok(new { success, message = success ? "Notification sent successfully" : "Failed to send notification" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send bulk notification
        /// </summary>
        [HttpPost("notifications/bulk")]
        public async Task<IActionResult> SendBulkNotification([FromBody] SendBulkNotificationRequest request)
        {
            try
            {
                var success = await _notificationIntegration.SendBulkNotificationAsync(
                    request.Recipients,
                    request.Subject,
                    request.Message,
                    request.Type);

                return Ok(new { success, message = success ? "Bulk notification sent successfully" : "Failed to send bulk notification" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send bulk notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Get notification status
        /// </summary>
        [HttpGet("notifications/{notificationId}/status")]
        public async Task<IActionResult> GetNotificationStatus(string notificationId)
        {
            try
            {
                var status = await _notificationIntegration.GetNotificationStatusAsync(notificationId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve notification status: {ex.Message}");
            }
        }
    }

    public class SyncRequest
    {
        public string SystemName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public object Data { get; set; } = new();
    }

    public class CreateProjectRequest
    {
        public int WorkRequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public WorkIntakeSystem.Core.Enums.WorkCategory Category { get; set; }
        public WorkIntakeSystem.Core.Enums.PriorityLevel PriorityLevel { get; set; }
        public int EstimatedEffort { get; set; }
        public DateTime? TargetDate { get; set; }
    }

    public class CreateCalendarEventRequest
    {
        public int WorkRequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SendNotificationRequest
    {
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public WorkIntakeSystem.Core.Interfaces.NotificationType Type { get; set; }
    }

    public class SendBulkNotificationRequest
    {
        public List<string> Recipients { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public WorkIntakeSystem.Core.Interfaces.NotificationType Type { get; set; }
    }
} 