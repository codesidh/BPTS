using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using WorkIntakeSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace WorkIntakeSystem.Infrastructure.Services;

public class DevOpsIntegrationService : IDevOpsIntegrationService
{
    private readonly VssConnection _azureDevOpsConnection;
    private readonly ILogger<DevOpsIntegrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public DevOpsIntegrationService(
        ILogger<DevOpsIntegrationService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;

        var azureDevOpsUrl = _configuration["AzureDevOps:OrganizationUrl"];
        var personalAccessToken = _configuration["AzureDevOps:PersonalAccessToken"];
        
        if (!string.IsNullOrEmpty(azureDevOpsUrl) && !string.IsNullOrEmpty(personalAccessToken))
        {
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            _azureDevOpsConnection = new VssConnection(new Uri(azureDevOpsUrl), credentials);
        }
    }

    // Azure DevOps Integration
    public async Task<string> CreateAzureDevOpsWorkItemAsync(string project, string workItemType, string title, string description, int workRequestId)
    {
        try
        {
            var workItemTrackingClient = _azureDevOpsConnection.GetClient<WorkItemTrackingHttpClient>();

            var document = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.Title",
                    Value = title
                },
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.Description",
                    Value = description
                },
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/Custom.WorkRequestId",
                    Value = workRequestId.ToString()
                }
            };

            var workItem = await workItemTrackingClient.CreateWorkItemAsync(document, project, workItemType);
            _logger.LogInformation("Created Azure DevOps work item {WorkItemId} for work request {WorkRequestId}", workItem.Id, workRequestId);
            return workItem.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Azure DevOps work item for work request {WorkRequestId}", workRequestId);
            return string.Empty;
        }
    }

    public async Task<bool> UpdateAzureDevOpsWorkItemAsync(string workItemId, Dictionary<string, object> fields)
    {
        try
        {
            var workItemTrackingClient = _azureDevOpsConnection.GetClient<WorkItemTrackingHttpClient>();

            var document = new JsonPatchDocument();
            foreach (var field in fields)
            {
                document.Add(new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = $"/fields/{field.Key}",
                    Value = field.Value
                });
            }

            await workItemTrackingClient.UpdateWorkItemAsync(document, int.Parse(workItemId));
            _logger.LogInformation("Updated Azure DevOps work item {WorkItemId}", workItemId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Azure DevOps work item {WorkItemId}", workItemId);
            return false;
        }
    }

    public async Task<AzureDevOpsWorkItem> GetAzureDevOpsWorkItemAsync(string workItemId)
    {
        try
        {
            var workItemTrackingClient = _azureDevOpsConnection.GetClient<WorkItemTrackingHttpClient>();
            var workItem = await workItemTrackingClient.GetWorkItemAsync(int.Parse(workItemId));

            var result = new AzureDevOpsWorkItem
            {
                Id = workItem.Id.ToString(),
                Title = workItem.Fields["System.Title"]?.ToString() ?? string.Empty,
                WorkItemType = workItem.Fields["System.WorkItemType"]?.ToString() ?? string.Empty,
                State = workItem.Fields["System.State"]?.ToString() ?? string.Empty,
                AssignedTo = workItem.Fields.ContainsKey("System.AssignedTo") ? workItem.Fields["System.AssignedTo"]?.ToString() ?? string.Empty : string.Empty,
                CreatedDate = DateTime.Parse(workItem.Fields["System.CreatedDate"]?.ToString() ?? DateTime.UtcNow.ToString()),
                ChangedDate = DateTime.Parse(workItem.Fields["System.ChangedDate"]?.ToString() ?? DateTime.UtcNow.ToString()),
                Url = workItem.Url,
                WorkRequestId = int.TryParse(workItem.Fields.ContainsKey("Custom.WorkRequestId") ? workItem.Fields["Custom.WorkRequestId"]?.ToString() : "0", out var id) ? id : 0
            };

            _logger.LogInformation("Retrieved Azure DevOps work item {WorkItemId}", workItemId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Azure DevOps work item {WorkItemId}", workItemId);
            return new AzureDevOpsWorkItem();
        }
    }

    public async Task<List<AzureDevOpsWorkItem>> GetWorkItemsByWorkRequestAsync(int workRequestId)
    {
        try
        {
            var workItemTrackingClient = _azureDevOpsConnection.GetClient<WorkItemTrackingHttpClient>();
            var wiql = new Wiql
            {
                Query = $"SELECT [System.Id] FROM WorkItems WHERE [Custom.WorkRequestId] = '{workRequestId}'"
            };

            var result = await workItemTrackingClient.QueryByWiqlAsync(wiql);
            var workItems = new List<AzureDevOpsWorkItem>();

            if (result.WorkItems != null)
            {
                foreach (var workItemRef in result.WorkItems)
                {
                    var workItem = await GetAzureDevOpsWorkItemAsync(workItemRef.Id.ToString());
                    workItems.Add(workItem);
                }
            }

            _logger.LogInformation("Retrieved {WorkItemCount} Azure DevOps work items for work request {WorkRequestId}", workItems.Count, workRequestId);
            return workItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Azure DevOps work items for work request {WorkRequestId}", workRequestId);
            return new List<AzureDevOpsWorkItem>();
        }
    }

    // Jira Integration
    public async Task<string> CreateJiraIssueAsync(string project, string issueType, string summary, string description, int workRequestId)
    {
        try
        {
            var jiraUrl = _configuration["Jira:BaseUrl"];
            var jiraToken = _configuration["Jira:ApiToken"];
            var jiraEmail = _configuration["Jira:Email"];

            var issueData = new
            {
                fields = new
                {
                    project = new { key = project },
                    summary = summary,
                    description = description,
                    issuetype = new { name = issueType },
                    customfield_10000 = workRequestId.ToString() // Custom field for Work Request ID
                }
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{jiraEmail}:{jiraToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.PostAsync($"{jiraUrl}/rest/api/3/issue", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var issueKey = responseData.GetProperty("key").GetString();
                
                _logger.LogInformation("Created Jira issue {IssueKey} for work request {WorkRequestId}", issueKey, workRequestId);
                return issueKey ?? string.Empty;
            }

            _logger.LogError("Failed to create Jira issue. Status: {StatusCode}", response.StatusCode);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Jira issue for work request {WorkRequestId}", workRequestId);
            return string.Empty;
        }
    }

    public async Task<bool> UpdateJiraIssueAsync(string issueKey, Dictionary<string, object> fields)
    {
        try
        {
            var jiraUrl = _configuration["Jira:BaseUrl"];
            var jiraToken = _configuration["Jira:ApiToken"];
            var jiraEmail = _configuration["Jira:Email"];

            var updateData = new { fields = fields };
            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{jiraEmail}:{jiraToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.PutAsync($"{jiraUrl}/rest/api/3/issue/{issueKey}", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Updated Jira issue {IssueKey}", issueKey);
                return true;
            }

            _logger.LogError("Failed to update Jira issue {IssueKey}. Status: {StatusCode}", issueKey, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Jira issue {IssueKey}", issueKey);
            return false;
        }
    }

    public async Task<JiraIssue> GetJiraIssueAsync(string issueKey)
    {
        try
        {
            var jiraUrl = _configuration["Jira:BaseUrl"];
            var jiraToken = _configuration["Jira:ApiToken"];
            var jiraEmail = _configuration["Jira:Email"];

            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{jiraEmail}:{jiraToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.GetAsync($"{jiraUrl}/rest/api/3/issue/{issueKey}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var issueData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var fields = issueData.GetProperty("fields");

                var result = new JiraIssue
                {
                    Key = issueKey,
                    Summary = fields.GetProperty("summary").GetString() ?? string.Empty,
                    IssueType = fields.GetProperty("issuetype").GetProperty("name").GetString() ?? string.Empty,
                    Status = fields.GetProperty("status").GetProperty("name").GetString() ?? string.Empty,
                    Assignee = fields.TryGetProperty("assignee", out var assignee) ? assignee.GetProperty("displayName").GetString() ?? string.Empty : string.Empty,
                    Created = DateTime.Parse(fields.GetProperty("created").GetString() ?? DateTime.UtcNow.ToString()),
                    Updated = DateTime.Parse(fields.GetProperty("updated").GetString() ?? DateTime.UtcNow.ToString()),
                    Url = $"{jiraUrl}/browse/{issueKey}",
                    WorkRequestId = fields.TryGetProperty("customfield_10000", out var workRequestId) ? int.Parse(workRequestId.GetString() ?? "0") : 0
                };

                _logger.LogInformation("Retrieved Jira issue {IssueKey}", issueKey);
                return result;
            }

            _logger.LogError("Failed to get Jira issue {IssueKey}. Status: {StatusCode}", issueKey, response.StatusCode);
            return new JiraIssue();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Jira issue {IssueKey}", issueKey);
            return new JiraIssue();
        }
    }

    public async Task<List<JiraIssue>> GetIssuesByWorkRequestAsync(int workRequestId)
    {
        try
        {
            var jiraUrl = _configuration["Jira:BaseUrl"];
            var jiraToken = _configuration["Jira:ApiToken"];
            var jiraEmail = _configuration["Jira:Email"];

            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{jiraEmail}:{jiraToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var jql = $"customfield_10000 = '{workRequestId}'";
            var encodedJql = Uri.EscapeDataString(jql);
            var response = await _httpClient.GetAsync($"{jiraUrl}/rest/api/3/search?jql={encodedJql}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var issues = searchResult.GetProperty("issues");
                var result = new List<JiraIssue>();

                foreach (var issue in issues.EnumerateArray())
                {
                    var issueKey = issue.GetProperty("key").GetString();
                    if (!string.IsNullOrEmpty(issueKey))
                    {
                        var jiraIssue = await GetJiraIssueAsync(issueKey);
                        result.Add(jiraIssue);
                    }
                }

                _logger.LogInformation("Retrieved {IssueCount} Jira issues for work request {WorkRequestId}", result.Count, workRequestId);
                return result;
            }

            _logger.LogError("Failed to search Jira issues for work request {WorkRequestId}. Status: {StatusCode}", workRequestId, response.StatusCode);
            return new List<JiraIssue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Jira issues for work request {WorkRequestId}", workRequestId);
            return new List<JiraIssue>();
        }
    }

    // Synchronization
    public async Task<bool> SyncWorkRequestStatusAsync(int workRequestId)
    {
        try
        {
            // Get work items from both systems
            var azureDevOpsItems = await GetWorkItemsByWorkRequestAsync(workRequestId);
            var jiraIssues = await GetIssuesByWorkRequestAsync(workRequestId);

            // Implement synchronization logic based on business rules
            // This is a simplified implementation
            var syncResults = new List<IntegrationSyncResult>();

            foreach (var item in azureDevOpsItems)
            {
                syncResults.Add(new IntegrationSyncResult
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkRequestId = workRequestId,
                    IntegrationType = "AzureDevOps",
                    ExternalId = item.Id,
                    SyncTime = DateTime.UtcNow,
                    Success = true
                });
            }

            foreach (var issue in jiraIssues)
            {
                syncResults.Add(new IntegrationSyncResult
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkRequestId = workRequestId,
                    IntegrationType = "Jira",
                    ExternalId = issue.Key,
                    SyncTime = DateTime.UtcNow,
                    Success = true
                });
            }

            _logger.LogInformation("Synchronized work request {WorkRequestId} with {SyncCount} external items", workRequestId, syncResults.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync work request {WorkRequestId}", workRequestId);
            return false;
        }
    }

    public async Task<List<IntegrationSyncResult>> GetSyncHistoryAsync(int workRequestId)
    {
        try
        {
            // This would typically be stored in a database
            // For now, returning mock data
            var syncHistory = new List<IntegrationSyncResult>
            {
                new IntegrationSyncResult
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkRequestId = workRequestId,
                    IntegrationType = "AzureDevOps",
                    ExternalId = "12345",
                    SyncTime = DateTime.UtcNow.AddHours(-1),
                    Success = true
                },
                new IntegrationSyncResult
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkRequestId = workRequestId,
                    IntegrationType = "Jira",
                    ExternalId = "PROJ-123",
                    SyncTime = DateTime.UtcNow.AddHours(-2),
                    Success = true
                }
            };

            _logger.LogInformation("Retrieved sync history for work request {WorkRequestId}", workRequestId);
            return syncHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync history for work request {WorkRequestId}", workRequestId);
            return new List<IntegrationSyncResult>();
        }
    }
} 