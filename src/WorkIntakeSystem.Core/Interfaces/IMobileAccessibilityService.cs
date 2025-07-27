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

public class PWAManifest
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StartUrl { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public List<PWAIcon> Icons { get; set; } = new();
}

public class PWAIcon
{
    public string Src { get; set; } = string.Empty;
    public string Sizes { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}

public class ServiceWorkerConfig
{
    public string Version { get; set; } = string.Empty;
    public List<string> CacheUrls { get; set; } = new();
    public Dictionary<string, string> CacheStrategies { get; set; } = new();
    public int CacheMaxAge { get; set; }
}

public class OfflineResource
{
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public bool IsCritical { get; set; }
}

public class OfflineWorkRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastSynced { get; set; }
    public bool HasPendingChanges { get; set; }
}

public class OfflineAction
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsSynced { get; set; }
}

public class AccessibilityProfile
{
    public string UserId { get; set; } = string.Empty;
    public bool HighContrast { get; set; }
    public double FontScale { get; set; } = 1.0;
    public bool ReducedMotion { get; set; }
    public bool ScreenReaderEnabled { get; set; }
    public string KeyboardNavigation { get; set; } = string.Empty;
    public List<string> PreferredColorSchemes { get; set; } = new();
}

public class AccessibilityReport
{
    public DateTime GeneratedAt { get; set; }
    public double ComplianceScore { get; set; }
    public List<AccessibilityIssue> Issues { get; set; } = new();
    public List<AccessibilityRecommendation> Recommendations { get; set; } = new();
}

public class AccessibilityIssue
{
    public string Id { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Element { get; set; } = string.Empty;
    public string WCAGCriterion { get; set; } = string.Empty;
}

public class AccessibilityRecommendation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public double EstimatedEffort { get; set; }
}

public class MobileConfiguration
{
    public bool PushNotificationsEnabled { get; set; }
    public int OfflineSyncInterval { get; set; }
    public bool BiometricAuthEnabled { get; set; }
    public Dictionary<string, object> FeatureFlags { get; set; } = new();
}

public class MobileNotification
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = new();
    public DateTime ScheduledAt { get; set; }
    public bool IsRead { get; set; }
} 