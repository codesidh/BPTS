using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace WorkIntakeSystem.Infrastructure.Services;

public class GitLabIntegrationService : IGitLabIntegrationService
{
    private readonly ILogger<GitLabIntegrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _gitLabUrl;
    private readonly string _privateToken;

    public GitLabIntegrationService(
        ILogger<GitLabIntegrationService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _gitLabUrl = _configuration["GitLab:ServerUrl"] ?? "";
        _privateToken = _configuration["GitLab:PrivateToken"] ?? "";

        // Configure HTTP client with authentication
        if (!string.IsNullOrEmpty(_privateToken))
        {
            _httpClient.DefaultRequestHeaders.Add("Private-Token", _privateToken);
        }
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    #region Repository Management

    public async Task<GitLabProject> CreateProjectAsync(string name, string description, string visibility = "private")
    {
        try
        {
            _logger.LogInformation("Creating GitLab project {Name} with visibility {Visibility}", name, visibility);

            var projectData = new
            {
                name = name,
                description = description,
                visibility = visibility,
                initialize_with_readme = true
            };

            var json = JsonSerializer.Serialize(projectData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var projectJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var project = MapToGitLabProject(projectJson);
                _logger.LogInformation("Successfully created GitLab project {Name} with ID {ProjectId}", name, project.GitLabProjectId);
                return project;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create GitLab project {Name}: {Error}", name, error);
                throw new Exception($"Failed to create GitLab project: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitLab project {Name}", name);
            throw;
        }
    }

    public async Task<GitLabProject> GetProjectAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab project {ProjectId}", projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var projectJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return MapToGitLabProject(projectJson);
            }
            else
            {
                _logger.LogError("Failed to get GitLab project {ProjectId}", projectId);
                throw new Exception($"Failed to get GitLab project: {projectId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<GitLabProject>> GetProjectsByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab projects for work request {WorkRequestId}", workRequestId);

            // This would typically query a database to find projects linked to the work request
            // For now, return empty list as this requires database integration
            _logger.LogWarning("Database integration required to fetch projects by work request ID");
            return new List<GitLabProject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab projects for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<bool> ArchiveProjectAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Archiving GitLab project {ProjectId}", projectId);

            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/archive", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully archived GitLab project {ProjectId}", projectId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to archive GitLab project {ProjectId}: {Error}", projectId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving GitLab project {ProjectId}", projectId);
            return false;
        }
    }

    #endregion

    #region Pipeline Management

    public async Task<GitLabPipeline> TriggerPipelineAsync(int projectId, string branch, Dictionary<string, string>? variables = null)
    {
        try
        {
            _logger.LogInformation("Triggering GitLab pipeline for project {ProjectId} on branch {Branch}", projectId, branch);

            var pipelineData = new Dictionary<string, object>
            {
                { "ref", branch }
            };

            if (variables != null && variables.Any())
            {
                var variablesList = variables.Select(v => new { key = v.Key, value = v.Value }).ToList();
                pipelineData.Add("variables", variablesList);
            }

            var json = JsonSerializer.Serialize(pipelineData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/pipeline", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pipelineJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var pipeline = MapToGitLabPipeline(pipelineJson, projectId);
                _logger.LogInformation("Successfully triggered GitLab pipeline {PipelineId} for project {ProjectId}", pipeline.GitLabPipelineId, projectId);
                return pipeline;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to trigger GitLab pipeline for project {ProjectId}: {Error}", projectId, error);
                throw new Exception($"Failed to trigger GitLab pipeline: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering GitLab pipeline for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<GitLabPipeline> GetPipelineAsync(int projectId, int pipelineId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pipelineJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return MapToGitLabPipeline(pipelineJson, projectId);
            }
            else
            {
                _logger.LogError("Failed to get GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);
                throw new Exception($"Failed to get GitLab pipeline: {pipelineId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);
            throw;
        }
    }

    public async Task<List<GitLabPipeline>> GetPipelinesAsync(int projectId, string? status = null, int? limit = null)
    {
        try
        {
            _logger.LogInformation("Getting GitLab pipelines for project {ProjectId}", projectId);

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"status={status}");
            if (limit.HasValue)
                queryParams.Add($"per_page={limit.Value}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pipelinesArray = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                
                var pipelines = new List<GitLabPipeline>();
                foreach (var pipelineJson in pipelinesArray)
                {
                    pipelines.Add(MapToGitLabPipeline(pipelineJson, projectId));
                }

                return pipelines;
            }
            else
            {
                _logger.LogError("Failed to get GitLab pipelines for project {ProjectId}", projectId);
                throw new Exception($"Failed to get GitLab pipelines for project: {projectId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab pipelines for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> CancelPipelineAsync(int projectId, int pipelineId)
    {
        try
        {
            _logger.LogInformation("Canceling GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);

            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}/cancel", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully canceled GitLab pipeline {PipelineId}", pipelineId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to cancel GitLab pipeline {PipelineId}: {Error}", pipelineId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling GitLab pipeline {PipelineId}", pipelineId);
            return false;
        }
    }

    public async Task<bool> RetryPipelineAsync(int projectId, int pipelineId)
    {
        try
        {
            _logger.LogInformation("Retrying GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);

            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}/retry", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully retried GitLab pipeline {PipelineId}", pipelineId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to retry GitLab pipeline {PipelineId}: {Error}", pipelineId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying GitLab pipeline {PipelineId}", pipelineId);
            return false;
        }
    }

    #endregion

    #region Job Management

    public async Task<GitLabJob> GetJobAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab job {JobId} for project {ProjectId}", jobId, projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jobJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return MapToGitLabJob(jobJson);
            }
            else
            {
                _logger.LogError("Failed to get GitLab job {JobId} for project {ProjectId}", jobId, projectId);
                throw new Exception($"Failed to get GitLab job: {jobId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab job {JobId} for project {ProjectId}", jobId, projectId);
            throw;
        }
    }

    public async Task<List<GitLabJob>> GetJobsForPipelineAsync(int projectId, int pipelineId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab jobs for pipeline {PipelineId} in project {ProjectId}", pipelineId, projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}/jobs");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jobsArray = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                
                var jobs = new List<GitLabJob>();
                foreach (var jobJson in jobsArray)
                {
                    jobs.Add(MapToGitLabJob(jobJson));
                }

                return jobs;
            }
            else
            {
                _logger.LogError("Failed to get GitLab jobs for pipeline {PipelineId}", pipelineId);
                throw new Exception($"Failed to get GitLab jobs for pipeline: {pipelineId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab jobs for pipeline {PipelineId}", pipelineId);
            throw;
        }
    }

    public async Task<string> GetJobLogsAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab job logs for job {JobId} in project {ProjectId}", jobId, projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/trace");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger.LogError("Failed to get GitLab job logs for job {JobId}", jobId);
                return $"Failed to retrieve job logs for job {jobId}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab job logs for job {JobId}", jobId);
            return $"Error retrieving job logs: {ex.Message}";
        }
    }

    public async Task<byte[]> GetJobArtifactsAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab job artifacts for job {JobId} in project {ProjectId}", jobId, projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/artifacts");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                _logger.LogError("Failed to get GitLab job artifacts for job {JobId}", jobId);
                return Array.Empty<byte>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab job artifacts for job {JobId}", jobId);
            return Array.Empty<byte>();
        }
    }

    public async Task<bool> RetryJobAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Retrying GitLab job {JobId} in project {ProjectId}", jobId, projectId);

            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/retry", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully retried GitLab job {JobId}", jobId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to retry GitLab job {JobId}: {Error}", jobId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying GitLab job {JobId}", jobId);
            return false;
        }
    }

    public async Task<bool> CancelJobAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Canceling GitLab job {JobId} in project {ProjectId}", jobId, projectId);

            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/cancel", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully canceled GitLab job {JobId}", jobId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to cancel GitLab job {JobId}: {Error}", jobId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling GitLab job {JobId}", jobId);
            return false;
        }
    }

    #endregion

    #region Merge Request Management

    public async Task<GitLabMergeRequest> CreateMergeRequestAsync(int projectId, string sourceBranch, string targetBranch, string title, string description)
    {
        try
        {
            _logger.LogInformation("Creating GitLab merge request in project {ProjectId}: {Title}", projectId, title);

            var mergeRequestData = new
            {
                source_branch = sourceBranch,
                target_branch = targetBranch,
                title = title,
                description = description
            };

            var json = JsonSerializer.Serialize(mergeRequestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var mergeRequest = MapToGitLabMergeRequest(mergeRequestJson, projectId);
                _logger.LogInformation("Successfully created GitLab merge request {MergeRequestId}", mergeRequest.GitLabMergeRequestId);
                return mergeRequest;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create GitLab merge request: {Error}", error);
                throw new Exception($"Failed to create GitLab merge request: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitLab merge request in project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<GitLabMergeRequest> GetMergeRequestAsync(int projectId, int mergeRequestId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab merge request {MergeRequestId} in project {ProjectId}", mergeRequestId, projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests/{mergeRequestId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return MapToGitLabMergeRequest(mergeRequestJson, projectId);
            }
            else
            {
                _logger.LogError("Failed to get GitLab merge request {MergeRequestId}", mergeRequestId);
                throw new Exception($"Failed to get GitLab merge request: {mergeRequestId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab merge request {MergeRequestId}", mergeRequestId);
            throw;
        }
    }

    public async Task<List<GitLabMergeRequest>> GetMergeRequestsAsync(int projectId, string? state = null)
    {
        try
        {
            _logger.LogInformation("Getting GitLab merge requests for project {ProjectId}", projectId);

            var queryString = !string.IsNullOrEmpty(state) ? $"?state={state}" : "";
            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestsArray = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                
                var mergeRequests = new List<GitLabMergeRequest>();
                foreach (var mergeRequestJson in mergeRequestsArray)
                {
                    mergeRequests.Add(MapToGitLabMergeRequest(mergeRequestJson, projectId));
                }

                return mergeRequests;
            }
            else
            {
                _logger.LogError("Failed to get GitLab merge requests for project {ProjectId}", projectId);
                throw new Exception($"Failed to get GitLab merge requests for project: {projectId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab merge requests for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> ApproveMergeRequestAsync(int projectId, int mergeRequestId)
    {
        try
        {
            _logger.LogInformation("Approving GitLab merge request {MergeRequestId} in project {ProjectId}", mergeRequestId, projectId);

            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests/{mergeRequestId}/approve", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully approved GitLab merge request {MergeRequestId}", mergeRequestId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to approve GitLab merge request {MergeRequestId}: {Error}", mergeRequestId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving GitLab merge request {MergeRequestId}", mergeRequestId);
            return false;
        }
    }

    public async Task<GitLabMergeRequest> MergeMergeRequestAsync(int projectId, int mergeRequestId)
    {
        try
        {
            _logger.LogInformation("Merging GitLab merge request {MergeRequestId} in project {ProjectId}", mergeRequestId, projectId);

            var response = await _httpClient.PutAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests/{mergeRequestId}/merge", null);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var mergeRequest = MapToGitLabMergeRequest(mergeRequestJson, projectId);
                _logger.LogInformation("Successfully merged GitLab merge request {MergeRequestId}", mergeRequestId);
                return mergeRequest;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to merge GitLab merge request {MergeRequestId}: {Error}", mergeRequestId, error);
                throw new Exception($"Failed to merge GitLab merge request: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging GitLab merge request {MergeRequestId}", mergeRequestId);
            throw;
        }
    }

    #endregion

    #region Environment & Deployment Management

    public async Task<List<GitLabEnvironment>> GetEnvironmentsAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab environments for project {ProjectId}", projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/environments");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var environmentsArray = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                
                var environments = new List<GitLabEnvironment>();
                foreach (var environmentJson in environmentsArray)
                {
                    environments.Add(MapToGitLabEnvironment(environmentJson, projectId));
                }

                return environments;
            }
            else
            {
                _logger.LogError("Failed to get GitLab environments for project {ProjectId}", projectId);
                throw new Exception($"Failed to get GitLab environments for project: {projectId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab environments for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<GitLabDeployment> GetDeploymentAsync(int projectId, int deploymentId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab deployment {DeploymentId} for project {ProjectId}", deploymentId, projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/deployments/{deploymentId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var deploymentJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return MapToGitLabDeployment(deploymentJson);
            }
            else
            {
                _logger.LogError("Failed to get GitLab deployment {DeploymentId}", deploymentId);
                throw new Exception($"Failed to get GitLab deployment: {deploymentId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab deployment {DeploymentId}", deploymentId);
            throw;
        }
    }

    public async Task<List<GitLabDeployment>> GetDeploymentsAsync(int projectId, string? environment = null)
    {
        try
        {
            _logger.LogInformation("Getting GitLab deployments for project {ProjectId}", projectId);

            var queryString = !string.IsNullOrEmpty(environment) ? $"?environment={environment}" : "";
            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/deployments{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var deploymentsArray = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                
                var deployments = new List<GitLabDeployment>();
                foreach (var deploymentJson in deploymentsArray)
                {
                    deployments.Add(MapToGitLabDeployment(deploymentJson));
                }

                return deployments;
            }
            else
            {
                _logger.LogError("Failed to get GitLab deployments for project {ProjectId}", projectId);
                throw new Exception($"Failed to get GitLab deployments for project: {projectId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab deployments for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> CreateDeploymentAsync(int projectId, string environment, string sha, Dictionary<string, string>? variables = null)
    {
        try
        {
            _logger.LogInformation("Creating GitLab deployment for project {ProjectId} to {Environment}", projectId, environment);

            var deploymentData = new Dictionary<string, object>
            {
                { "environment", environment },
                { "sha", sha }
            };

            if (variables != null && variables.Any())
            {
                deploymentData.Add("variables", variables);
            }

            var json = JsonSerializer.Serialize(deploymentData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/deployments", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created GitLab deployment for project {ProjectId}", projectId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create GitLab deployment: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitLab deployment for project {ProjectId}", projectId);
            return false;
        }
    }

    #endregion

    #region CI/CD Configuration

    public async Task<string> GetCiConfigAsync(int projectId, string? gitRef = null)
    {
        try
        {
            _logger.LogInformation("Getting CI configuration for GitLab project {ProjectId}", projectId);

            var queryString = !string.IsNullOrEmpty(gitRef) ? $"?ref={gitRef}" : "";
            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/repository/files/.gitlab-ci.yml/raw{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger.LogError("Failed to get CI configuration for project {ProjectId}", projectId);
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CI configuration for project {ProjectId}", projectId);
            return string.Empty;
        }
    }

    public async Task<bool> UpdateCiConfigAsync(int projectId, string ciConfigContent, string commitMessage)
    {
        try
        {
            _logger.LogInformation("Updating CI configuration for GitLab project {ProjectId}", projectId);

            var updateData = new
            {
                branch = "main",
                content = ciConfigContent,
                commit_message = commitMessage
            };

            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/repository/files/.gitlab-ci.yml", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated CI configuration for project {ProjectId}", projectId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update CI configuration for project {ProjectId}: {Error}", projectId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating CI configuration for project {ProjectId}", projectId);
            return false;
        }
    }

    public async Task<GitLabCiLint> LintCiConfigAsync(string ciConfigContent)
    {
        try
        {
            _logger.LogInformation("Linting CI configuration");

            var lintData = new { content = ciConfigContent };
            var json = JsonSerializer.Serialize(lintData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/ci/lint", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var lintJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return new GitLabCiLint
                {
                    Valid = GetJsonPropertyBool(lintJson, "valid"),
                    Errors = GetJsonPropertyArray(lintJson, "errors"),
                    Warnings = GetJsonPropertyArray(lintJson, "warnings"),
                    Status = GetJsonProperty(lintJson, "status")
                };
            }
            else
            {
                _logger.LogError("Failed to lint CI configuration");
                return new GitLabCiLint { Valid = false, Errors = new List<string> { "Failed to lint configuration" } };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linting CI configuration");
            return new GitLabCiLint { Valid = false, Errors = new List<string> { ex.Message } };
        }
    }

    #endregion

    #region Variables & Secrets Management

    public async Task<bool> CreateVariableAsync(int projectId, string key, string value, bool masked = false, bool protectedVar = false)
    {
        try
        {
            _logger.LogInformation("Creating variable {Key} for GitLab project {ProjectId}", key, projectId);

            var variableData = new
            {
                key = key,
                value = value,
                masked = masked,
                @protected = protectedVar
            };

            var json = JsonSerializer.Serialize(variableData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/variables", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created variable {Key} for project {ProjectId}", key, projectId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create variable {Key}: {Error}", key, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating variable {Key} for project {ProjectId}", key, projectId);
            return false;
        }
    }

    public async Task<List<GitLabVariable>> GetVariablesAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Getting variables for GitLab project {ProjectId}", projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/variables");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var variablesArray = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                
                var variables = new List<GitLabVariable>();
                foreach (var variableJson in variablesArray)
                {
                    variables.Add(MapToGitLabVariable(variableJson, projectId));
                }

                return variables;
            }
            else
            {
                _logger.LogError("Failed to get variables for project {ProjectId}", projectId);
                throw new Exception($"Failed to get variables for project: {projectId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting variables for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> UpdateVariableAsync(int projectId, string key, string value, bool? masked = null, bool? protectedVar = null)
    {
        try
        {
            _logger.LogInformation("Updating variable {Key} for GitLab project {ProjectId}", key, projectId);

            var variableData = new Dictionary<string, object> { { "value", value } };
            if (masked.HasValue) variableData.Add("masked", masked.Value);
            if (protectedVar.HasValue) variableData.Add("protected", protectedVar.Value);

            var json = JsonSerializer.Serialize(variableData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/variables/{key}", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated variable {Key} for project {ProjectId}", key, projectId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update variable {Key}: {Error}", key, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating variable {Key} for project {ProjectId}", key, projectId);
            return false;
        }
    }

    public async Task<bool> DeleteVariableAsync(int projectId, string key)
    {
        try
        {
            _logger.LogInformation("Deleting variable {Key} for GitLab project {ProjectId}", key, projectId);

            var response = await _httpClient.DeleteAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/variables/{key}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted variable {Key} for project {ProjectId}", key, projectId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete variable {Key}: {Error}", key, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting variable {Key} for project {ProjectId}", key, projectId);
            return false;
        }
    }

    #endregion

    #region Webhooks & Integration

    public async Task<GitLabWebhook> CreateWebhookAsync(int projectId, string url, List<string> events)
    {
        try
        {
            _logger.LogInformation("Creating webhook for GitLab project {ProjectId}", projectId);

            var webhookData = new Dictionary<string, object>
            {
                { "url", url }
            };

            foreach (var eventType in events)
            {
                webhookData.Add($"{eventType}_events", true);
            }

            var json = JsonSerializer.Serialize(webhookData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/hooks", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var webhookJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var webhook = MapToGitLabWebhook(webhookJson, projectId);
                _logger.LogInformation("Successfully created webhook {WebhookId} for project {ProjectId}", webhook.GitLabWebhookId, projectId);
                return webhook;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create webhook for project {ProjectId}: {Error}", projectId, error);
                throw new Exception($"Failed to create webhook: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating webhook for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<GitLabWebhook>> GetWebhooksAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Getting webhooks for GitLab project {ProjectId}", projectId);

            var response = await _httpClient.GetAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/hooks");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var webhooksArray = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                
                var webhooks = new List<GitLabWebhook>();
                foreach (var webhookJson in webhooksArray)
                {
                    webhooks.Add(MapToGitLabWebhook(webhookJson, projectId));
                }

                return webhooks;
            }
            else
            {
                _logger.LogError("Failed to get webhooks for project {ProjectId}", projectId);
                throw new Exception($"Failed to get webhooks for project: {projectId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhooks for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> DeleteWebhookAsync(int projectId, int hookId)
    {
        try
        {
            _logger.LogInformation("Deleting webhook {HookId} for GitLab project {ProjectId}", hookId, projectId);

            var response = await _httpClient.DeleteAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/hooks/{hookId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted webhook {HookId} for project {ProjectId}", hookId, projectId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete webhook {HookId}: {Error}", hookId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook {HookId} for project {ProjectId}", hookId, projectId);
            return false;
        }
    }

    #endregion

    #region Integration with Work Requests

    public async Task<bool> LinkWorkRequestToProjectAsync(int workRequestId, int projectId)
    {
        try
        {
            _logger.LogInformation("Linking work request {WorkRequestId} to GitLab project {ProjectId}", workRequestId, projectId);

            // This would typically update a database to link the work request to the project
            // For now, just log and return success
            _logger.LogWarning("Database integration required to link work request to project");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking work request {WorkRequestId} to GitLab project {ProjectId}", workRequestId, projectId);
            return false;
        }
    }

    public async Task<List<GitLabPipeline>> GetPipelinesForWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Getting GitLab pipelines for work request {WorkRequestId}", workRequestId);

            // This would typically query a database to find pipelines for the work request
            // For now, return empty list as this requires database integration
            _logger.LogWarning("Database integration required to fetch pipelines by work request ID");
            return new List<GitLabPipeline>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab pipelines for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<PipelineStatusSummary> GetWorkRequestPipelineStatusAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Getting pipeline status summary for work request {WorkRequestId}", workRequestId);

            var pipelines = await GetPipelinesForWorkRequestAsync(workRequestId);
            
            return new PipelineStatusSummary
            {
                WorkRequestId = workRequestId,
                TotalPipelines = pipelines.Count,
                SuccessfulPipelines = pipelines.Count(p => p.Status == GitLabPipelineStatus.Success),
                FailedPipelines = pipelines.Count(p => p.Status == GitLabPipelineStatus.Failed),
                RunningPipelines = pipelines.Count(p => p.Status == GitLabPipelineStatus.Running),
                LastPipelineDate = pipelines.OrderByDescending(p => p.StartedAt).FirstOrDefault()?.StartedAt,
                LastPipelineStatus = pipelines.OrderByDescending(p => p.StartedAt).FirstOrDefault()?.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pipeline status summary for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<bool> CreateWorkRequestBranchAsync(int workRequestId, int projectId, string branchName)
    {
        try
        {
            _logger.LogInformation("Creating branch {BranchName} for work request {WorkRequestId} in project {ProjectId}", branchName, workRequestId, projectId);

            var branchData = new
            {
                branch = branchName,
                @ref = "main"
            };

            var json = JsonSerializer.Serialize(branchData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_gitLabUrl}/api/v4/projects/{projectId}/repository/branches", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created branch {BranchName} for work request {WorkRequestId}", branchName, workRequestId);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create branch {BranchName}: {Error}", branchName, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch {BranchName} for work request {WorkRequestId}", branchName, workRequestId);
            return false;
        }
    }

    #endregion

    #region Private Helper Methods

    private GitLabProject MapToGitLabProject(JsonElement projectJson)
    {
        return new GitLabProject
        {
            Id = 0, // Would be set from database
            GitLabProjectId = GetJsonPropertyInt(projectJson, "id"),
            Name = GetJsonProperty(projectJson, "name"),
            Description = GetJsonProperty(projectJson, "description"),
            Visibility = GetJsonProperty(projectJson, "visibility"),
            WebUrl = GetJsonProperty(projectJson, "web_url"),
            SshUrl = GetJsonProperty(projectJson, "ssh_url_to_repo"),
            HttpUrl = GetJsonProperty(projectJson, "http_url_to_repo"),
            DefaultBranch = GetJsonProperty(projectJson, "default_branch"),
            Archived = GetJsonPropertyBool(projectJson, "archived")
        };
    }

    private GitLabPipeline MapToGitLabPipeline(JsonElement pipelineJson, int projectId)
    {
        return new GitLabPipeline
        {
            Id = 0, // Would be set from database
            GitLabPipelineId = GetJsonPropertyInt(pipelineJson, "id"),
            GitLabProjectId = projectId,
            Status = MapStringToGitLabPipelineStatus(GetJsonProperty(pipelineJson, "status")),
            Ref = GetJsonProperty(pipelineJson, "ref"),
            Sha = GetJsonProperty(pipelineJson, "sha"),
            WebUrl = GetJsonProperty(pipelineJson, "web_url"),
            StartedAt = GetJsonPropertyDateTime(pipelineJson, "started_at"),
            FinishedAt = GetJsonPropertyDateTime(pipelineJson, "finished_at"),
            TriggeredBy = GetJsonProperty(pipelineJson, "user")
        };
    }

    private GitLabJob MapToGitLabJob(JsonElement jobJson)
    {
        return new GitLabJob
        {
            Id = 0, // Would be set from database
            GitLabJobId = GetJsonPropertyInt(jobJson, "id"),
            Name = GetJsonProperty(jobJson, "name"),
            Stage = GetJsonProperty(jobJson, "stage"),
            Status = MapStringToGitLabJobStatus(GetJsonProperty(jobJson, "status")),
            StartedAt = GetJsonPropertyDateTime(jobJson, "started_at"),
            FinishedAt = GetJsonPropertyDateTime(jobJson, "finished_at"),
            WebUrl = GetJsonProperty(jobJson, "web_url"),
            RunnerDescription = GetJsonProperty(jobJson, "runner"),
            AllowFailure = GetJsonPropertyBool(jobJson, "allow_failure")
        };
    }

    private GitLabMergeRequest MapToGitLabMergeRequest(JsonElement mergeRequestJson, int projectId)
    {
        return new GitLabMergeRequest
        {
            Id = 0, // Would be set from database
            GitLabMergeRequestId = GetJsonPropertyInt(mergeRequestJson, "iid"),
            GitLabProjectId = projectId,
            Title = GetJsonProperty(mergeRequestJson, "title"),
            Description = GetJsonProperty(mergeRequestJson, "description"),
            State = MapStringToGitLabMergeRequestState(GetJsonProperty(mergeRequestJson, "state")),
            SourceBranch = GetJsonProperty(mergeRequestJson, "source_branch"),
            TargetBranch = GetJsonProperty(mergeRequestJson, "target_branch"),
            Author = GetJsonProperty(mergeRequestJson, "author"),
            WebUrl = GetJsonProperty(mergeRequestJson, "web_url"),
            CreatedAt = GetJsonPropertyDateTime(mergeRequestJson, "created_at"),
            MergedAt = GetJsonPropertyDateTime(mergeRequestJson, "merged_at"),
            ClosedAt = GetJsonPropertyDateTime(mergeRequestJson, "closed_at"),
            WorkInProgress = GetJsonPropertyBool(mergeRequestJson, "work_in_progress"),
            HasConflicts = GetJsonPropertyBool(mergeRequestJson, "has_conflicts")
        };
    }

    private GitLabEnvironment MapToGitLabEnvironment(JsonElement environmentJson, int projectId)
    {
        return new GitLabEnvironment
        {
            Id = 0, // Would be set from database
            GitLabEnvironmentId = GetJsonPropertyInt(environmentJson, "id"),
            GitLabProjectId = projectId,
            Name = GetJsonProperty(environmentJson, "name"),
            ExternalUrl = GetJsonProperty(environmentJson, "external_url"),
            State = GetJsonProperty(environmentJson, "state")
        };
    }

    private GitLabDeployment MapToGitLabDeployment(JsonElement deploymentJson)
    {
        return new GitLabDeployment
        {
            Id = 0, // Would be set from database
            GitLabDeploymentId = GetJsonPropertyInt(deploymentJson, "id"),
            Status = GetJsonProperty(deploymentJson, "status"),
            Ref = GetJsonProperty(deploymentJson, "ref"),
            Sha = GetJsonProperty(deploymentJson, "sha"),
            DeployedBy = GetJsonProperty(deploymentJson, "user"),
            CreatedAt = GetJsonPropertyDateTime(deploymentJson, "created_at"),
            DeployedAt = GetJsonPropertyDateTime(deploymentJson, "deployed_at")
        };
    }

    private GitLabVariable MapToGitLabVariable(JsonElement variableJson, int projectId)
    {
        return new GitLabVariable
        {
            Id = 0, // Would be set from database
            GitLabProjectId = projectId,
            Key = GetJsonProperty(variableJson, "key"),
            Value = GetJsonProperty(variableJson, "value"),
            Masked = GetJsonPropertyBool(variableJson, "masked"),
            Protected = GetJsonPropertyBool(variableJson, "protected"),
            VariableType = GetJsonProperty(variableJson, "variable_type")
        };
    }

    private GitLabWebhook MapToGitLabWebhook(JsonElement webhookJson, int projectId)
    {
        return new GitLabWebhook
        {
            Id = 0, // Would be set from database
            GitLabWebhookId = GetJsonPropertyInt(webhookJson, "id"),
            GitLabProjectId = projectId,
            Url = GetJsonProperty(webhookJson, "url"),
            PushEvents = GetJsonPropertyBool(webhookJson, "push_events"),
            MergeRequestsEvents = GetJsonPropertyBool(webhookJson, "merge_requests_events"),
            PipelineEvents = GetJsonPropertyBool(webhookJson, "pipeline_events"),
            JobEvents = GetJsonPropertyBool(webhookJson, "job_events"),
            DeploymentEvents = GetJsonPropertyBool(webhookJson, "deployment_events"),
            Token = GetJsonProperty(webhookJson, "token"),
            EnableSslVerification = GetJsonPropertyBool(webhookJson, "enable_ssl_verification")
        };
    }

    private GitLabPipelineStatus MapStringToGitLabPipelineStatus(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "created" => GitLabPipelineStatus.Created,
            "waiting_for_resource" => GitLabPipelineStatus.WaitingForResource,
            "preparing" => GitLabPipelineStatus.Preparing,
            "pending" => GitLabPipelineStatus.Pending,
            "running" => GitLabPipelineStatus.Running,
            "success" => GitLabPipelineStatus.Success,
            "failed" => GitLabPipelineStatus.Failed,
            "canceled" => GitLabPipelineStatus.Canceled,
            "skipped" => GitLabPipelineStatus.Skipped,
            "manual" => GitLabPipelineStatus.Manual,
            "scheduled" => GitLabPipelineStatus.Scheduled,
            _ => GitLabPipelineStatus.Created
        };
    }

    private GitLabJobStatus MapStringToGitLabJobStatus(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "created" => GitLabJobStatus.Created,
            "pending" => GitLabJobStatus.Pending,
            "running" => GitLabJobStatus.Running,
            "success" => GitLabJobStatus.Success,
            "failed" => GitLabJobStatus.Failed,
            "canceled" => GitLabJobStatus.Canceled,
            "skipped" => GitLabJobStatus.Skipped,
            "manual" => GitLabJobStatus.Manual,
            "waiting_for_resource" => GitLabJobStatus.WaitingForResource,
            "preparing" => GitLabJobStatus.Preparing,
            _ => GitLabJobStatus.Created
        };
    }

    private GitLabMergeRequestState MapStringToGitLabMergeRequestState(string state)
    {
        return state?.ToLowerInvariant() switch
        {
            "opened" => GitLabMergeRequestState.Opened,
            "closed" => GitLabMergeRequestState.Closed,
            "merged" => GitLabMergeRequestState.Merged,
            _ => GitLabMergeRequestState.Opened
        };
    }

    private string GetJsonProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? (property.GetString() ?? string.Empty) : string.Empty;
    }

    private int GetJsonPropertyInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetInt32() : 0;
    }

    private bool GetJsonPropertyBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.GetBoolean();
    }

    private DateTime GetJsonPropertyDateTime(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.GetString() is string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
                return date;
        }
        return DateTime.MinValue;
    }

    private DateTime? GetJsonPropertyDateTime(JsonElement element, string propertyName, bool nullable)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.GetString() is string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
                return date;
        }
        return null;
    }

    private List<string> GetJsonPropertyArray(JsonElement element, string propertyName)
    {
        var result = new List<string>();
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in property.EnumerateArray())
            {
                if (item.GetString() is string stringValue)
                    result.Add(stringValue);
            }
        }
        return result;
    }

    #endregion
} 