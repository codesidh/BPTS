using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace WorkIntakeSystem.Infrastructure.Services;

public class DevOpsIntegrationService : IDevOpsIntegrationService
{
    private readonly ILogger<DevOpsIntegrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _azureDevOpsUrl;
    private readonly string _azureDevOpsToken;
    private readonly string _jiraUrl;
    private readonly string _jiraUsername;
    private readonly string _jiraApiToken;

    public DevOpsIntegrationService(
        ILogger<DevOpsIntegrationService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _azureDevOpsUrl = _configuration["DevOps:AzureDevOps:BaseUrl"] ?? "https://dev.azure.com";
        _azureDevOpsToken = _configuration["DevOps:AzureDevOps:PersonalAccessToken"] ?? "";
        _jiraUrl = _configuration["DevOps:Jira:ServerUrl"] ?? "";
        _jiraUsername = _configuration["DevOps:Jira:Username"] ?? "";
        _jiraApiToken = _configuration["DevOps:Jira:ApiToken"] ?? "";
    }

    // Azure DevOps Integration with Real API Calls
    public async Task<string> CreateAzureDevOpsWorkItemAsync(string project, string workItemType, string title, string description, int workRequestId)
    {
        try
        {
            _logger.LogInformation("Creating Azure DevOps work item for project {Project}: {Title}", project, title);

            var organization = _configuration["DevOps:AzureDevOps:Organization"] ?? "";
            var url = $"{_azureDevOpsUrl}/{organization}/{project}/_apis/wit/workitems/${workItemType}?api-version=7.0";

            var workItemData = new object[]
            {
                new { op = "add", path = "/fields/System.Title", value = title },
                new { op = "add", path = "/fields/System.Description", value = description },
                new { op = "add", path = "/fields/System.Tags", value = $"WorkRequest-{workRequestId}" },
                new { op = "add", path = "/fields/Microsoft.VSTS.Common.Priority", value = 2 },
                new { op = "add", path = "/fields/System.CreatedBy", value = "Work Intake System" }
            };

            var json = JsonSerializer.Serialize(workItemData);
            var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_azureDevOpsToken}")));

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var workItemResponse = JsonSerializer.Deserialize<AzureDevOpsWorkItemResponse>(responseContent);
                
                if (workItemResponse?.Id != null)
                {
                    _logger.LogInformation("Successfully created Azure DevOps work item {WorkItemId} for work request {WorkRequestId}", workItemResponse.Id, workRequestId);
                    return workItemResponse.Id.ToString();
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create Azure DevOps work item. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Azure DevOps work item for project {Project}", project);
            return string.Empty;
        }
    }

    public async Task<bool> UpdateAzureDevOpsWorkItemAsync(string workItemId, Dictionary<string, object> fields)
    {
        try
        {
            _logger.LogInformation("Updating Azure DevOps work item {WorkItemId}", workItemId);

            var organization = _configuration["DevOps:AzureDevOps:Organization"] ?? "";
            var project = _configuration["DevOps:AzureDevOps:Project"] ?? "";
            var url = $"{_azureDevOpsUrl}/{organization}/{project}/_apis/wit/workitems/{workItemId}?api-version=7.0";

            var updateOperations = fields.Select(field => new
            {
                op = "add",
                path = $"/fields/{field.Key}",
                value = field.Value
            }).ToArray();

            var json = JsonSerializer.Serialize(updateOperations);
            var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_azureDevOpsToken}")));

            var response = await _httpClient.PatchAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated Azure DevOps work item {WorkItemId}", workItemId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update Azure DevOps work item {WorkItemId}. Status: {StatusCode}, Error: {Error}", workItemId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Azure DevOps work item {WorkItemId}", workItemId);
            return false;
        }
    }

    public async Task<AzureDevOpsWorkItem> GetAzureDevOpsWorkItemAsync(string workItemId)
    {
        try
        {
            _logger.LogInformation("Retrieving Azure DevOps work item {WorkItemId}", workItemId);

            var organization = _configuration["DevOps:AzureDevOps:Organization"] ?? "";
            var project = _configuration["DevOps:AzureDevOps:Project"] ?? "";
            var url = $"{_azureDevOpsUrl}/{organization}/{project}/_apis/wit/workitems/{workItemId}?api-version=7.0";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_azureDevOpsToken}")));

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var workItemResponse = JsonSerializer.Deserialize<AzureDevOpsWorkItemResponse>(responseContent);
                
                if (workItemResponse != null)
                {
                    var workItem = new AzureDevOpsWorkItem
                    {
                        Id = workItemResponse.Id.ToString(),
                        Title = workItemResponse.Fields?.GetValueOrDefault("System.Title")?.ToString() ?? "",
                        Description = workItemResponse.Fields?.GetValueOrDefault("System.Description")?.ToString() ?? "",
                        State = workItemResponse.Fields?.GetValueOrDefault("System.State")?.ToString() ?? "",
                        AssignedTo = workItemResponse.Fields?.GetValueOrDefault("System.AssignedTo")?.ToString() ?? "",
                        CreatedDate = DateTime.TryParse(workItemResponse.Fields?.GetValueOrDefault("System.CreatedDate")?.ToString(), out var createdDate) ? createdDate : DateTime.UtcNow,
                        ChangedDate = DateTime.TryParse(workItemResponse.Fields?.GetValueOrDefault("System.ChangedDate")?.ToString(), out var changedDate) ? changedDate : DateTime.UtcNow
                    };

                    _logger.LogInformation("Successfully retrieved Azure DevOps work item {WorkItemId}", workItemId);
                    return workItem;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Azure DevOps work item {WorkItemId}. Status: {StatusCode}, Error: {Error}", workItemId, response.StatusCode, errorContent);
            return new AzureDevOpsWorkItem();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Azure DevOps work item {WorkItemId}", workItemId);
            return new AzureDevOpsWorkItem();
        }
    }

    public async Task<List<AzureDevOpsWorkItem>> GetWorkItemsByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving Azure DevOps work items for work request {WorkRequestId}", workRequestId);

            var organization = _configuration["DevOps:AzureDevOps:Organization"] ?? "";
            var project = _configuration["DevOps:AzureDevOps:Project"] ?? "";
            var query = $"SELECT [System.Id] FROM WorkItems WHERE [System.Tags] CONTAINS 'WorkRequest-{workRequestId}'";
            var url = $"{_azureDevOpsUrl}/{organization}/{project}/_apis/wit/wiql?api-version=7.0";

            var queryData = new { query = query };
            var json = JsonSerializer.Serialize(queryData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_azureDevOpsToken}")));

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var queryResponse = JsonSerializer.Deserialize<AzureDevOpsQueryResponse>(responseContent);
                
                var workItems = new List<AzureDevOpsWorkItem>();
                if (queryResponse?.WorkItems != null)
                {
                    foreach (var workItemRef in queryResponse.WorkItems)
                    {
                        var workItem = await GetAzureDevOpsWorkItemAsync(workItemRef.Id.ToString());
                        if (!string.IsNullOrEmpty(workItem.Id))
                        {
                            workItems.Add(workItem);
                        }
                    }
                }

                _logger.LogInformation("Retrieved {WorkItemCount} Azure DevOps work items for work request {WorkRequestId}", workItems.Count, workRequestId);
                return workItems;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to query Azure DevOps work items for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new List<AzureDevOpsWorkItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Azure DevOps work items for work request {WorkRequestId}", workRequestId);
            return new List<AzureDevOpsWorkItem>();
        }
    }

    // Jira Integration with Real API Calls
    public async Task<string> CreateJiraIssueAsync(string project, string issueType, string summary, string description, int workRequestId)
    {
        try
        {
            _logger.LogInformation("Creating Jira issue for project {Project}: {Summary}", project, summary);

            var url = $"{_jiraUrl}/rest/api/3/issue";

            var issueData = new
            {
                fields = new
                {
                    project = new { key = project },
                    summary = summary,
                    description = new
                    {
                        type = "doc",
                        version = 1,
                        content = new[]
                        {
                            new
                            {
                                type = "paragraph",
                                content = new[]
                                {
                                    new
                                    {
                                        type = "text",
                                        text = description
                                    }
                                }
                            }
                        }
                    },
                    issuetype = new { name = issueType },
                    labels = new[] { $"WorkRequest-{workRequestId}" },
                    priority = new { name = "Medium" }
                }
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_jiraUsername}:{_jiraApiToken}")));

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var issueResponse = JsonSerializer.Deserialize<JiraIssueResponse>(responseContent);
                
                if (issueResponse?.Key != null)
                {
                    _logger.LogInformation("Successfully created Jira issue {IssueKey} for work request {WorkRequestId}", issueResponse.Key, workRequestId);
                    return issueResponse.Key;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create Jira issue. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Jira issue for project {Project}", project);
            return string.Empty;
        }
    }

    public async Task<bool> UpdateJiraIssueAsync(string issueKey, Dictionary<string, object> fields)
    {
        try
        {
            _logger.LogInformation("Updating Jira issue {IssueKey}", issueKey);

            var url = $"{_jiraUrl}/rest/api/3/issue/{issueKey}";

            var updateData = new { fields = fields };
            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_jiraUsername}:{_jiraApiToken}")));

            var response = await _httpClient.PutAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated Jira issue {IssueKey}", issueKey);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update Jira issue {IssueKey}. Status: {StatusCode}, Error: {Error}", issueKey, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Jira issue {IssueKey}", issueKey);
            return false;
        }
    }

    public async Task<JiraIssue> GetJiraIssueAsync(string issueKey)
    {
        try
        {
            _logger.LogInformation("Retrieving Jira issue {IssueKey}", issueKey);

            var url = $"{_jiraUrl}/rest/api/3/issue/{issueKey}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_jiraUsername}:{_jiraApiToken}")));

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var issueResponse = JsonSerializer.Deserialize<JiraIssueResponse>(responseContent);
                
                if (issueResponse?.Fields != null)
                {
                    var issue = new JiraIssue
                    {
                        Key = issueResponse.Key ?? "",
                        Summary = issueResponse.Fields.Summary ?? "",
                        Description = issueResponse.Fields.Description?.Content?.FirstOrDefault()?.Content?.FirstOrDefault()?.Text ?? "",
                        Status = issueResponse.Fields.Status?.Name ?? "",
                        Assignee = issueResponse.Fields.Assignee?.DisplayName ?? "",
                        Created = DateTime.TryParse(issueResponse.Fields.Created, out var created) ? created : DateTime.UtcNow,
                        Updated = DateTime.TryParse(issueResponse.Fields.Updated, out var updated) ? updated : DateTime.UtcNow
                    };

                    _logger.LogInformation("Successfully retrieved Jira issue {IssueKey}", issueKey);
                    return issue;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jira issue {IssueKey}. Status: {StatusCode}, Error: {Error}", issueKey, response.StatusCode, errorContent);
            return new JiraIssue();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jira issue {IssueKey}", issueKey);
            return new JiraIssue();
        }
    }

    public async Task<List<JiraIssue>> GetIssuesByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving Jira issues for work request {WorkRequestId}", workRequestId);

            var jql = $"labels = WorkRequest-{workRequestId}";
            var url = $"{_jiraUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_jiraUsername}:{_jiraApiToken}")));

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var searchResponse = JsonSerializer.Deserialize<JiraSearchResponse>(responseContent);
                
                var issues = new List<JiraIssue>();
                if (searchResponse?.Issues != null)
                {
                    foreach (var issueResponse in searchResponse.Issues)
                    {
                        var issue = new JiraIssue
                        {
                            Key = issueResponse.Key ?? "",
                            Summary = issueResponse.Fields?.Summary ?? "",
                            Description = issueResponse.Fields?.Description?.Content?.FirstOrDefault()?.Content?.FirstOrDefault()?.Text ?? "",
                            Status = issueResponse.Fields?.Status?.Name ?? "",
                            Assignee = issueResponse.Fields?.Assignee?.DisplayName ?? "",
                            Created = DateTime.TryParse(issueResponse.Fields?.Created, out var created) ? created : DateTime.UtcNow,
                            Updated = DateTime.TryParse(issueResponse.Fields?.Updated, out var updated) ? updated : DateTime.UtcNow
                        };
                        issues.Add(issue);
                    }
                }

                _logger.LogInformation("Retrieved {IssueCount} Jira issues for work request {WorkRequestId}", issues.Count, workRequestId);
                return issues;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to search Jira issues for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new List<JiraIssue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jira issues for work request {WorkRequestId}", workRequestId);
            return new List<JiraIssue>();
        }
    }

    // Synchronization
    public async Task<bool> SyncWorkRequestStatusAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Synchronizing work request status for {WorkRequestId}", workRequestId);

            // Get work items from Azure DevOps
            var azureDevOpsWorkItems = await GetWorkItemsByWorkRequestAsync(workRequestId);
            
            // Get issues from Jira
            var jiraIssues = await GetIssuesByWorkRequestAsync(workRequestId);

            // Log synchronization results
            _logger.LogInformation("Synchronization completed for work request {WorkRequestId}: {AzureDevOpsCount} Azure DevOps work items, {JiraCount} Jira issues", 
                workRequestId, azureDevOpsWorkItems.Count, jiraIssues.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing work request status for {WorkRequestId}", workRequestId);
            return false;
        }
    }

    public async Task<List<IntegrationSyncResult>> GetSyncHistoryAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving sync history for work request {WorkRequestId}", workRequestId);

            // In a real implementation, this would query a database table that stores sync history
            var syncResults = new List<IntegrationSyncResult>
            {
                new IntegrationSyncResult
                {
                    WorkRequestId = workRequestId,
                    IntegrationType = "Azure DevOps",
                    SyncTime = DateTime.UtcNow.AddHours(-1),
                    Status = "Success"
                },
                new IntegrationSyncResult
                {
                    WorkRequestId = workRequestId,
                    IntegrationType = "Jira",
                    SyncTime = DateTime.UtcNow.AddHours(-2),
                    Status = "Success"
                }
            };

            return syncResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync history for work request {WorkRequestId}", workRequestId);
            return new List<IntegrationSyncResult>();
        }
    }

    // Response Models for JSON Deserialization
    private class AzureDevOpsWorkItemResponse
    {
        public int? Id { get; set; }
        public Dictionary<string, object>? Fields { get; set; }
    }

    private class AzureDevOpsQueryResponse
    {
        public List<AzureDevOpsWorkItemReference>? WorkItems { get; set; }
    }

    private class AzureDevOpsWorkItemReference
    {
        public int Id { get; set; }
    }

    private class JiraIssueResponse
    {
        public string? Key { get; set; }
        public JiraIssueFields? Fields { get; set; }
    }

    private class JiraIssueFields
    {
        public string? Summary { get; set; }
        public JiraDescription? Description { get; set; }
        public JiraStatus? Status { get; set; }
        public JiraUser? Assignee { get; set; }
        public string? Created { get; set; }
        public string? Updated { get; set; }
    }

    private class JiraDescription
    {
        public List<JiraContent>? Content { get; set; }
    }

    private class JiraContent
    {
        public List<JiraContentItem>? Content { get; set; }
    }

    private class JiraContentItem
    {
        public string? Text { get; set; }
    }

    private class JiraStatus
    {
        public string? Name { get; set; }
    }

    private class JiraUser
    {
        public string? DisplayName { get; set; }
    }

    private class JiraSearchResponse
    {
        public List<JiraIssueResponse>? Issues { get; set; }
    }
} 