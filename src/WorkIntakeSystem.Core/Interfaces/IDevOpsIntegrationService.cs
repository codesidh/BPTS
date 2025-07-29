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