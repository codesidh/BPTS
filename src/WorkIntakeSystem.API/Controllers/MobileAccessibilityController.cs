using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MobileAccessibilityController : ControllerBase
{
    private readonly IMobileAccessibilityService _mobileAccessibilityService;
    private readonly ILogger<MobileAccessibilityController> _logger;

    public MobileAccessibilityController(
        IMobileAccessibilityService mobileAccessibilityService,
        ILogger<MobileAccessibilityController> logger)
    {
        _mobileAccessibilityService = mobileAccessibilityService;
        _logger = logger;
    }

    // PWA Support
    [HttpGet("pwa/manifest")]
    public async Task<IActionResult> GetPWAManifest()
    {
        try
        {
            var manifest = await _mobileAccessibilityService.GetPWAManifestAsync();
            return Ok(manifest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PWA manifest");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("pwa/service-worker-config")]
    public async Task<IActionResult> GetServiceWorkerConfig()
    {
        try
        {
            var config = await _mobileAccessibilityService.GetServiceWorkerConfigAsync();
            return Ok(new { Success = true, Config = config });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service worker config");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("pwa/offline-resources")]
    public async Task<IActionResult> GetOfflineResources()
    {
        try
        {
            var resources = await _mobileAccessibilityService.GetOfflineResourcesAsync();
            return Ok(new { Success = true, Resources = resources });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting offline resources");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Offline Capabilities
    [HttpPost("offline/sync")]
    [Authorize]
    public async Task<IActionResult> SyncOfflineData()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _mobileAccessibilityService.SyncOfflineDataAsync(userId);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Offline data synced successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to sync offline data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing offline data");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("offline/work-requests")]
    [Authorize]
    public async Task<IActionResult> GetOfflineWorkRequests()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var workRequests = await _mobileAccessibilityService.GetOfflineWorkRequestsAsync(userId);
            return Ok(new { Success = true, WorkRequests = workRequests });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting offline work requests");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("offline/actions")]
    [Authorize]
    public async Task<IActionResult> QueueOfflineAction([FromBody] QueueOfflineActionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var action = new OfflineAction
            {
                UserId = userId,
                ActionType = request.ActionType,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Data = request.Data
            };

            var result = await _mobileAccessibilityService.QueueOfflineActionAsync(action);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Offline action queued successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to queue offline action" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing offline action");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("offline/actions/pending")]
    [Authorize]
    public async Task<IActionResult> GetPendingActions()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var actions = await _mobileAccessibilityService.GetPendingActionsAsync(userId);
            return Ok(new { Success = true, Actions = actions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending actions");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Accessibility Features
    [HttpGet("accessibility/profile")]
    [Authorize]
    public async Task<IActionResult> GetAccessibilityProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _mobileAccessibilityService.GetUserAccessibilityProfileAsync(userId);
            return Ok(new { Success = true, Profile = profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessibility profile");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPut("accessibility/profile")]
    [Authorize]
    public async Task<IActionResult> UpdateAccessibilityProfile([FromBody] AccessibilityProfile profile)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _mobileAccessibilityService.UpdateAccessibilityProfileAsync(userId, profile);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Accessibility profile updated successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to update accessibility profile" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating accessibility profile");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("accessibility/report")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GenerateAccessibilityReport()
    {
        try
        {
            var report = await _mobileAccessibilityService.GenerateAccessibilityReportAsync();
            return Ok(new { Success = true, Report = report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating accessibility report");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    // Mobile Optimization
    [HttpGet("mobile/configuration")]
    public async Task<IActionResult> GetMobileConfiguration()
    {
        try
        {
            var config = await _mobileAccessibilityService.GetMobileConfigurationAsync();
            return Ok(new { Success = true, Configuration = config });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile configuration");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("mobile/notifications/pending")]
    [Authorize]
    public async Task<IActionResult> GetPendingNotifications()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var notifications = await _mobileAccessibilityService.GetPendingNotificationsAsync(userId);
            return Ok(new { Success = true, Notifications = notifications });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending notifications");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("mobile/device-token")]
    [Authorize]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _mobileAccessibilityService.RegisterDeviceTokenAsync(
                userId, request.DeviceToken, request.Platform);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Device token registered successfully" });
            }
            
            return BadRequest(new { Success = false, Message = "Failed to register device token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }
}

// Request DTOs
public class QueueOfflineActionRequest
{
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class RegisterDeviceTokenRequest
{
    public string DeviceToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
} 