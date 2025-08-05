using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using Xunit;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;


namespace WorkIntakeSystem.Tests;

public class Phase4IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public Phase4IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationSchemeHandler>(
                        "Test", options => { });
                
                // Override policy evaluator for testing
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                
                // Mock services for testing
                services.AddScoped<IMicrosoft365Service, MockMicrosoft365Service>();
                services.AddScoped<IDevOpsIntegrationService, MockDevOpsIntegrationService>();
                services.AddScoped<IAdvancedAnalyticsService, MockAdvancedAnalyticsService>();
                services.AddScoped<IMobileAccessibilityService, MockMobileAccessibilityService>();
            });
        });

        _client = _factory.CreateClient();
    }

    #region Microsoft 365 Integration Tests

    [Fact]
    public async Task Microsoft365_CreateTeamsChannel_ReturnsSuccess()
    {
        // Arrange
        var request = new
        {
            TeamId = "test-team-id",
            ChannelName = "Test Channel",
            Description = "Test channel description"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/microsoft365/teams/channels", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
    }

    [Fact]
    public async Task Microsoft365_SendTeamsNotification_ReturnsSuccess()
    {
        // Arrange
        var request = new
        {
            ChannelId = "test-channel-id",
            Message = "Test notification message",
            WorkRequestId = 123
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/microsoft365/teams/notifications", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
    }

    [Fact]
    public async Task Microsoft365_CreatePowerBIWorkspace_ReturnsWorkspaceId()
    {
        // Arrange
        var request = new
        {
            WorkspaceName = "Test Workspace"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/microsoft365/powerbi/workspaces", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("WorkspaceId", out var workspaceId));
        Assert.False(string.IsNullOrEmpty(workspaceId.GetString()));
    }

    [Fact]
    public async Task Microsoft365_GetPowerBIReports_ReturnsReportsList()
    {
        // Arrange
        var workspaceId = "test-workspace-id";

        // Act
        var response = await _client.GetAsync($"/api/microsoft365/powerbi/workspaces/{workspaceId}/reports");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("Reports", out var reports));
        Assert.True(reports.ValueKind == JsonValueKind.Array);
    }

    #endregion

    #region DevOps Integration Tests

    [Fact]
    public async Task DevOpsIntegration_CreateAzureDevOpsWorkItem_ReturnsWorkItemId()
    {
        // Arrange
        var request = new
        {
            Project = "TestProject",
            WorkItemType = "User Story",
            Title = "Test Work Item",
            Description = "Test work item description",
            WorkRequestId = 123
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/devopsintegration/azure-devops/work-items", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("WorkItemId", out var workItemId));
        Assert.False(string.IsNullOrEmpty(workItemId.GetString()));
    }

    [Fact]
    public async Task DevOpsIntegration_CreateJiraIssue_ReturnsIssueKey()
    {
        // Arrange
        var request = new
        {
            Project = "TEST",
            IssueType = "Story",
            Summary = "Test Jira Issue",
            Description = "Test issue description",
            WorkRequestId = 123
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/devopsintegration/jira/issues", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("IssueKey", out var issueKey));
        Assert.False(string.IsNullOrEmpty(issueKey.GetString()));
    }

    [Fact]
    public async Task DevOpsIntegration_SyncWorkRequestStatus_ReturnsSuccess()
    {
        // Arrange
        var workRequestId = 123;

        // Act
        var response = await _client.PostAsync($"/api/devopsintegration/sync/work-requests/{workRequestId}", null);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
    }

    #endregion

    #region Advanced Analytics Tests

    [Fact]
    public async Task AdvancedAnalytics_PredictWorkRequestPriority_ReturnsPrediction()
    {
        // Arrange
        var workRequestId = 123;

        // Act
        var response = await _client.GetAsync($"/api/advancedanalytics/predict/priority/{workRequestId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.TryGetProperty("WorkRequestId", out var workRequestIdProp));
        Assert.Equal(workRequestId, workRequestIdProp.GetInt32());
        Assert.True(result.TryGetProperty("PredictedPriority", out var priority));
        Assert.True(priority.GetDecimal() > 0);
    }

    [Fact]
    public async Task AdvancedAnalytics_GetExecutiveDashboard_ReturnsDashboardData()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync($"/api/advancedanalytics/dashboard/executive?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.TryGetProperty("TotalWorkRequests", out var totalRequests));
        Assert.True(totalRequests.GetInt32() >= 0);
    }

    [Fact]
    public async Task AdvancedAnalytics_GetRiskIndicators_ReturnsRiskIndicators()
    {
        // Arrange
        var departmentId = 1;

        // Act
        var response = await _client.GetAsync($"/api/advancedanalytics/assess/risk-indicators/{departmentId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task AdvancedAnalytics_PredictWorkload_ReturnsWorkloadPrediction()
    {
        // Arrange
        var departmentId = 1;
        var targetDate = DateTime.UtcNow.AddDays(30);

        // Act
        var response = await _client.GetAsync($"/api/advancedanalytics/predict/workload/{departmentId}?targetDate={targetDate:yyyy-MM-dd}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.TryGetProperty("DepartmentId", out var deptId));
        Assert.Equal(departmentId, deptId.GetInt32());
        Assert.True(result.TryGetProperty("TargetDate", out var date));
    }

    #endregion

    #region Mobile & Accessibility Tests

    [Fact]
    public async Task MobileAccessibility_GetPWAManifest_ReturnsManifest()
    {
        // Act
        var response = await _client.GetAsync("/api/mobileaccessibility/pwa/manifest");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.TryGetProperty("name", out var name));
        Assert.Equal("Work Intake System", name.GetString());
    }

    [Fact]
    public async Task MobileAccessibility_GetServiceWorkerConfig_ReturnsConfig()
    {
        // Act
        var response = await _client.GetAsync("/api/mobileaccessibility/pwa/service-worker-config");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("Config", out var config));
        Assert.True(config.TryGetProperty("Version", out var version));
        Assert.False(string.IsNullOrEmpty(version.GetString()));
    }

    [Fact]
    public async Task MobileAccessibility_SyncOfflineData_ReturnsSuccess()
    {
        // Act
        var response = await _client.PostAsync("/api/mobileaccessibility/offline/sync", null);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
    }

    [Fact]
    public async Task MobileAccessibility_GetOfflineWorkRequests_ReturnsWorkRequests()
    {
        // Act
        var response = await _client.GetAsync("/api/mobileaccessibility/offline/work-requests");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("WorkRequests", out var workRequests));
        Assert.True(workRequests.ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task MobileAccessibility_GetAccessibilityProfile_ReturnsProfile()
    {
        // Act
        var response = await _client.GetAsync("/api/mobileaccessibility/accessibility/profile");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("Profile", out var profile));
        Assert.True(profile.TryGetProperty("FontScale", out var fontScale));
        Assert.True(fontScale.GetDouble() > 0);
    }

    [Fact]
    public async Task MobileAccessibility_UpdateAccessibilityProfile_ReturnsSuccess()
    {
        // Arrange
        var profile = new
        {
            HighContrast = true,
            FontScale = 1.2,
            ReducedMotion = false,
            ScreenReaderEnabled = false,
            KeyboardNavigation = "enhanced",
            PreferredColorSchemes = new[] { "dark" }
        };

        var json = JsonSerializer.Serialize(profile);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/mobileaccessibility/accessibility/profile", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
    }

    [Fact]
    public async Task MobileAccessibility_GetMobileConfiguration_ReturnsConfiguration()
    {
        // Act
        var response = await _client.GetAsync("/api/mobileaccessibility/mobile/configuration");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(result.GetProperty("Success").GetBoolean());
        Assert.True(result.TryGetProperty("Configuration", out var config));
        Assert.True(config.TryGetProperty("PushNotificationsEnabled", out var pushEnabled));
        Assert.True(pushEnabled.ValueKind == JsonValueKind.True || pushEnabled.ValueKind == JsonValueKind.False);
    }

    #endregion

    #region End-to-End Integration Tests

    [Fact]
    public async Task Phase4_EndToEndWorkflow_Microsoft365Integration()
    {
        // 1. Create Teams channel for work request
        var channelRequest = new
        {
            TeamId = "test-team",
            ChannelName = "Work Request 123",
            Description = "Channel for work request collaboration"
        };

        var channelJson = JsonSerializer.Serialize(channelRequest);
        var channelContent = new StringContent(channelJson, Encoding.UTF8, "application/json");
        var channelResponse = await _client.PostAsync("/api/microsoft365/teams/channels", channelContent);
        Assert.True(channelResponse.IsSuccessStatusCode);

        // 2. Create SharePoint site
        var siteRequest = new
        {
            SiteName = "Work Request 123 Site",
            Description = "Document repository for work request",
            WorkRequestId = 123
        };

        var siteJson = JsonSerializer.Serialize(siteRequest);
        var siteContent = new StringContent(siteJson, Encoding.UTF8, "application/json");
        var siteResponse = await _client.PostAsync("/api/microsoft365/sharepoint/sites", siteContent);
        Assert.True(siteResponse.IsSuccessStatusCode);

        // 3. Create Power BI workspace for analytics
        var workspaceRequest = new
        {
            WorkspaceName = "Work Request Analytics"
        };

        var workspaceJson = JsonSerializer.Serialize(workspaceRequest);
        var workspaceContent = new StringContent(workspaceJson, Encoding.UTF8, "application/json");
        var workspaceResponse = await _client.PostAsync("/api/microsoft365/powerbi/workspaces", workspaceContent);
        Assert.True(workspaceResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Phase4_EndToEndWorkflow_DevOpsIntegration()
    {
        // 1. Create Azure DevOps work item
        var devOpsRequest = new
        {
            Project = "TestProject",
            WorkItemType = "User Story",
            Title = "Implement Feature X",
            Description = "Detailed description of feature implementation",
            WorkRequestId = 123
        };

        var devOpsJson = JsonSerializer.Serialize(devOpsRequest);
        var devOpsContent = new StringContent(devOpsJson, Encoding.UTF8, "application/json");
        var devOpsResponse = await _client.PostAsync("/api/devopsintegration/azure-devops/work-items", devOpsContent);
        Assert.True(devOpsResponse.IsSuccessStatusCode);

        // 2. Create Jira issue
        var jiraRequest = new
        {
            Project = "TEST",
            IssueType = "Story",
            Summary = "Implement Feature X",
            Description = "Detailed description of feature implementation",
            WorkRequestId = 123
        };

        var jiraJson = JsonSerializer.Serialize(jiraRequest);
        var jiraContent = new StringContent(jiraJson, Encoding.UTF8, "application/json");
        var jiraResponse = await _client.PostAsync("/api/devopsintegration/jira/issues", jiraContent);
        Assert.True(jiraResponse.IsSuccessStatusCode);

        // 3. Sync work request status
        var syncResponse = await _client.PostAsync("/api/devopsintegration/sync/work-requests/123", null);
        Assert.True(syncResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Phase4_EndToEndWorkflow_AnalyticsAndReporting()
    {
        // 1. Get executive dashboard
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var dashboardResponse = await _client.GetAsync($"/api/advancedanalytics/dashboard/executive?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        Assert.True(dashboardResponse.IsSuccessStatusCode);

        // 2. Identify workflow bottlenecks
        var bottlenecksResponse = await _client.GetAsync("/api/advancedanalytics/workflow/bottlenecks");
        Assert.True(bottlenecksResponse.IsSuccessStatusCode);

        // 3. Build custom report
        var reportRequest = new
        {
            ReportName = "Department Performance Report",
            DataSources = new[] { "WorkRequests", "Departments", "Users" },
            Filters = new object[] { },
            Columns = new object[] { },
            Groupings = new object[] { },
            Charts = new object[] { }
        };

        var reportJson = JsonSerializer.Serialize(reportRequest);
        var reportContent = new StringContent(reportJson, Encoding.UTF8, "application/json");
        var reportResponse = await _client.PostAsync("/api/advancedanalytics/reports/custom", reportContent);
        Assert.True(reportResponse.IsSuccessStatusCode);

        // 4. Export data
        var exportRequest = new
        {
            EntityType = "WorkRequest",
            Fields = new[] { "Id", "Title", "Status", "Priority" },
            Filters = new Dictionary<string, object>(),
            Format = 1, // Excel
            StartDate = startDate,
            EndDate = endDate
        };

        var exportJson = JsonSerializer.Serialize(exportRequest);
        var exportContent = new StringContent(exportJson, Encoding.UTF8, "application/json");
        var exportResponse = await _client.PostAsync("/api/advancedanalytics/export/data", exportContent);
        Assert.True(exportResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Phase4_EndToEndWorkflow_MobileAndAccessibility()
    {
        // 1. Get PWA manifest
        var manifestResponse = await _client.GetAsync("/api/mobileaccessibility/pwa/manifest");
        Assert.True(manifestResponse.IsSuccessStatusCode);

        // 2. Configure accessibility profile
        var profileRequest = new
        {
            HighContrast = true,
            FontScale = 1.5,
            ReducedMotion = true,
            ScreenReaderEnabled = false,
            KeyboardNavigation = "enhanced",
            PreferredColorSchemes = new[] { "high-contrast" }
        };

        var profileJson = JsonSerializer.Serialize(profileRequest);
        var profileContent = new StringContent(profileJson, Encoding.UTF8, "application/json");
        var profileResponse = await _client.PutAsync("/api/mobileaccessibility/accessibility/profile", profileContent);
        Assert.True(profileResponse.IsSuccessStatusCode);

        // 3. Queue offline action
        var offlineActionRequest = new
        {
            ActionType = "UpdateWorkRequest",
            EntityType = "WorkRequest",
            EntityId = 123,
            Data = new Dictionary<string, object> { { "Status", "In Progress" } }
        };

        var offlineJson = JsonSerializer.Serialize(offlineActionRequest);
        var offlineContent = new StringContent(offlineJson, Encoding.UTF8, "application/json");
        var offlineResponse = await _client.PostAsync("/api/mobileaccessibility/offline/actions", offlineContent);
        Assert.True(offlineResponse.IsSuccessStatusCode);

        // 4. Sync offline data
        var syncResponse = await _client.PostAsync("/api/mobileaccessibility/offline/sync", null);
        Assert.True(syncResponse.IsSuccessStatusCode);
    }

    #endregion
}

// Mock implementations for testing
public class MockMicrosoft365Service : IMicrosoft365Service
{
    public Task<bool> CreateTeamsChannelAsync(string teamId, string channelName, string description) => Task.FromResult(true);
    public Task<bool> SendTeamsNotificationAsync(string channelId, string message, int workRequestId) => Task.FromResult(true);
    public Task<string> CreateTeamsMeetingAsync(string subject, DateTime startTime, DateTime endTime, List<string> attendees) => Task.FromResult("https://teams.microsoft.com/meeting/123");
    public Task<string> CreateSharePointSiteAsync(string siteName, string description, int workRequestId) => Task.FromResult("https://tenant.sharepoint.com/sites/test");
    public Task<bool> UploadDocumentAsync(string siteUrl, string fileName, Stream fileContent) => Task.FromResult(true);
    public Task<List<SharePointDocument>> GetDocumentsAsync(string siteUrl) => Task.FromResult(new List<SharePointDocument>());
    public Task<string> CreatePowerBIWorkspaceAsync(string workspaceName) => Task.FromResult(Guid.NewGuid().ToString());
    public Task<bool> PublishReportAsync(string workspaceId, string reportName, byte[] reportData) => Task.FromResult(true);
    public Task<List<PowerBIReport>> GetReportsAsync(string workspaceId) => Task.FromResult(new List<PowerBIReport>());
    public Task<string> GetReportEmbedTokenAsync(string reportId) => Task.FromResult("test-token");
}

public class MockDevOpsIntegrationService : IDevOpsIntegrationService
{
    public Task<string> CreateAzureDevOpsWorkItemAsync(string project, string workItemType, string title, string description, int workRequestId) => Task.FromResult("12345");
    public Task<bool> UpdateAzureDevOpsWorkItemAsync(string workItemId, Dictionary<string, object> fields) => Task.FromResult(true);
    public Task<AzureDevOpsWorkItem> GetAzureDevOpsWorkItemAsync(string workItemId) => Task.FromResult(new AzureDevOpsWorkItem { Id = workItemId });
    public Task<List<AzureDevOpsWorkItem>> GetWorkItemsByWorkRequestAsync(int workRequestId) => Task.FromResult(new List<AzureDevOpsWorkItem>());
    public Task<string> CreateJiraIssueAsync(string project, string issueType, string summary, string description, int workRequestId) => Task.FromResult("TEST-123");
    public Task<bool> UpdateJiraIssueAsync(string issueKey, Dictionary<string, object> fields) => Task.FromResult(true);
    public Task<JiraIssue> GetJiraIssueAsync(string issueKey) => Task.FromResult(new JiraIssue { Key = issueKey });
    public Task<List<JiraIssue>> GetIssuesByWorkRequestAsync(int workRequestId) => Task.FromResult(new List<JiraIssue>());
    public Task<bool> SyncWorkRequestStatusAsync(int workRequestId) => Task.FromResult(true);
    public Task<List<IntegrationSyncResult>> GetSyncHistoryAsync(int workRequestId) => Task.FromResult(new List<IntegrationSyncResult>());
}

public class MockAdvancedAnalyticsService : IAdvancedAnalyticsService
{
    public Task<PriorityPrediction> PredictPriorityAsync(int workRequestId) => Task.FromResult(new PriorityPrediction { WorkRequestId = workRequestId, PredictedPriority = 7.5m, Confidence = 0.85m });
    public Task<IEnumerable<PriorityTrend>> PredictPriorityTrendsAsync(int departmentId, DateTime targetDate) => Task.FromResult<IEnumerable<PriorityTrend>>(new List<PriorityTrend>());
    public Task<ResourceForecast> ForecastResourceNeedsAsync(int departmentId, DateTime targetDate) => Task.FromResult(new ResourceForecast { DepartmentId = departmentId, TargetDate = targetDate });
    public Task<CapacityPrediction> PredictCapacityUtilizationAsync(int departmentId, DateTime targetDate) => Task.FromResult(new CapacityPrediction { DepartmentId = departmentId, TargetDate = targetDate });
    public Task<CompletionPrediction> PredictCompletionTimeAsync(int workRequestId) => Task.FromResult(new CompletionPrediction { WorkRequestId = workRequestId });
    public Task<IEnumerable<CompletionTrend>> PredictCompletionTrendsAsync(int departmentId, DateTime targetDate) => Task.FromResult<IEnumerable<CompletionTrend>>(new List<CompletionTrend>());
    public Task<BusinessValueROI> CalculateROIAsync(int workRequestId) => Task.FromResult(new BusinessValueROI { WorkRequestId = workRequestId });
    public Task<IEnumerable<BusinessValueTrend>> AnalyzeBusinessValueTrendsAsync(int businessVerticalId, DateTime fromDate, DateTime toDate) => Task.FromResult<IEnumerable<BusinessValueTrend>>(new List<BusinessValueTrend>());
    public Task<RiskAssessment> AssessProjectRiskAsync(int workRequestId) => Task.FromResult(new RiskAssessment { WorkRequestId = workRequestId });
    public Task<IEnumerable<RiskIndicator>> GetRiskIndicatorsAsync(int departmentId) => Task.FromResult<IEnumerable<RiskIndicator>>(new List<RiskIndicator>());
    public Task<IEnumerable<PredictiveInsight>> GetPredictiveInsightsAsync(int businessVerticalId) => Task.FromResult<IEnumerable<PredictiveInsight>>(new List<PredictiveInsight>());
    public Task<WorkloadPrediction> PredictWorkloadAsync(int departmentId, DateTime targetDate) => Task.FromResult(new WorkloadPrediction { DepartmentId = departmentId, TargetDate = targetDate });
}

public class MockMobileAccessibilityService : IMobileAccessibilityService
{
    public Task<PWAManifest> GetPWAManifestAsync() => Task.FromResult(new PWAManifest { Name = "Work Intake System", ShortName = "WorkIntake" });
    public Task<ServiceWorkerConfig> GetServiceWorkerConfigAsync() => Task.FromResult(new ServiceWorkerConfig { Version = "1.0.0" });
    public Task<List<OfflineResource>> GetOfflineResourcesAsync() => Task.FromResult(new List<OfflineResource>());
    public Task<bool> SyncOfflineDataAsync(string userId) => Task.FromResult(true);
    public Task<List<OfflineWorkRequest>> GetOfflineWorkRequestsAsync(string userId) => Task.FromResult(new List<OfflineWorkRequest>());
    public Task<bool> QueueOfflineActionAsync(OfflineAction action) => Task.FromResult(true);
    public Task<List<OfflineAction>> GetPendingActionsAsync(string userId) => Task.FromResult(new List<OfflineAction>());
    public Task<AccessibilityProfile> GetUserAccessibilityProfileAsync(string userId) => Task.FromResult(new AccessibilityProfile { UserId = userId, FontScale = 1.0 });
    public Task<bool> UpdateAccessibilityProfileAsync(string userId, AccessibilityProfile profile) => Task.FromResult(true);
    public Task<AccessibilityReport> GenerateAccessibilityReportAsync() => Task.FromResult(new AccessibilityReport { ComplianceScore = 85.5 });
    public Task<MobileConfiguration> GetMobileConfigurationAsync() => Task.FromResult(new MobileConfiguration { PushNotificationsEnabled = true });
    public Task<List<MobileNotification>> GetPendingNotificationsAsync(string userId) => Task.FromResult(new List<MobileNotification>());
    public Task<bool> RegisterDeviceTokenAsync(string userId, string deviceToken, string platform) => Task.FromResult(true);
}

// Test authentication helpers
public class TestAuthenticationSchemeHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationSchemeHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Role, "Administrator")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class FakePolicyEvaluator : IPolicyEvaluator
{
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Role, "Administrator")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
} 