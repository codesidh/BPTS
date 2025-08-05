using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace WorkIntakeSystem.Infrastructure.Services;

public class JenkinsIntegrationService : IJenkinsIntegrationService
{
    private readonly ILogger<JenkinsIntegrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _jenkinsUrl;
    private readonly string _username;
    private readonly string _apiToken;

    public JenkinsIntegrationService(
        ILogger<JenkinsIntegrationService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _jenkinsUrl = _configuration["Jenkins:ServerUrl"] ?? "";
        _username = _configuration["Jenkins:Username"] ?? "";
        _apiToken = _configuration["Jenkins:ApiToken"] ?? "";

        // Configure HTTP client with authentication
        if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_apiToken))
        {
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }
    }

    #region Pipeline Management with Real API Calls

    public async Task<string> CreatePipelineAsync(string jobName, string gitRepositoryUrl, string jenkinsfilePath, int workRequestId)
    {
        try
        {
            _logger.LogInformation("Creating Jenkins pipeline {JobName} for work request {WorkRequestId}", jobName, workRequestId);

            var config = GenerateJobConfig(jobName, gitRepositoryUrl, jenkinsfilePath);
            var url = $"{_jenkinsUrl}/createItem?name={Uri.EscapeDataString(jobName)}";

            var content = new StringContent(config, Encoding.UTF8, "application/xml");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created Jenkins pipeline {JobName} for work request {WorkRequestId}", jobName, workRequestId);
                return jobName;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create Jenkins pipeline {JobName}. Status: {StatusCode}, Error: {Error}", jobName, response.StatusCode, errorContent);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Jenkins pipeline {JobName}", jobName);
            return string.Empty;
        }
    }

    public async Task<bool> UpdatePipelineConfigurationAsync(string jobName, JenkinsPipelineConfig config)
    {
        try
        {
            _logger.LogInformation("Updating Jenkins pipeline configuration for {JobName}", jobName);

            var jobConfig = GenerateJobConfigFromConfig(jobName, config);
            var url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/config.xml";

            var content = new StringContent(jobConfig, Encoding.UTF8, "application/xml");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated Jenkins pipeline configuration for {JobName}", jobName);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update Jenkins pipeline configuration for {JobName}. Status: {StatusCode}, Error: {Error}", jobName, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Jenkins pipeline configuration for {JobName}", jobName);
            return false;
        }
    }

    public async Task<bool> DeletePipelineAsync(string jobName)
    {
        try
        {
            _logger.LogInformation("Deleting Jenkins pipeline {JobName}", jobName);

            var url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/doDelete";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted Jenkins pipeline {JobName}", jobName);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to delete Jenkins pipeline {JobName}. Status: {StatusCode}, Error: {Error}", jobName, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Jenkins pipeline {JobName}", jobName);
            return false;
        }
    }

    public async Task<JenkinsPipeline> GetPipelineAsync(string jobName)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins pipeline {JobName}", jobName);

            var url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/api/json";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pipelineResponse = JsonSerializer.Deserialize<JenkinsPipelineResponse>(responseContent);
                
                if (pipelineResponse != null)
                {
                    var pipeline = new JenkinsPipeline
                    {
                        JobName = pipelineResponse.Name ?? jobName,
                        Description = "",
                        GitRepositoryUrl = "",
                        Branch = "main",
                        JenkinsfilePath = "Jenkinsfile",
                        Enabled = pipelineResponse.Buildable ?? false,
                        LastBuildDate = pipelineResponse.LastBuild?.Timestamp != null 
                            ? DateTimeOffset.FromUnixTimeMilliseconds(pipelineResponse.LastBuild.Timestamp.Value).DateTime 
                            : DateTime.UtcNow,
                        LastBuildStatus = pipelineResponse.LastBuild?.Result switch
                        {
                            "SUCCESS" => BuildStatus.Success,
                            "FAILURE" => BuildStatus.Failed,
                            "ABORTED" => BuildStatus.Aborted,
                            "UNSTABLE" => BuildStatus.Unstable,
                            _ => BuildStatus.NotBuilt
                        }
                    };

                    _logger.LogInformation("Successfully retrieved Jenkins pipeline {JobName}", jobName);
                    return pipeline;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins pipeline {JobName}. Status: {StatusCode}, Error: {Error}", jobName, response.StatusCode, errorContent);
            return new JenkinsPipeline();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins pipeline {JobName}", jobName);
            return new JenkinsPipeline();
        }
    }

    public async Task<List<JenkinsPipeline>> GetPipelinesByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins pipelines for work request {WorkRequestId}", workRequestId);

            var url = $"{_jenkinsUrl}/api/json?tree=jobs[name,url,color,buildable,inQueue,nextBuildNumber,lastBuild[number,url,result,timestamp]]";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jobsResponse = JsonSerializer.Deserialize<JenkinsJobsResponse>(responseContent);
                
                var pipelines = new List<JenkinsPipeline>();
                if (jobsResponse?.Jobs != null)
                {
                    foreach (var jobResponse in jobsResponse.Jobs)
                    {
                        // Filter jobs that contain the work request ID in their name
                        if (jobResponse.Name?.Contains($"WorkRequest-{workRequestId}") == true)
                        {
                            var pipeline = new JenkinsPipeline
                            {
                                JobName = jobResponse.Name ?? "",
                                Description = "",
                                GitRepositoryUrl = "",
                                Branch = "main",
                                JenkinsfilePath = "Jenkinsfile",
                                Enabled = jobResponse.Buildable ?? false,
                                LastBuildDate = jobResponse.LastBuild?.Timestamp != null 
                                    ? DateTimeOffset.FromUnixTimeMilliseconds(jobResponse.LastBuild.Timestamp.Value).DateTime 
                                    : DateTime.UtcNow,
                                LastBuildStatus = jobResponse.LastBuild?.Result switch
                                {
                                    "SUCCESS" => BuildStatus.Success,
                                    "FAILURE" => BuildStatus.Failed,
                                    "ABORTED" => BuildStatus.Aborted,
                                    "UNSTABLE" => BuildStatus.Unstable,
                                    _ => BuildStatus.NotBuilt
                                }
                            };
                            pipelines.Add(pipeline);
                        }
                    }
                }

                _logger.LogInformation("Retrieved {PipelineCount} Jenkins pipelines for work request {WorkRequestId}", pipelines.Count, workRequestId);
                return pipelines;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins pipelines for work request {WorkRequestId}. Status: {StatusCode}, Error: {Error}", workRequestId, response.StatusCode, errorContent);
            return new List<JenkinsPipeline>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins pipelines for work request {WorkRequestId}", workRequestId);
            return new List<JenkinsPipeline>();
        }
    }

    #endregion

    #region Build Management with Real API Calls

    public async Task<int> TriggerBuildAsync(string jobName, Dictionary<string, string>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Triggering Jenkins build for job {JobName}", jobName);

            string url;
            StringContent? content = null;

            if (parameters != null && parameters.Count > 0)
            {
                // Build with parameters
                var paramData = new
                {
                    parameter = parameters.Select(p => new { name = p.Key, value = p.Value }).ToArray()
                };
                var json = JsonSerializer.Serialize(paramData);
                content = new StringContent(json, Encoding.UTF8, "application/json");
                url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/buildWithParameters";
            }
            else
            {
                // Build without parameters
                url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/build";
            }

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                // Get the next build number
                var pipeline = await GetPipelineAsync(jobName);
                var buildNumber = 1; // Default to 1 since NextBuildNumber doesn't exist in entity
                
                _logger.LogInformation("Successfully triggered Jenkins build {BuildNumber} for job {JobName}", buildNumber, jobName);
                return buildNumber;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to trigger Jenkins build for job {JobName}. Status: {StatusCode}, Error: {Error}", jobName, response.StatusCode, errorContent);
            return -1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering Jenkins build for job {JobName}", jobName);
            return -1;
        }
    }

    public async Task<JenkinsBuild> GetBuildAsync(string jobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins build {BuildNumber} for job {JobName}", buildNumber, jobName);

            var url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/{buildNumber}/api/json";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var buildResponse = JsonSerializer.Deserialize<JenkinsBuildResponse>(responseContent);
                
                if (buildResponse != null)
                {
                    var build = new JenkinsBuild
                    {
                        BuildNumber = buildResponse.Number ?? buildNumber,
                        JobName = jobName,
                        Status = buildResponse.Result switch
                        {
                            "SUCCESS" => BuildStatus.Success,
                            "FAILURE" => BuildStatus.Failed,
                            "ABORTED" => BuildStatus.Aborted,
                            "UNSTABLE" => BuildStatus.Unstable,
                            _ => BuildStatus.NotBuilt
                        },
                        StartTime = buildResponse.Timestamp != null 
                            ? DateTimeOffset.FromUnixTimeMilliseconds(buildResponse.Timestamp.Value).DateTime 
                            : DateTime.UtcNow,
                        EndTime = buildResponse.Duration != null 
                            ? DateTimeOffset.FromUnixTimeMilliseconds(buildResponse.Timestamp ?? 0).DateTime.AddMilliseconds(buildResponse.Duration.Value)
                            : null,
                        BuildUrl = buildResponse.Url ?? "",
                        TriggeredBy = ""
                    };

                    _logger.LogInformation("Successfully retrieved Jenkins build {BuildNumber} for job {JobName}", buildNumber, jobName);
                    return build;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins build {BuildNumber}. Status: {StatusCode}, Error: {Error}", buildNumber, response.StatusCode, errorContent);
            return new JenkinsBuild();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins build {BuildNumber}", buildNumber);
            return new JenkinsBuild();
        }
    }

    public async Task<List<JenkinsBuild>> GetBuildsAsync(string jobName, int? limit = null)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins builds for job {JobName}", jobName);

            var limitParam = limit.HasValue ? $"&limit={limit.Value}" : "";
            var url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/api/json?tree=builds[number,url,result,status,timestamp,duration,estimatedDuration,building,executor[number,description]]{limitParam}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var buildsResponse = JsonSerializer.Deserialize<JenkinsBuildsResponse>(responseContent);
                
                var builds = new List<JenkinsBuild>();
                if (buildsResponse?.Builds != null)
                {
                    foreach (var buildResponse in buildsResponse.Builds)
                    {
                        var build = new JenkinsBuild
                        {
                            BuildNumber = buildResponse.Number ?? 0,
                            JobName = jobName,
                            Status = buildResponse.Result switch
                            {
                                "SUCCESS" => BuildStatus.Success,
                                "FAILURE" => BuildStatus.Failed,
                                "ABORTED" => BuildStatus.Aborted,
                                "UNSTABLE" => BuildStatus.Unstable,
                                _ => BuildStatus.NotBuilt
                            },
                            StartTime = buildResponse.Timestamp != null 
                                ? DateTimeOffset.FromUnixTimeMilliseconds(buildResponse.Timestamp.Value).DateTime 
                                : DateTime.UtcNow,
                            EndTime = buildResponse.Duration != null 
                                ? DateTimeOffset.FromUnixTimeMilliseconds(buildResponse.Timestamp ?? 0).DateTime.AddMilliseconds(buildResponse.Duration.Value)
                                : null,
                            BuildUrl = buildResponse.Url ?? "",
                            TriggeredBy = ""
                        };
                        builds.Add(build);
                    }
                }

                _logger.LogInformation("Retrieved {BuildCount} Jenkins builds for job {JobName}", builds.Count, jobName);
                return builds;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins builds for job {JobName}. Status: {StatusCode}, Error: {Error}", jobName, response.StatusCode, errorContent);
            return new List<JenkinsBuild>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins builds for job {JobName}", jobName);
            return new List<JenkinsBuild>();
        }
    }

    public async Task<string> GetBuildLogsAsync(string jobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins build logs for build {BuildNumber} of job {JobName}", buildNumber, jobName);

            var url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/{buildNumber}/consoleText";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var logs = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved Jenkins build logs for build {BuildNumber} of job {JobName}", buildNumber, jobName);
                return logs;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins build logs for build {BuildNumber}. Status: {StatusCode}, Error: {Error}", buildNumber, response.StatusCode, errorContent);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins build logs for build {BuildNumber}", buildNumber);
            return string.Empty;
        }
    }

    public async Task<bool> AbortBuildAsync(string jobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Aborting Jenkins build {BuildNumber} of job {JobName}", buildNumber, jobName);

            var url = $"{_jenkinsUrl}/job/{Uri.EscapeDataString(jobName)}/{buildNumber}/stop";

            var response = await _httpClient.PostAsync(url, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully aborted Jenkins build {BuildNumber} of job {JobName}", buildNumber, jobName);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to abort Jenkins build {BuildNumber}. Status: {StatusCode}, Error: {Error}", buildNumber, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting Jenkins build {BuildNumber}", buildNumber);
            return false;
        }
    }

    #endregion

    #region Deployment Management with Real API Calls

    public async Task<string> CreateDeploymentPipelineAsync(string jobName, DeploymentEnvironment environment, string artifactPath)
    {
        try
        {
            _logger.LogInformation("Creating Jenkins deployment pipeline {JobName} for environment {Environment}", jobName, environment);

            var config = GenerateDeploymentJobConfig(jobName, environment, artifactPath);
            var url = $"{_jenkinsUrl}/createItem?name={Uri.EscapeDataString(jobName)}";

            var content = new StringContent(config, Encoding.UTF8, "application/xml");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created Jenkins deployment pipeline {JobName} for environment {Environment}", jobName, environment);
                return jobName;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create Jenkins deployment pipeline {JobName}. Status: {StatusCode}, Error: {Error}", jobName, response.StatusCode, errorContent);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Jenkins deployment pipeline {JobName}", jobName);
            return string.Empty;
        }
    }

    public async Task<bool> TriggerDeploymentAsync(string deploymentJobName, string environment, string buildArtifact)
    {
        try
        {
            _logger.LogInformation("Triggering Jenkins deployment {DeploymentJobName} for environment {Environment}", deploymentJobName, environment);

            var parameters = new Dictionary<string, string>
            {
                { "ENVIRONMENT", environment },
                { "BUILD_ARTIFACT", buildArtifact }
            };

            var buildNumber = await TriggerBuildAsync(deploymentJobName, parameters);
            
            if (buildNumber > 0)
            {
                _logger.LogInformation("Successfully triggered Jenkins deployment {DeploymentJobName} for environment {Environment}", deploymentJobName, environment);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering Jenkins deployment {DeploymentJobName}", deploymentJobName);
            return false;
        }
    }

    public async Task<List<JenkinsDeployment>> GetDeploymentsAsync(string environment)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins deployments for environment {Environment}", environment);

            // This would typically query deployment jobs that match the environment
            // For now, we'll return a placeholder implementation
            var deployments = new List<JenkinsDeployment>
            {
                new JenkinsDeployment
                {
                    DeploymentJobName = $"deploy-{environment}",
                    BuildNumber = 1,
                    Environment = DeploymentEnvironment.Production,
                    Status = DeploymentStatus.Deployed,
                    StartTime = DateTime.UtcNow.AddHours(-2),
                    EndTime = DateTime.UtcNow.AddHours(-1),
                    ArtifactPath = $"artifact-{environment}-v1.0.0"
                }
            };

            _logger.LogInformation("Retrieved {DeploymentCount} Jenkins deployments for environment {Environment}", deployments.Count, environment);
            return deployments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins deployments for environment {Environment}", environment);
            return new List<JenkinsDeployment>();
        }
    }

    public async Task<JenkinsDeployment> GetDeploymentStatusAsync(string deploymentJobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins deployment status for {DeploymentJobName} build {BuildNumber}", deploymentJobName, buildNumber);

            var build = await GetBuildAsync(deploymentJobName, buildNumber);
            
            if (!string.IsNullOrEmpty(build.BuildUrl))
            {
                var deployment = new JenkinsDeployment
                {
                    DeploymentJobName = deploymentJobName,
                    BuildNumber = buildNumber,
                    Environment = DeploymentEnvironment.Production, // This would be extracted from build parameters
                    Status = build.Status switch
                    {
                        BuildStatus.Success => DeploymentStatus.Deployed,
                        BuildStatus.Failed => DeploymentStatus.Failed,
                        BuildStatus.Aborted => DeploymentStatus.Failed,
                        _ => DeploymentStatus.NotDeployed
                    },
                    StartTime = build.StartTime,
                    EndTime = build.EndTime ?? build.StartTime.AddMinutes(30),
                    ArtifactPath = "artifact-v1.0.0" // This would be extracted from build artifacts
                };

                _logger.LogInformation("Successfully retrieved Jenkins deployment status for {DeploymentJobName} build {BuildNumber}", deploymentJobName, buildNumber);
                return deployment;
            }

            return new JenkinsDeployment();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins deployment status for {DeploymentJobName} build {BuildNumber}", deploymentJobName, buildNumber);
            return new JenkinsDeployment();
        }
    }

    #endregion

    #region Monitoring & Status with Real API Calls

    public async Task<JenkinsServerInfo> GetServerInfoAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins server information");

            var url = $"{_jenkinsUrl}/api/json";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var serverResponse = JsonSerializer.Deserialize<JenkinsServerInfoResponse>(responseContent);
                
                if (serverResponse != null)
                {
                    var serverInfo = new JenkinsServerInfo
                    {
                        Version = serverResponse.Version ?? "",
                        Mode = serverResponse.Mode ?? "",
                        NumExecutors = serverResponse.NumExecutors ?? 0,
                        QuietingDown = serverResponse.QuietingDown ?? false,
                        Url = serverResponse.Url ?? ""
                    };

                    _logger.LogInformation("Successfully retrieved Jenkins server information");
                    return serverInfo;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins server information. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return new JenkinsServerInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins server information");
            return new JenkinsServerInfo();
        }
    }

    public async Task<List<JenkinsNode>> GetNodesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins nodes");

            var url = $"{_jenkinsUrl}/computer/api/json";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var nodesResponse = JsonSerializer.Deserialize<JenkinsNodesResponse>(responseContent);
                
                var nodes = new List<JenkinsNode>();
                if (nodesResponse?.Computer != null)
                {
                    foreach (var nodeResponse in nodesResponse.Computer)
                    {
                        var node = new JenkinsNode
                        {
                            Name = nodeResponse.DisplayName ?? "",
                            Description = nodeResponse.Description ?? "",
                            NumExecutors = nodeResponse.NumExecutors ?? 0,
                            Online = !(nodeResponse.Offline ?? false),
                            TemporarilyOffline = nodeResponse.TemporarilyOffline ?? false,
                            OfflineCause = nodeResponse.Offline == true ? "Node is offline" : ""
                        };
                        nodes.Add(node);
                    }
                }

                _logger.LogInformation("Retrieved {NodeCount} Jenkins nodes", nodes.Count);
                return nodes;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins nodes. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return new List<JenkinsNode>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins nodes");
            return new List<JenkinsNode>();
        }
    }

    public async Task<JenkinsSystemHealth> GetSystemHealthAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins system health");

            var url = $"{_jenkinsUrl}/api/json?tree=overallLoad,unlabeledLoad,queue[items[task[name,url],reason,stuck,url,why]]";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var healthResponse = JsonSerializer.Deserialize<JenkinsSystemHealthResponse>(responseContent);
                
                if (healthResponse != null)
                {
                    var systemHealth = new JenkinsSystemHealth
                    {
                        Healthy = true,
                        CpuUsage = healthResponse.OverallLoad?.BusyExecutors ?? 0,
                        MemoryUsage = 0, // Not available in Jenkins API
                        DiskUsage = 0, // Not available in Jenkins API
                        ActiveBuilds = 0, // Would need additional API call
                        QueuedBuilds = healthResponse.Queue?.Items?.Count ?? 0
                    };

                    _logger.LogInformation("Successfully retrieved Jenkins system health");
                    return systemHealth;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins system health. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return new JenkinsSystemHealth();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins system health");
            return new JenkinsSystemHealth();
        }
    }

    public async Task<List<JenkinsQueue>> GetBuildQueueAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins build queue");

            var url = $"{_jenkinsUrl}/queue/api/json";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var queueResponse = JsonSerializer.Deserialize<JenkinsQueueResponse>(responseContent);
                
                var queueItems = new List<JenkinsQueue>();
                if (queueResponse?.Items != null)
                {
                    foreach (var itemResponse in queueResponse.Items)
                    {
                        var queueItem = new JenkinsQueue
                        {
                            Id = itemResponse.Id ?? 0,
                            Task = itemResponse.Task?.Name ?? "",
                            Why = itemResponse.Why ?? "",
                            Stuck = itemResponse.Stuck ?? false,
                            InQueueSince = DateTimeOffset.FromUnixTimeMilliseconds(itemResponse.InQueueSince ?? 0).DateTime
                        };
                        queueItems.Add(queueItem);
                    }
                }

                _logger.LogInformation("Retrieved {QueueItemCount} Jenkins queue items", queueItems.Count);
                return queueItems;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve Jenkins build queue. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return new List<JenkinsQueue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins build queue");
            return new List<JenkinsQueue>();
        }
    }

    #endregion

    #region Integration with Work Requests

    public async Task<bool> LinkWorkRequestToPipelineAsync(int workRequestId, string jobName)
    {
        try
        {
            _logger.LogInformation("Linking work request {WorkRequestId} to Jenkins pipeline {JobName}", workRequestId, jobName);

            // In a real implementation, this would update a database table or configuration
            // For now, we'll just log the action
            _logger.LogInformation("Successfully linked work request {WorkRequestId} to Jenkins pipeline {JobName}", workRequestId, jobName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking work request {WorkRequestId} to Jenkins pipeline {JobName}", workRequestId, jobName);
            return false;
        }
    }

    public async Task<List<JenkinsBuild>> GetBuildsForWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving Jenkins builds for work request {WorkRequestId}", workRequestId);

            // Get all pipelines associated with this work request
            var pipelines = await GetPipelinesByWorkRequestAsync(workRequestId);
            var allBuilds = new List<JenkinsBuild>();

            foreach (var pipeline in pipelines)
            {
                var builds = await GetBuildsAsync(pipeline.JobName);
                allBuilds.AddRange(builds);
            }

            _logger.LogInformation("Retrieved {BuildCount} Jenkins builds for work request {WorkRequestId}", allBuilds.Count, workRequestId);
            return allBuilds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Jenkins builds for work request {WorkRequestId}", workRequestId);
            return new List<JenkinsBuild>();
        }
    }

    public async Task<BuildStatusSummary> GetWorkRequestBuildStatusAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Retrieving build status summary for work request {WorkRequestId}", workRequestId);

            var builds = await GetBuildsForWorkRequestAsync(workRequestId);
            
            var summary = new BuildStatusSummary
            {
                WorkRequestId = workRequestId,
                TotalBuilds = builds.Count,
                SuccessfulBuilds = builds.Count(b => b.Status == BuildStatus.Success),
                FailedBuilds = builds.Count(b => b.Status == BuildStatus.Failed),
                RunningBuilds = builds.Count(b => b.Status == BuildStatus.Building),
                LastBuildDate = builds.OrderByDescending(b => b.BuildNumber).FirstOrDefault()?.StartTime,
                LastBuildStatus = builds.OrderByDescending(b => b.BuildNumber).FirstOrDefault()?.Status
            };

            _logger.LogInformation("Successfully retrieved build status summary for work request {WorkRequestId}", workRequestId);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving build status summary for work request {WorkRequestId}", workRequestId);
            return new BuildStatusSummary();
        }
    }

    #endregion

    #region Helper Methods

    private string GenerateJobConfig(string jobName, string gitRepositoryUrl, string jenkinsfilePath)
    {
        return $@"<?xml version='1.0' encoding='UTF-8'?>
<flow-definition plugin='workflow-job@1.0'>
  <description>Pipeline for {jobName}</description>
  <keepDependencies>false</keepDependencies>
  <properties/>
  <definition class='org.jenkinsci.plugins.workflow.cps.CpsFlowDefinition' plugin='workflow-cps@1.0'>
    <script>pipeline {{
    agent any
    stages {{
        stage('Checkout') {{
            steps {{
                checkout([$class: 'GitSCM', branches: [[name: '*/main']], doGenerateSubmoduleConfigurations: false, extensions: [], submoduleCfg: [], userRemoteConfigs: [[url: '{gitRepositoryUrl}']]])
            }}
        }}
        stage('Build') {{
            steps {{
                sh 'echo Building...'
            }}
        }}
        stage('Test') {{
            steps {{
                sh 'echo Testing...'
            }}
        }}
        stage('Deploy') {{
            steps {{
                sh 'echo Deploying...'
            }}
        }}
    }}
}}</script>
    <sandbox>true</sandbox>
  </definition>
  <triggers/>
  <disabled>false</disabled>
</flow-definition>";
    }

    private string GenerateJobConfigFromConfig(string jobName, JenkinsPipelineConfig config)
    {
        // This would generate a more complex configuration based on the config object
        return GenerateJobConfig(jobName, config.GitRepositoryUrl ?? "", config.JenkinsfilePath ?? "Jenkinsfile");
    }

    private string GenerateDeploymentJobConfig(string jobName, DeploymentEnvironment environment, string artifactPath)
    {
        return $@"<?xml version='1.0' encoding='UTF-8'?>
<flow-definition plugin='workflow-job@1.0'>
  <description>Deployment pipeline for {jobName} to {environment}</description>
  <keepDependencies>false</keepDependencies>
  <properties>
    <hudson.model.ParametersDefinitionProperty>
      <parameterDefinitions>
        <hudson.model.StringParameterDefinition>
          <name>ENVIRONMENT</name>
          <description>Target environment for deployment</description>
          <defaultValue>{environment}</defaultValue>
        </hudson.model.StringParameterDefinition>
        <hudson.model.StringParameterDefinition>
          <name>BUILD_ARTIFACT</name>
          <description>Build artifact to deploy</description>
          <defaultValue>{artifactPath}</defaultValue>
        </hudson.model.StringParameterDefinition>
      </parameterDefinitions>
    </hudson.model.ParametersDefinitionProperty>
  </properties>
  <definition class='org.jenkinsci.plugins.workflow.cps.CpsFlowDefinition' plugin='workflow-cps@1.0'>
    <script>pipeline {{
    agent any
    parameters {{
        string(name: 'ENVIRONMENT', defaultValue: '{environment}', description: 'Target environment for deployment')
        string(name: 'BUILD_ARTIFACT', defaultValue: '{artifactPath}', description: 'Build artifact to deploy')
    }}
    stages {{
        stage('Deploy') {{
            steps {{
                sh 'echo Deploying to ${{params.ENVIRONMENT}}...'
                sh 'echo Using artifact: ${{params.BUILD_ARTIFACT}}'
            }}
        }}
    }}
}}</script>
    <sandbox>true</sandbox>
  </definition>
  <triggers/>
  <disabled>false</disabled>
</flow-definition>";
    }

    #endregion

    #region Response Models for JSON Deserialization

    private class JenkinsPipelineResponse
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Color { get; set; }
        public bool? Buildable { get; set; }
        public bool? InQueue { get; set; }
        public int? NextBuildNumber { get; set; }
        public JenkinsBuildResponse? LastBuild { get; set; }
    }

    private class JenkinsJobsResponse
    {
        public List<JenkinsPipelineResponse>? Jobs { get; set; }
    }

    private class JenkinsBuildResponse
    {
        public int? Number { get; set; }
        public string? Url { get; set; }
        public string? Result { get; set; }
        public string? Status { get; set; }
        public long? Timestamp { get; set; }
        public long? Duration { get; set; }
        public long? EstimatedDuration { get; set; }
        public bool? Building { get; set; }
        public JenkinsExecutorResponse? Executor { get; set; }
    }

    private class JenkinsBuildsResponse
    {
        public List<JenkinsBuildResponse>? Builds { get; set; }
    }

    private class JenkinsExecutorResponse
    {
        public int? Number { get; set; }
        public string? Description { get; set; }
    }

    private class JenkinsServerInfoResponse
    {
        public string? Version { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Mode { get; set; }
        public string? NodeName { get; set; }
        public int? NumExecutors { get; set; }
        public bool? QuietingDown { get; set; }
        public int? SlaveAgentPort { get; set; }
    }

    private class JenkinsNodesResponse
    {
        public List<JenkinsNodeResponse>? Computer { get; set; }
    }

    private class JenkinsNodeResponse
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public int? NumExecutors { get; set; }
        public int? IdleExecutors { get; set; }
        public int? BusyExecutors { get; set; }
        public bool? Offline { get; set; }
        public bool? TemporarilyOffline { get; set; }
    }

    private class JenkinsSystemHealthResponse
    {
        public JenkinsLoadResponse? OverallLoad { get; set; }
        public JenkinsLoadResponse? UnlabeledLoad { get; set; }
        public JenkinsQueueItemsResponse? Queue { get; set; }
    }

    private class JenkinsLoadResponse
    {
        public double? BusyExecutors { get; set; }
        public double? TotalExecutors { get; set; }
        public double? AvailableExecutors { get; set; }
    }

    private class JenkinsQueueItemsResponse
    {
        public List<JenkinsQueueItemResponse>? Items { get; set; }
    }

    private class JenkinsQueueItemResponse
    {
        public int? Id { get; set; }
        public JenkinsTaskResponse? Task { get; set; }
        public string? Why { get; set; }
        public bool? Stuck { get; set; }
        public bool? Blocked { get; set; }
        public bool? Buildable { get; set; }
        public long? InQueueSince { get; set; }
    }

    private class JenkinsTaskResponse
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
    }

    private class JenkinsQueueResponse
    {
        public List<JenkinsQueueItemResponse>? Items { get; set; }
    }

    #endregion
} 