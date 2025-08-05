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
    private readonly string _personalAccessToken;
    private readonly string _defaultBranch;

    public GitLabIntegrationService(
        ILogger<GitLabIntegrationService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _gitLabUrl = _configuration["GitLab:ServerUrl"] ?? "";
        _personalAccessToken = _configuration["GitLab:PersonalAccessToken"] ?? "";
        _defaultBranch = _configuration["GitLab:DefaultBranch"] ?? "main";

        // Configure HTTP client for GitLab API
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _personalAccessToken);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "WorkIntakeSystem/1.0");
    }

    // Repository Management with Real API Calls
    public async Task<GitLabProject> CreateProjectAsync(string name, string description, string visibility = "private")
    {
        try
        {
            _logger.LogInformation("Creating GitLab project {ProjectName} with visibility {Visibility}", name, visibility);

            var url = $"{_gitLabUrl}/api/v4/projects";

            var projectData = new
            {
                name = name,
                description = description,
                visibility = visibility,
                default_branch = _defaultBranch,
                initialize_with_readme = true
            };

            var json = JsonSerializer.Serialize(projectData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var projectResponse = JsonSerializer.Deserialize<GitLabProjectResponse>(responseContent);
                
                if (projectResponse != null)
                {
                    var project = new GitLabProject
                    {
                        GitLabProjectId = projectResponse.Id,
                        Name = projectResponse.Name ?? "",
                        Description = projectResponse.Description ?? "",
                        WebUrl = projectResponse.WebUrl ?? "",
                        Visibility = projectResponse.Visibility ?? "",
                        DefaultBranch = projectResponse.DefaultBranch ?? _defaultBranch
                    };

                    _logger.LogInformation("Successfully created GitLab project {ProjectName} with ID {ProjectId}", name, project.GitLabProjectId);
                    return project;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create GitLab project {ProjectName}. Status: {StatusCode}, Error: {Error}", name, response.StatusCode, errorContent);
            return new GitLabProject();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitLab project {ProjectName}", name);
            return new GitLabProject();
        }
    }

    public async Task<GitLabProject> GetProjectAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab project {ProjectId}", projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var projectResponse = JsonSerializer.Deserialize<GitLabProjectResponse>(responseContent);
                
                if (projectResponse != null)
                {
                    var project = new GitLabProject
                    {
                        GitLabProjectId = projectResponse.Id,
                        Name = projectResponse.Name ?? "",
                        Description = projectResponse.Description ?? "",
                        WebUrl = projectResponse.WebUrl ?? "",
                        Visibility = projectResponse.Visibility ?? "",
                        DefaultBranch = projectResponse.DefaultBranch ?? _defaultBranch
                    };

                    _logger.LogInformation("Successfully retrieved GitLab project {ProjectName} with ID {ProjectId}", project.Name, projectId);
                    return project;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab project {ProjectId}. Status: {StatusCode}, Error: {Error}", projectId, response.StatusCode, errorContent);
            return new GitLabProject();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab project {ProjectId}", projectId);
            return new GitLabProject();
        }
    }

    public async Task<List<GitLabProject>> GetProjectsByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab projects for work request {WorkRequestId}", workRequestId);

            var searchTerm = $"WorkRequest-{workRequestId}";
            var url = $"{_gitLabUrl}/api/v4/projects?search={Uri.EscapeDataString(searchTerm)}&per_page=100";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var projectsResponse = JsonSerializer.Deserialize<List<GitLabProjectResponse>>(responseContent);
                
                var projects = new List<GitLabProject>();
                if (projectsResponse != null)
                {
                    foreach (var projectResponse in projectsResponse)
                    {
                        var project = new GitLabProject
                        {
                            GitLabProjectId = projectResponse.Id,
                            Name = projectResponse.Name ?? "",
                            Description = projectResponse.Description ?? "",
                            WebUrl = projectResponse.WebUrl ?? "",
                            Visibility = projectResponse.Visibility ?? "",
                            DefaultBranch = projectResponse.DefaultBranch ?? _defaultBranch
                        };
                        projects.Add(project);
                    }
                }

                _logger.LogInformation("Retrieved {ProjectCount} GitLab projects for work request {WorkRequestId}", projects.Count, workRequestId);
                return projects;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to search GitLab projects for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new List<GitLabProject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab projects for work request {WorkRequestId}", workRequestId);
            return new List<GitLabProject>();
        }
    }

    public async Task<bool> ArchiveProjectAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Archiving GitLab project {ProjectId}", projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/archive";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully archived GitLab project {ProjectId}", projectId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to archive GitLab project {ProjectId}. Status: {StatusCode}, Error: {Error}", projectId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving GitLab project {ProjectId}", projectId);
            return false;
        }
    }

    // Pipeline Management with Real API Calls
    public async Task<GitLabPipeline> TriggerPipelineAsync(int projectId, string branch, Dictionary<string, string>? variables = null)
    {
        try
        {
            _logger.LogInformation("Triggering GitLab pipeline for project {ProjectId} on branch {Branch}", projectId, branch);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/pipeline";

            var pipelineData = new
            {
                @ref = branch,
                variables = variables?.Select(v => new { key = v.Key, value = v.Value }).ToArray()
            };

            var json = JsonSerializer.Serialize(pipelineData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pipelineResponse = JsonSerializer.Deserialize<GitLabPipelineResponse>(responseContent);
                
                if (pipelineResponse != null)
                {
                    var pipeline = new GitLabPipeline
                    {
                        GitLabPipelineId = pipelineResponse.Id,
                        Status = pipelineResponse.Status switch
                        {
                            "success" => GitLabPipelineStatus.Success,
                            "failed" => GitLabPipelineStatus.Failed,
                            "canceled" => GitLabPipelineStatus.Canceled,
                            "running" => GitLabPipelineStatus.Running,
                            "pending" => GitLabPipelineStatus.Pending,
                            _ => GitLabPipelineStatus.Pending
                        },
                        Ref = pipelineResponse.Ref ?? "",
                        Sha = pipelineResponse.Sha ?? "",
                        WebUrl = pipelineResponse.WebUrl ?? "",
                        StartedAt = DateTime.TryParse(pipelineResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        FinishedAt = DateTime.TryParse(pipelineResponse.UpdatedAt, out var updatedAt) ? updatedAt : null
                    };

                    _logger.LogInformation("Successfully triggered GitLab pipeline {PipelineId} for project {ProjectId}", pipeline.Id, projectId);
                    return pipeline;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to trigger GitLab pipeline for project {ProjectId}. Status: {StatusCode}, Error: {Error}", projectId, response.StatusCode, errorContent);
            return new GitLabPipeline();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering GitLab pipeline for project {ProjectId}", projectId);
            return new GitLabPipeline();
        }
    }

    public async Task<GitLabPipeline> GetPipelineAsync(int projectId, int pipelineId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pipelineResponse = JsonSerializer.Deserialize<GitLabPipelineResponse>(responseContent);
                
                if (pipelineResponse != null)
                {
                    var pipeline = new GitLabPipeline
                    {
                        GitLabPipelineId = pipelineResponse.Id,
                        Status = pipelineResponse.Status switch
                        {
                            "success" => GitLabPipelineStatus.Success,
                            "failed" => GitLabPipelineStatus.Failed,
                            "canceled" => GitLabPipelineStatus.Canceled,
                            "running" => GitLabPipelineStatus.Running,
                            "pending" => GitLabPipelineStatus.Pending,
                            _ => GitLabPipelineStatus.Pending
                        },
                        Ref = pipelineResponse.Ref ?? "",
                        Sha = pipelineResponse.Sha ?? "",
                        WebUrl = pipelineResponse.WebUrl ?? "",
                        StartedAt = DateTime.TryParse(pipelineResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        FinishedAt = DateTime.TryParse(pipelineResponse.UpdatedAt, out var updatedAt) ? updatedAt : null
                    };

                    _logger.LogInformation("Successfully retrieved GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);
                    return pipeline;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab pipeline {PipelineId}. Status: {StatusCode}, Error: {Error}", pipelineId, response.StatusCode, errorContent);
            return new GitLabPipeline();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab pipeline {PipelineId}", pipelineId);
            return new GitLabPipeline();
        }
    }

    public async Task<List<GitLabPipeline>> GetPipelinesAsync(int projectId, string? status = null, int? limit = null)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab pipelines for project {ProjectId}", projectId);

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (limit.HasValue)
                queryParams.Add($"per_page={limit.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines{queryString}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pipelinesResponse = JsonSerializer.Deserialize<List<GitLabPipelineResponse>>(responseContent);
                
                var pipelines = new List<GitLabPipeline>();
                if (pipelinesResponse != null)
                {
                    foreach (var pipelineResponse in pipelinesResponse)
                    {
                        var pipeline = new GitLabPipeline
                        {
                            GitLabPipelineId = pipelineResponse.Id,
                            Status = pipelineResponse.Status switch
                            {
                                "success" => GitLabPipelineStatus.Success,
                                "failed" => GitLabPipelineStatus.Failed,
                                "canceled" => GitLabPipelineStatus.Canceled,
                                "skipped" => GitLabPipelineStatus.Skipped,
                                "running" => GitLabPipelineStatus.Running,
                                "pending" => GitLabPipelineStatus.Pending,
                                _ => GitLabPipelineStatus.Pending
                            },
                            Ref = pipelineResponse.Ref ?? "",
                            Sha = pipelineResponse.Sha ?? "",
                            WebUrl = pipelineResponse.WebUrl ?? "",
                            StartedAt = DateTime.TryParse(pipelineResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                            FinishedAt = DateTime.TryParse(pipelineResponse.UpdatedAt, out var updatedAt) ? updatedAt : null
                        };
                        pipelines.Add(pipeline);
                    }
                }

                _logger.LogInformation("Retrieved {PipelineCount} GitLab pipelines for project {ProjectId}", pipelines.Count, projectId);
                return pipelines;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab pipelines for project {ProjectId}. Status: {StatusCode}, Error: {Error}", projectId, response.StatusCode, errorContent);
            return new List<GitLabPipeline>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab pipelines for project {ProjectId}", projectId);
            return new List<GitLabPipeline>();
        }
    }

    public async Task<bool> CancelPipelineAsync(int projectId, int pipelineId)
    {
        try
        {
            _logger.LogInformation("Canceling GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}/cancel";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully canceled GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to cancel GitLab pipeline {PipelineId}. Status: {StatusCode}, Error: {Error}", pipelineId, response.StatusCode, errorContent);
            return false;
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

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}/retry";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully retried GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retry GitLab pipeline {PipelineId}. Status: {StatusCode}, Error: {Error}", pipelineId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying GitLab pipeline {PipelineId}", pipelineId);
            return false;
        }
    }

    // Job Management with Real API Calls
    public async Task<GitLabJob> GetJobAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab job {JobId} for project {ProjectId}", jobId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jobResponse = JsonSerializer.Deserialize<GitLabJobResponse>(responseContent);
                
                if (jobResponse != null)
                {
                    var job = new GitLabJob
                    {
                        GitLabJobId = jobResponse.Id,
                        Name = jobResponse.Name ?? "",
                        Status = jobResponse.Status switch
                        {
                            "success" => GitLabJobStatus.Success,
                            "failed" => GitLabJobStatus.Failed,
                            "canceled" => GitLabJobStatus.Canceled,
                            "running" => GitLabJobStatus.Running,
                            "pending" => GitLabJobStatus.Pending,
                            _ => GitLabJobStatus.Pending
                        },
                        Stage = jobResponse.Stage ?? "",
                        StartedAt = DateTime.TryParse(jobResponse.StartedAt, out var startedAt) ? startedAt : DateTime.UtcNow,
                        FinishedAt = DateTime.TryParse(jobResponse.FinishedAt, out var finishedAt) ? finishedAt : null,
                        WebUrl = jobResponse.WebUrl ?? ""
                    };

                    _logger.LogInformation("Successfully retrieved GitLab job {JobId} for project {ProjectId}", jobId, projectId);
                    return job;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab job {JobId}. Status: {StatusCode}, Error: {Error}", jobId, response.StatusCode, errorContent);
            return new GitLabJob();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab job {JobId}", jobId);
            return new GitLabJob();
        }
    }

    public async Task<List<GitLabJob>> GetJobsForPipelineAsync(int projectId, int pipelineId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab jobs for pipeline {PipelineId} in project {ProjectId}", pipelineId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/pipelines/{pipelineId}/jobs";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jobsResponse = JsonSerializer.Deserialize<List<GitLabJobResponse>>(responseContent);
                
                var jobs = new List<GitLabJob>();
                if (jobsResponse != null)
                {
                    foreach (var jobResponse in jobsResponse)
                    {
                        var job = new GitLabJob
                        {
                            GitLabJobId = jobResponse.Id,
                            Name = jobResponse.Name ?? "",
                            Status = jobResponse.Status switch
                            {
                                "success" => GitLabJobStatus.Success,
                                "failed" => GitLabJobStatus.Failed,
                                "canceled" => GitLabJobStatus.Canceled,
                                "running" => GitLabJobStatus.Running,
                                "pending" => GitLabJobStatus.Pending,
                                _ => GitLabJobStatus.Pending
                            },
                            Stage = jobResponse.Stage ?? "",
                            StartedAt = DateTime.TryParse(jobResponse.StartedAt, out var startedAt) ? startedAt : DateTime.UtcNow,
                            FinishedAt = DateTime.TryParse(jobResponse.FinishedAt, out var finishedAt) ? finishedAt : null,
                            WebUrl = jobResponse.WebUrl ?? ""
                        };
                        jobs.Add(job);
                    }
                }

                _logger.LogInformation("Retrieved {JobCount} GitLab jobs for pipeline {PipelineId}", jobs.Count, pipelineId);
                return jobs;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab jobs for pipeline {PipelineId}. Status: {StatusCode}, Error: {Error}", pipelineId, response.StatusCode, errorContent);
            return new List<GitLabJob>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab jobs for pipeline {PipelineId}", pipelineId);
            return new List<GitLabJob>();
        }
    }

    public async Task<string> GetJobLogsAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab job logs for job {JobId} in project {ProjectId}", jobId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/trace";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var logs = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved GitLab job logs for job {JobId}", jobId);
                return logs;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab job logs for job {JobId}. Status: {StatusCode}, Error: {Error}", jobId, response.StatusCode, errorContent);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab job logs for job {JobId}", jobId);
            return string.Empty;
        }
    }

    public async Task<byte[]> GetJobArtifactsAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab job artifacts for job {JobId} in project {ProjectId}", jobId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/artifacts";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var artifacts = await response.Content.ReadAsByteArrayAsync();
                _logger.LogInformation("Successfully retrieved GitLab job artifacts for job {JobId}", jobId);
                return artifacts;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab job artifacts for job {JobId}. Status: {StatusCode}, Error: {Error}", jobId, response.StatusCode, errorContent);
            return new byte[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab job artifacts for job {JobId}", jobId);
            return new byte[0];
        }
    }

    public async Task<bool> RetryJobAsync(int projectId, int jobId)
    {
        try
        {
            _logger.LogInformation("Retrying GitLab job {JobId} in project {ProjectId}", jobId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/retry";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully retried GitLab job {JobId} in project {ProjectId}", jobId, projectId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retry GitLab job {JobId}. Status: {StatusCode}, Error: {Error}", jobId, response.StatusCode, errorContent);
            return false;
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

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/jobs/{jobId}/cancel";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully canceled GitLab job {JobId} in project {ProjectId}", jobId, projectId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to cancel GitLab job {JobId}. Status: {StatusCode}, Error: {Error}", jobId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling GitLab job {JobId}", jobId);
            return false;
        }
    }

    // Merge Request Management with Real API Calls
    public async Task<GitLabMergeRequest> CreateMergeRequestAsync(int projectId, string sourceBranch, string targetBranch, string title, string description)
    {
        try
        {
            _logger.LogInformation("Creating GitLab merge request for project {ProjectId}: {Title}", projectId, title);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests";

            var mergeRequestData = new
            {
                source_branch = sourceBranch,
                target_branch = targetBranch,
                title = title,
                description = description
            };

            var json = JsonSerializer.Serialize(mergeRequestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestResponse = JsonSerializer.Deserialize<GitLabMergeRequestResponse>(responseContent);
                
                if (mergeRequestResponse != null)
                {
                    var mergeRequest = new GitLabMergeRequest
                    {
                        GitLabMergeRequestId = mergeRequestResponse.Id,
                        Title = mergeRequestResponse.Title ?? "",
                        Description = mergeRequestResponse.Description ?? "",
                        State = mergeRequestResponse.State switch
                        {
                            "opened" => GitLabMergeRequestState.Opened,
                            "closed" => GitLabMergeRequestState.Closed,
                            "merged" => GitLabMergeRequestState.Merged,
                            _ => GitLabMergeRequestState.Opened
                        },
                        SourceBranch = mergeRequestResponse.SourceBranch ?? "",
                        TargetBranch = mergeRequestResponse.TargetBranch ?? "",
                        WebUrl = mergeRequestResponse.WebUrl ?? "",
                        CreatedAt = DateTime.TryParse(mergeRequestResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow
                    };

                    _logger.LogInformation("Successfully created GitLab merge request {MergeRequestId} for project {ProjectId}", mergeRequest.Id, projectId);
                    return mergeRequest;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create GitLab merge request for project {ProjectId}. Status: {StatusCode}, Error: {Error}", projectId, response.StatusCode, errorContent);
            return new GitLabMergeRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitLab merge request for project {ProjectId}", projectId);
            return new GitLabMergeRequest();
        }
    }

    public async Task<GitLabMergeRequest> GetMergeRequestAsync(int projectId, int mergeRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab merge request {MergeRequestId} for project {ProjectId}", mergeRequestId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests/{mergeRequestId}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestResponse = JsonSerializer.Deserialize<GitLabMergeRequestResponse>(responseContent);
                
                if (mergeRequestResponse != null)
                {
                    var mergeRequest = new GitLabMergeRequest
                    {
                        GitLabMergeRequestId = mergeRequestResponse.Id,
                        Title = mergeRequestResponse.Title ?? "",
                        Description = mergeRequestResponse.Description ?? "",
                        State = mergeRequestResponse.State switch
                        {
                            "opened" => GitLabMergeRequestState.Opened,
                            "closed" => GitLabMergeRequestState.Closed,
                            "merged" => GitLabMergeRequestState.Merged,
                            _ => GitLabMergeRequestState.Opened
                        },
                        SourceBranch = mergeRequestResponse.SourceBranch ?? "",
                        TargetBranch = mergeRequestResponse.TargetBranch ?? "",
                        WebUrl = mergeRequestResponse.WebUrl ?? "",
                        CreatedAt = DateTime.TryParse(mergeRequestResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow
                    };

                    _logger.LogInformation("Successfully retrieved GitLab merge request {MergeRequestId} for project {ProjectId}", mergeRequestId, projectId);
                    return mergeRequest;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab merge request {MergeRequestId}. Status: {StatusCode}, Error: {Error}", mergeRequestId, response.StatusCode, errorContent);
            return new GitLabMergeRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab merge request {MergeRequestId}", mergeRequestId);
            return new GitLabMergeRequest();
        }
    }

    public async Task<List<GitLabMergeRequest>> GetMergeRequestsAsync(int projectId, string? state = null)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab merge requests for project {ProjectId}", projectId);

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(state))
                queryParams.Add($"state={Uri.EscapeDataString(state)}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests{queryString}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestsResponse = JsonSerializer.Deserialize<List<GitLabMergeRequestResponse>>(responseContent);
                
                var mergeRequests = new List<GitLabMergeRequest>();
                if (mergeRequestsResponse != null)
                {
                    foreach (var mergeRequestResponse in mergeRequestsResponse)
                    {
                        var mergeRequest = new GitLabMergeRequest
                        {
                            GitLabMergeRequestId = mergeRequestResponse.Id,
                            Title = mergeRequestResponse.Title ?? "",
                            Description = mergeRequestResponse.Description ?? "",
                            State = mergeRequestResponse.State switch
                            {
                                "opened" => GitLabMergeRequestState.Opened,
                                "closed" => GitLabMergeRequestState.Closed,
                                "merged" => GitLabMergeRequestState.Merged,
                                _ => GitLabMergeRequestState.Opened
                            },
                            SourceBranch = mergeRequestResponse.SourceBranch ?? "",
                            TargetBranch = mergeRequestResponse.TargetBranch ?? "",
                            WebUrl = mergeRequestResponse.WebUrl ?? "",
                            CreatedAt = DateTime.TryParse(mergeRequestResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow
                        };
                        mergeRequests.Add(mergeRequest);
                    }
                }

                _logger.LogInformation("Retrieved {MergeRequestCount} GitLab merge requests for project {ProjectId}", mergeRequests.Count, projectId);
                return mergeRequests;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab merge requests for project {ProjectId}. Status: {StatusCode}, Error: {Error}", projectId, response.StatusCode, errorContent);
            return new List<GitLabMergeRequest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab merge requests for project {ProjectId}", projectId);
            return new List<GitLabMergeRequest>();
        }
    }

    public async Task<bool> ApproveMergeRequestAsync(int projectId, int mergeRequestId)
    {
        try
        {
            _logger.LogInformation("Approving GitLab merge request {MergeRequestId} for project {ProjectId}", mergeRequestId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests/{mergeRequestId}/approve";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully approved GitLab merge request {MergeRequestId} for project {ProjectId}", mergeRequestId, projectId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to approve GitLab merge request {MergeRequestId}. Status: {StatusCode}, Error: {Error}", mergeRequestId, response.StatusCode, errorContent);
            return false;
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
            _logger.LogInformation("Merging GitLab merge request {MergeRequestId} for project {ProjectId}", mergeRequestId, projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/merge_requests/{mergeRequestId}/merge";

            var response = await _httpClient.PutAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mergeRequestResponse = JsonSerializer.Deserialize<GitLabMergeRequestResponse>(responseContent);
                
                if (mergeRequestResponse != null)
                {
                    var mergeRequest = new GitLabMergeRequest
                    {
                        GitLabMergeRequestId = mergeRequestResponse.Id,
                        Title = mergeRequestResponse.Title ?? "",
                        Description = mergeRequestResponse.Description ?? "",
                        State = mergeRequestResponse.State switch
                        {
                            "opened" => GitLabMergeRequestState.Opened,
                            "closed" => GitLabMergeRequestState.Closed,
                            "merged" => GitLabMergeRequestState.Merged,
                            _ => GitLabMergeRequestState.Opened
                        },
                        SourceBranch = mergeRequestResponse.SourceBranch ?? "",
                        TargetBranch = mergeRequestResponse.TargetBranch ?? "",
                        WebUrl = mergeRequestResponse.WebUrl ?? "",
                        CreatedAt = DateTime.TryParse(mergeRequestResponse.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow
                    };

                    _logger.LogInformation("Successfully merged GitLab merge request {MergeRequestId} for project {ProjectId}", mergeRequestId, projectId);
                    return mergeRequest;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to merge GitLab merge request {MergeRequestId}. Status: {StatusCode}, Error: {Error}", mergeRequestId, response.StatusCode, errorContent);
            return new GitLabMergeRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging GitLab merge request {MergeRequestId}", mergeRequestId);
            return new GitLabMergeRequest();
        }
    }

    // Environment & Deployment Management
    public async Task<List<GitLabEnvironment>> GetEnvironmentsAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab environments for project {ProjectId}", projectId);

            var url = $"{_gitLabUrl}/api/v4/projects/{projectId}/environments";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var environmentsResponse = JsonSerializer.Deserialize<List<GitLabEnvironmentResponse>>(responseContent);
                
                var environments = new List<GitLabEnvironment>();
                if (environmentsResponse != null)
                {
                    foreach (var environmentResponse in environmentsResponse)
                    {
                        var environment = new GitLabEnvironment
                        {
                            GitLabEnvironmentId = environmentResponse.Id,
                            Name = environmentResponse.Name ?? "",
                            State = environmentResponse.State ?? "",
                            ExternalUrl = environmentResponse.ExternalUrl ?? ""
                        };
                        environments.Add(environment);
                    }
                }

                _logger.LogInformation("Retrieved {EnvironmentCount} GitLab environments for project {ProjectId}", environments.Count, projectId);
                return environments;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve GitLab environments for project {ProjectId}. Status: {StatusCode}, Error: {Error}", projectId, response.StatusCode, errorContent);
            return new List<GitLabEnvironment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab environments for project {ProjectId}", projectId);
            return new List<GitLabEnvironment>();
        }
    }

    // Response Models for JSON Deserialization
    private class GitLabProjectResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? WebUrl { get; set; }
        public string? Visibility { get; set; }
        public string? DefaultBranch { get; set; }
        public string? CreatedAt { get; set; }
    }

    private class GitLabPipelineResponse
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? Ref { get; set; }
        public string? Sha { get; set; }
        public string? WebUrl { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }

    private class GitLabJobResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? Stage { get; set; }
        public string? CreatedAt { get; set; }
        public string? StartedAt { get; set; }
        public string? FinishedAt { get; set; }
        public double? Duration { get; set; }
        public string? WebUrl { get; set; }
    }

    private class GitLabMergeRequestResponse
    {
        public int Id { get; set; }
        public int Iid { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? State { get; set; }
        public string? SourceBranch { get; set; }
        public string? TargetBranch { get; set; }
        public string? WebUrl { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }

    private class GitLabEnvironmentResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? State { get; set; }
        public string? ExternalUrl { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }

    // Missing method implementations
    public async Task<GitLabDeployment> GetDeploymentAsync(int projectId, int deploymentId)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab deployment {DeploymentId} for project {ProjectId}", deploymentId, projectId);
            // TODO: Implement actual GitLab API call
            return new GitLabDeployment();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab deployment {DeploymentId}", deploymentId);
            return new GitLabDeployment();
        }
    }

    public async Task<List<GitLabDeployment>> GetDeploymentsAsync(int projectId, string? environment = null)
    {
        try
        {
            _logger.LogInformation("Retrieving GitLab deployments for project {ProjectId}", projectId);
            // TODO: Implement actual GitLab API call
            return new List<GitLabDeployment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitLab deployments for project {ProjectId}", projectId);
            return new List<GitLabDeployment>();
        }
    }

    public async Task<bool> CreateDeploymentAsync(int projectId, string environment, string sha, Dictionary<string, string>? variables = null)
    {
        try
        {
            _logger.LogInformation("Creating GitLab deployment for project {ProjectId} in environment {Environment}", projectId, environment);
            // TODO: Implement actual GitLab API call
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitLab deployment for project {ProjectId}", projectId);
            return false;
        }
    }

    public async Task<string> GetCiConfigAsync(int projectId, string? gitRef = null)
    {
        try
        {
            _logger.LogInformation("Retrieving CI config for project {ProjectId}", projectId);
            // TODO: Implement actual GitLab API call
            return "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CI config for project {ProjectId}", projectId);
            return "";
        }
    }

    public async Task<bool> UpdateCiConfigAsync(int projectId, string ciConfigContent, string commitMessage)
    {
        try
        {
            _logger.LogInformation("Updating CI config for project {ProjectId}", projectId);
            // TODO: Implement actual GitLab API call
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating CI config for project {ProjectId}", projectId);
            return false;
        }
    }

    public async Task<GitLabCiLint> LintCiConfigAsync(string ciConfigContent)
    {
        try
        {
            _logger.LogInformation("Linting CI config content");
            // TODO: Implement actual GitLab API call
            return new GitLabCiLint();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linting CI config");
            return new GitLabCiLint();
        }
    }

    public async Task<bool> CreateVariableAsync(int projectId, string key, string value, bool masked = false, bool protectedVar = false)
    {
        try
        {
            _logger.LogInformation("Creating variable {Key} for project {ProjectId}", key, projectId);
            // TODO: Implement actual GitLab API call
            return true;
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
            _logger.LogInformation("Retrieving variables for project {ProjectId}", projectId);
            // TODO: Implement actual GitLab API call
            return new List<GitLabVariable>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variables for project {ProjectId}", projectId);
            return new List<GitLabVariable>();
        }
    }

    public async Task<bool> UpdateVariableAsync(int projectId, string key, string value, bool? masked = null, bool? protectedVar = null)
    {
        try
        {
            _logger.LogInformation("Updating variable {Key} for project {ProjectId}", key, projectId);
            // TODO: Implement actual GitLab API call
            return true;
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
            _logger.LogInformation("Deleting variable {Key} for project {ProjectId}", key, projectId);
            // TODO: Implement actual GitLab API call
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting variable {Key} for project {ProjectId}", key, projectId);
            return false;
        }
    }

    public async Task<GitLabWebhook> CreateWebhookAsync(int projectId, string url, List<string> events)
    {
        try
        {
            _logger.LogInformation("Creating webhook for project {ProjectId}", projectId);
            // TODO: Implement actual GitLab API call
            return new GitLabWebhook();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating webhook for project {ProjectId}", projectId);
            return new GitLabWebhook();
        }
    }

    public async Task<List<GitLabWebhook>> GetWebhooksAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Retrieving webhooks for project {ProjectId}", projectId);
            // TODO: Implement actual GitLab API call
            return new List<GitLabWebhook>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhooks for project {ProjectId}", projectId);
            return new List<GitLabWebhook>();
        }
    }

    public async Task<bool> DeleteWebhookAsync(int projectId, int hookId)
    {
        try
        {
            _logger.LogInformation("Deleting webhook {HookId} for project {ProjectId}", hookId, projectId);
            // TODO: Implement actual GitLab API call
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook {HookId} for project {ProjectId}", hookId, projectId);
            return false;
        }
    }

    public async Task<bool> LinkWorkRequestToProjectAsync(int workRequestId, int projectId)
    {
        try
        {
            _logger.LogInformation("Linking work request {WorkRequestId} to project {ProjectId}", workRequestId, projectId);
            // TODO: Implement actual linking logic
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking work request {WorkRequestId} to project {ProjectId}", workRequestId, projectId);
            return false;
        }
    }

    public async Task<List<GitLabPipeline>> GetPipelinesForWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving pipelines for work request {WorkRequestId}", workRequestId);
            // TODO: Implement actual GitLab API call
            return new List<GitLabPipeline>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pipelines for work request {WorkRequestId}", workRequestId);
            return new List<GitLabPipeline>();
        }
    }

    public async Task<PipelineStatusSummary> GetWorkRequestPipelineStatusAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving pipeline status for work request {WorkRequestId}", workRequestId);
            // TODO: Implement actual GitLab API call
            return new PipelineStatusSummary();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pipeline status for work request {WorkRequestId}", workRequestId);
            return new PipelineStatusSummary();
        }
    }

    public async Task<bool> CreateWorkRequestBranchAsync(int workRequestId, int projectId, string branchName)
    {
        try
        {
            _logger.LogInformation("Creating branch {BranchName} for work request {WorkRequestId} in project {ProjectId}", branchName, workRequestId, projectId);
            // TODO: Implement actual GitLab API call
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch {BranchName} for work request {WorkRequestId}", branchName, workRequestId);
            return false;
        }
    }
} 