using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IGitLabIntegrationService
{
    // Repository Management
    Task<GitLabProject> CreateProjectAsync(string name, string description, string visibility = "private");
    Task<GitLabProject> GetProjectAsync(int projectId);
    Task<List<GitLabProject>> GetProjectsByWorkRequestAsync(int workRequestId);
    Task<bool> ArchiveProjectAsync(int projectId);
    
    // Pipeline Management
    Task<GitLabPipeline> TriggerPipelineAsync(int projectId, string branch, Dictionary<string, string>? variables = null);
    Task<GitLabPipeline> GetPipelineAsync(int projectId, int pipelineId);
    Task<List<GitLabPipeline>> GetPipelinesAsync(int projectId, string? status = null, int? limit = null);
    Task<bool> CancelPipelineAsync(int projectId, int pipelineId);
    Task<bool> RetryPipelineAsync(int projectId, int pipelineId);
    
    // Job Management
    Task<GitLabJob> GetJobAsync(int projectId, int jobId);
    Task<List<GitLabJob>> GetJobsForPipelineAsync(int projectId, int pipelineId);
    Task<string> GetJobLogsAsync(int projectId, int jobId);
    Task<byte[]> GetJobArtifactsAsync(int projectId, int jobId);
    Task<bool> RetryJobAsync(int projectId, int jobId);
    Task<bool> CancelJobAsync(int projectId, int jobId);
    
    // Merge Request Management
    Task<GitLabMergeRequest> CreateMergeRequestAsync(int projectId, string sourceBranch, string targetBranch, string title, string description);
    Task<GitLabMergeRequest> GetMergeRequestAsync(int projectId, int mergeRequestId);
    Task<List<GitLabMergeRequest>> GetMergeRequestsAsync(int projectId, string? state = null);
    Task<bool> ApproveMergeRequestAsync(int projectId, int mergeRequestId);
    Task<GitLabMergeRequest> MergeMergeRequestAsync(int projectId, int mergeRequestId);
    
    // Environment & Deployment Management
    Task<List<GitLabEnvironment>> GetEnvironmentsAsync(int projectId);
    Task<GitLabDeployment> GetDeploymentAsync(int projectId, int deploymentId);
    Task<List<GitLabDeployment>> GetDeploymentsAsync(int projectId, string? environment = null);
    Task<bool> CreateDeploymentAsync(int projectId, string environment, string sha, Dictionary<string, string>? variables = null);
    
    // CI/CD Configuration
    Task<string> GetCiConfigAsync(int projectId, string? gitRef = null);
    Task<bool> UpdateCiConfigAsync(int projectId, string ciConfigContent, string commitMessage);
    Task<GitLabCiLint> LintCiConfigAsync(string ciConfigContent);
    
    // Variables & Secrets Management
    Task<bool> CreateVariableAsync(int projectId, string key, string value, bool masked = false, bool protectedVar = false);
    Task<List<GitLabVariable>> GetVariablesAsync(int projectId);
    Task<bool> UpdateVariableAsync(int projectId, string key, string value, bool? masked = null, bool? protectedVar = null);
    Task<bool> DeleteVariableAsync(int projectId, string key);
    
    // Webhooks & Integration
    Task<GitLabWebhook> CreateWebhookAsync(int projectId, string url, List<string> events);
    Task<List<GitLabWebhook>> GetWebhooksAsync(int projectId);
    Task<bool> DeleteWebhookAsync(int projectId, int hookId);
    
    // Integration with Work Requests
    Task<bool> LinkWorkRequestToProjectAsync(int workRequestId, int projectId);
    Task<List<GitLabPipeline>> GetPipelinesForWorkRequestAsync(int workRequestId);
    Task<PipelineStatusSummary> GetWorkRequestPipelineStatusAsync(int workRequestId);
    Task<bool> CreateWorkRequestBranchAsync(int workRequestId, int projectId, string branchName);
}

public enum GitLabPipelineStatus
{
    Created,
    WaitingForResource,
    Preparing,
    Pending,
    Running,
    Success,
    Failed,
    Canceled,
    Skipped,
    Manual,
    Scheduled
}

public enum GitLabJobStatus
{
    Created,
    Pending,
    Running,
    Success,
    Failed,
    Canceled,
    Skipped,
    Manual,
    WaitingForResource,
    Preparing
}

public enum GitLabMergeRequestState
{
    Opened,
    Closed,
    Merged
}

public enum GitLabProjectVisibility
{
    Private,
    Internal,
    Public
} 