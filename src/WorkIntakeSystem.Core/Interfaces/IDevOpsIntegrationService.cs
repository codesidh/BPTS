using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IDevOpsIntegrationService
{
    // Azure DevOps Integration
    Task<string> CreateAzureDevOpsWorkItemAsync(string project, string workItemType, string title, string description, int workRequestId);
    Task<bool> UpdateAzureDevOpsWorkItemAsync(string workItemId, Dictionary<string, object> fields);
    Task<AzureDevOpsWorkItem> GetAzureDevOpsWorkItemAsync(string workItemId);
    Task<List<AzureDevOpsWorkItem>> GetWorkItemsByWorkRequestAsync(int workRequestId);
    
    // Jira Integration
    Task<string> CreateJiraIssueAsync(string project, string issueType, string summary, string description, int workRequestId);
    Task<bool> UpdateJiraIssueAsync(string issueKey, Dictionary<string, object> fields);
    Task<JiraIssue> GetJiraIssueAsync(string issueKey);
    Task<List<JiraIssue>> GetIssuesByWorkRequestAsync(int workRequestId);
    
    // Synchronization
    Task<bool> SyncWorkRequestStatusAsync(int workRequestId);
    Task<List<IntegrationSyncResult>> GetSyncHistoryAsync(int workRequestId);
}

public class AzureDevOpsWorkItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string WorkItemType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ChangedDate { get; set; }
    public string Url { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
}

public class JiraIssue
{
    public string Key { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public string Url { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
}

public class IntegrationSyncResult
{
    public string Id { get; set; } = string.Empty;
    public int WorkRequestId { get; set; }
    public string IntegrationType { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public DateTime SyncTime { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
} 