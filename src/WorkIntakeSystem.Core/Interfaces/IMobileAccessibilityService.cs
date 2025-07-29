using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IMobileAccessibilityService
{
    // PWA Support
    Task<PWAManifest> GetPWAManifestAsync();
    Task<ServiceWorkerConfig> GetServiceWorkerConfigAsync();
    Task<List<OfflineResource>> GetOfflineResourcesAsync();
    
    // Offline Capabilities
    Task<bool> SyncOfflineDataAsync(string userId);
    Task<List<OfflineWorkRequest>> GetOfflineWorkRequestsAsync(string userId);
    Task<bool> QueueOfflineActionAsync(OfflineAction action);
    Task<List<OfflineAction>> GetPendingActionsAsync(string userId);
    
    // Accessibility Features
    Task<AccessibilityProfile> GetUserAccessibilityProfileAsync(string userId);
    Task<bool> UpdateAccessibilityProfileAsync(string userId, AccessibilityProfile profile);
    Task<AccessibilityReport> GenerateAccessibilityReportAsync();
    
    // Mobile Optimization
    Task<MobileConfiguration> GetMobileConfigurationAsync();
    Task<List<MobileNotification>> GetPendingNotificationsAsync(string userId);
    Task<bool> RegisterDeviceTokenAsync(string userId, string deviceToken, string platform);
} 