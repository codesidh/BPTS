using Xunit;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace WorkIntakeSystem.Tests;

public class Phase4BasicTests
{
    [Fact]
    public void Phase4Models_CanBeCreatedAndSerialized()
    {
        // Test Phase 4 models can be created and serialized
        var projectDashboard = new ProjectDashboard
        {
            ProjectId = 1,
            ProjectName = "Test Project",
            CompletionPercentage = 75.5,
            TasksCompleted = 45,
            TasksRemaining = 15,
            TeamMembers = 8,
            Budget = 150000,
            BudgetUsed = 112500
        };

        var json = JsonSerializer.Serialize(projectDashboard);
        var deserialized = JsonSerializer.Deserialize<ProjectDashboard>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("Test Project", deserialized.ProjectName);
        Assert.Equal(75.5, deserialized.CompletionPercentage);
    }

    [Fact]
    public void PWAManifest_CanBeCreatedWithRequiredProperties()
    {
        var manifest = new PWAManifest
        {
            Name = "Work Intake System",
            ShortName = "WorkIntake",
            Description = "Enterprise work intake management system",
            StartUrl = "/",
            Display = "standalone",
            ThemeColor = "#1976d2",
            BackgroundColor = "#ffffff"
        };

        Assert.Equal("Work Intake System", manifest.Name);
        Assert.Equal("WorkIntake", manifest.ShortName);
        Assert.Equal("/", manifest.StartUrl);
        Assert.Equal("standalone", manifest.Display);
    }

    [Fact]
    public void AccessibilityProfile_CanBeConfigured()
    {
        var profile = new AccessibilityProfile
        {
            UserId = "test-user",
            HighContrast = true,
            FontScale = 1.5,
            ReducedMotion = true,
            ScreenReaderEnabled = false,
            KeyboardNavigation = true,
            PreferredColorSchemes = new List<string> { "high-contrast", "dark" }
        };

        Assert.Equal("test-user", profile.UserId);
        Assert.True(profile.HighContrast);
        Assert.Equal(1.5, profile.FontScale);
        Assert.True(profile.ReducedMotion);
        Assert.Contains("high-contrast", profile.PreferredColorSchemes);
    }

    [Fact]
    public void CustomReport_CanBeBuiltWithFiltersAndCharts()
    {
        var report = new CustomReport
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Department Performance Report",
            CreatedDate = DateTime.UtcNow,
            Filters = new List<ReportFilter>
            {
                new ReportFilter
                {
                    Id = "filter1",
                    FieldName = "Department",
                    Operator = "equals",
                    Value = "IT",
                    DisplayName = "Department Filter"
                }
            },
            Charts = new List<ReportChart>
            {
                new ReportChart
                {
                    Id = "chart1",
                    Title = "Work Request Trends",
                    ChartType = "line",
                    XAxisField = "Date",
                    YAxisField = "Count"
                }
            }
        };

        Assert.Equal("Department Performance Report", report.Name);
        Assert.Single(report.Filters);
        Assert.Single(report.Charts);
        Assert.Equal("Department", report.Filters[0].FieldName);
        Assert.Equal("Work Request Trends", report.Charts[0].Title);
    }

    [Fact]
    public void ExportFormat_EnumHasAllRequiredValues()
    {
        var formats = Enum.GetValues<ExportFormat>();
        var formatNames = formats.Select(f => f.ToString()).ToList();

        Assert.Contains("Excel", formatNames);
        Assert.Contains("CSV", formatNames);
        Assert.Contains("JSON", formatNames);
        Assert.Contains("PDF", formatNames);
        Assert.Equal(4, formats.Length);
    }

    [Fact]
    public void DataExportRequest_CanBeConfiguredForDifferentFormats()
    {
        var excelRequest = new DataExportRequest
        {
            EntityType = "WorkRequest",
            Fields = new List<string> { "Id", "Title", "Status", "Priority" },
            Filters = new Dictionary<string, object> { { "Status", "Active" } },
            Format = ExportFormat.Excel
        };

        var csvRequest = new DataExportRequest
        {
            EntityType = "Department",
            Fields = new List<string> { "Id", "Name", "TeamSize" },
            Filters = new Dictionary<string, object>(),
            Format = ExportFormat.CSV
        };

        Assert.Equal("WorkRequest", excelRequest.EntityType);
        Assert.Equal(ExportFormat.Excel, excelRequest.Format);
        Assert.Equal(4, excelRequest.Fields.Count);
        Assert.Contains("Status", excelRequest.Filters.Keys);

        Assert.Equal("Department", csvRequest.EntityType);
        Assert.Equal(ExportFormat.CSV, csvRequest.Format);
        Assert.Equal(3, csvRequest.Fields.Count);
    }

    [Fact]
    public void OfflineAction_CanBeQueuedWithData()
    {
        var action = new OfflineAction
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "test-user",
            ActionType = "UpdateWorkRequest",
            EntityType = "WorkRequest",
            EntityId = 123,
            Data = new Dictionary<string, object>
            {
                { "Status", "In Progress" },
                { "Notes", "Updated offline" },
                { "Priority", 8.5 }
            },
            CreatedAt = DateTime.UtcNow,
            IsSynced = false
        };

        Assert.Equal("test-user", action.UserId);
        Assert.Equal("UpdateWorkRequest", action.ActionType);
        Assert.Equal("WorkRequest", action.EntityType);
        Assert.Equal(123, action.EntityId);
        Assert.False(action.IsSynced);
        Assert.Equal(3, action.Data.Count);
        Assert.Equal("In Progress", action.Data["Status"]);
    }

    [Fact]
    public void PriorityPrediction_CanBeCreatedWithConfidenceScore()
    {
        var prediction = new PriorityPrediction
        {
            WorkRequestId = 123,
            PredictedScore = 8.7,
            Confidence = 0.92,
            InfluencingFactors = new List<string>
            {
                "Business Value",
                "Urgency",
                "Department Workload",
                "Historical Patterns"
            },
            PredictionDate = DateTime.UtcNow
        };

        Assert.Equal(123, prediction.WorkRequestId);
        Assert.Equal(8.7, prediction.PredictedScore);
        Assert.Equal(0.92, prediction.Confidence);
        Assert.Equal(4, prediction.InfluencingFactors.Count);
        Assert.Contains("Business Value", prediction.InfluencingFactors);
    }

    [Fact]
    public void WorkflowBottleneck_CanIdentifyStageIssues()
    {
        var bottleneck = new WorkflowBottleneck
        {
            Stage = "Technical Review",
            AverageTimeInStage = 5.2,
            ItemsInStage = 15,
            BottleneckScore = 0.85,
            Recommendations = new List<string>
            {
                "Add additional reviewer capacity",
                "Implement automated checks",
                "Streamline review process"
            }
        };

        Assert.Equal("Technical Review", bottleneck.Stage);
        Assert.Equal(5.2, bottleneck.AverageTimeInStage);
        Assert.Equal(15, bottleneck.ItemsInStage);
        Assert.Equal(0.85, bottleneck.BottleneckScore);
        Assert.Equal(3, bottleneck.Recommendations.Count);
    }

    [Fact]
    public void IntegrationSyncResult_CanTrackSynchronization()
    {
        var syncResult = new IntegrationSyncResult
        {
            Id = Guid.NewGuid().ToString(),
            WorkRequestId = 123,
            IntegrationType = "AzureDevOps",
            ExternalId = "12345",
            SyncTime = DateTime.UtcNow,
            Success = true,
            ErrorMessage = null
        };

        var failedSync = new IntegrationSyncResult
        {
            Id = Guid.NewGuid().ToString(),
            WorkRequestId = 124,
            IntegrationType = "Jira",
            ExternalId = "PROJ-456",
            SyncTime = DateTime.UtcNow,
            Success = false,
            ErrorMessage = "Authentication failed"
        };

        Assert.Equal(123, syncResult.WorkRequestId);
        Assert.Equal("AzureDevOps", syncResult.IntegrationType);
        Assert.True(syncResult.Success);
        Assert.Null(syncResult.ErrorMessage);

        Assert.Equal(124, failedSync.WorkRequestId);
        Assert.Equal("Jira", failedSync.IntegrationType);
        Assert.False(failedSync.Success);
        Assert.Equal("Authentication failed", failedSync.ErrorMessage);
    }

    [Fact]
    public void ServiceWorkerConfig_CanBeCachedWithStrategies()
    {
        var config = new ServiceWorkerConfig
        {
            Version = "1.0.0",
            CacheUrls = new List<string>
            {
                "/",
                "/dashboard",
                "/work-requests",
                "/static/js/bundle.js",
                "/static/css/main.css"
            },
            CacheStrategies = new List<string>
            {
                "/api/",
                "/static/",
                "/"
            },
            CacheMaxAge = 86400 // 24 hours
        };

        Assert.Equal("1.0.0", config.Version);
        Assert.Equal(5, config.CacheUrls.Count);
        Assert.Equal(3, config.CacheStrategies.Count);
        Assert.Contains("/api/", config.CacheStrategies);
        Assert.Contains("/static/", config.CacheStrategies);
        Assert.Equal(86400, config.CacheMaxAge);
    }
}

// Test helper classes to verify model structure
public class Phase4TestDataBuilder
{
    public static ExecutiveDashboard CreateExecutiveDashboard()
    {
        return new ExecutiveDashboard
        {
            TotalWorkRequests = 150,
            AverageCompletionTime = 14.5,
            StatusDistribution = new Dictionary<string, int>
            {
                { "Active", 45 },
                { "Completed", 85 },
                { "On Hold", 20 }
            },
            DepartmentMetrics = new List<DepartmentMetrics>
            {
                new DepartmentMetrics
                {
                    DepartmentId = 1,
                    DepartmentName = "IT",
                    ActiveWorkRequests = 25,
                    CompletedWorkRequests = 45,
                    AverageCompletionTime = 12.3,
                    TeamUtilization = 0.78,
                    TeamSize = 8
                }
            },
            TrendData = new List<TrendData>
            {
                new TrendData
                {
                    Date = DateTime.UtcNow.AddDays(-7),
                    Category = "Completed",
                    Value = 12,
                    Label = "Week -1"
                }
            }
        };
    }

    public static AccessibilityReport CreateAccessibilityReport()
    {
        return new AccessibilityReport
        {
            GeneratedAt = DateTime.UtcNow,
            ComplianceScore = 87.5,
            Issues = new List<string>
            {
                "Form labels not properly associated"
            },
            Recommendations = new List<string>
            {
                "Improve Form Labels - Associate all form labels with inputs"
            }
        };
    }
} 