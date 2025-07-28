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

    #region Pipeline Management

    public async Task<string> CreatePipelineAsync(string jobName, string gitRepositoryUrl, string jenkinsfilePath, int workRequestId)
    {
        try
        {
            _logger.LogInformation("Creating Jenkins pipeline {JobName} for work request {WorkRequestId}", jobName, workRequestId);

            var config = GenerateJobConfig(jobName, gitRepositoryUrl, jenkinsfilePath);
            var content = new StringContent(config, Encoding.UTF8, "application/xml");
            
            var response = await _httpClient.PostAsync($"{_jenkinsUrl}/createItem?name={jobName}", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created Jenkins pipeline {JobName}", jobName);
                return jobName;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create Jenkins pipeline {JobName}: {Error}", jobName, error);
                throw new Exception($"Failed to create Jenkins pipeline: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Jenkins pipeline {JobName}", jobName);
            throw;
        }
    }

    public async Task<bool> UpdatePipelineConfigurationAsync(string jobName, JenkinsPipelineConfig config)
    {
        try
        {
            _logger.LogInformation("Updating Jenkins pipeline configuration for {JobName}", jobName);

            var configXml = GenerateJobConfig(jobName, config.GitRepositoryUrl, config.JenkinsfilePath);
            var content = new StringContent(configXml, Encoding.UTF8, "application/xml");
            
            var response = await _httpClient.PostAsync($"{_jenkinsUrl}/job/{jobName}/config.xml", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated Jenkins pipeline configuration for {JobName}", jobName);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update Jenkins pipeline configuration for {JobName}: {Error}", jobName, error);
                return false;
            }
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

            var response = await _httpClient.PostAsync($"{_jenkinsUrl}/job/{jobName}/doDelete", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted Jenkins pipeline {JobName}", jobName);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete Jenkins pipeline {JobName}: {Error}", jobName, error);
                return false;
            }
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
            _logger.LogInformation("Getting Jenkins pipeline details for {JobName}", jobName);

            var response = await _httpClient.GetAsync($"{_jenkinsUrl}/job/{jobName}/api/json");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var jobData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                return MapToJenkinsPipeline(jobData, jobName);
            }
            else
            {
                _logger.LogError("Failed to get Jenkins pipeline {JobName}", jobName);
                throw new Exception($"Failed to get Jenkins pipeline: {jobName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins pipeline {JobName}", jobName);
            throw;
        }
    }

    public async Task<List<JenkinsPipeline>> GetPipelinesByWorkRequestAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Getting Jenkins pipelines for work request {WorkRequestId}", workRequestId);

            // This would typically query a database to find pipelines linked to the work request
            // For now, return empty list as this requires database integration
            _logger.LogWarning("Database integration required to fetch pipelines by work request ID");
            return new List<JenkinsPipeline>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins pipelines for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    #endregion

    #region Build Management

    public async Task<int> TriggerBuildAsync(string jobName, Dictionary<string, string>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Triggering build for Jenkins job {JobName}", jobName);

            string endpoint;
            HttpContent? content = null;

            if (parameters != null && parameters.Any())
            {
                endpoint = $"{_jenkinsUrl}/job/{jobName}/buildWithParameters";
                var formContent = new List<KeyValuePair<string, string>>();
                foreach (var param in parameters)
                {
                    formContent.Add(new KeyValuePair<string, string>(param.Key, param.Value));
                }
                content = new FormUrlEncodedContent(formContent);
            }
            else
            {
                endpoint = $"{_jenkinsUrl}/job/{jobName}/build";
            }

            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                // Get the next build number
                var nextBuildNumber = await GetNextBuildNumberAsync(jobName);
                _logger.LogInformation("Successfully triggered build #{BuildNumber} for Jenkins job {JobName}", nextBuildNumber, jobName);
                return nextBuildNumber;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to trigger build for Jenkins job {JobName}: {Error}", jobName, error);
                throw new Exception($"Failed to trigger build: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering build for Jenkins job {JobName}", jobName);
            throw;
        }
    }

    public async Task<JenkinsBuild> GetBuildAsync(string jobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Getting build details for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);

            var response = await _httpClient.GetAsync($"{_jenkinsUrl}/job/{jobName}/{buildNumber}/api/json");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var buildData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                return MapToJenkinsBuild(buildData, jobName);
            }
            else
            {
                _logger.LogError("Failed to get build details for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);
                throw new Exception($"Failed to get build details for {jobName} #{buildNumber}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting build details for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);
            throw;
        }
    }

    public async Task<List<JenkinsBuild>> GetBuildsAsync(string jobName, int? limit = null)
    {
        try
        {
            _logger.LogInformation("Getting builds for Jenkins job {JobName}", jobName);

            var url = $"{_jenkinsUrl}/job/{jobName}/api/json?tree=builds[number,status,timestamp,duration,url,result]";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var jobData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                var builds = new List<JenkinsBuild>();
                if (jobData.TryGetProperty("builds", out var buildsArray))
                {
                    foreach (var buildElement in buildsArray.EnumerateArray())
                    {
                        builds.Add(MapToJenkinsBuild(buildElement, jobName));
                    }
                }

                return limit.HasValue ? builds.Take(limit.Value).ToList() : builds;
            }
            else
            {
                _logger.LogError("Failed to get builds for Jenkins job {JobName}", jobName);
                throw new Exception($"Failed to get builds for {jobName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting builds for Jenkins job {JobName}", jobName);
            throw;
        }
    }

    public async Task<string> GetBuildLogsAsync(string jobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Getting build logs for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);

            var response = await _httpClient.GetAsync($"{_jenkinsUrl}/job/{jobName}/{buildNumber}/consoleText");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger.LogError("Failed to get build logs for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);
                return $"Failed to retrieve build logs for {jobName} #{buildNumber}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting build logs for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);
            return $"Error retrieving build logs: {ex.Message}";
        }
    }

    public async Task<bool> AbortBuildAsync(string jobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Aborting build for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);

            var response = await _httpClient.PostAsync($"{_jenkinsUrl}/job/{jobName}/{buildNumber}/stop", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully aborted build for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to abort build for Jenkins job {JobName} build #{BuildNumber}: {Error}", jobName, buildNumber, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting build for Jenkins job {JobName} build #{BuildNumber}", jobName, buildNumber);
            return false;
        }
    }

    #endregion

    #region Deployment Management

    public async Task<string> CreateDeploymentPipelineAsync(string jobName, DeploymentEnvironment environment, string artifactPath)
    {
        try
        {
            _logger.LogInformation("Creating deployment pipeline {JobName} for {Environment}", jobName, environment);

            var deploymentJobName = $"{jobName}-deploy-{environment.ToString().ToLower()}";
            var config = GenerateDeploymentJobConfig(deploymentJobName, environment, artifactPath);
            var content = new StringContent(config, Encoding.UTF8, "application/xml");
            
            var response = await _httpClient.PostAsync($"{_jenkinsUrl}/createItem?name={deploymentJobName}", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created deployment pipeline {DeploymentJobName}", deploymentJobName);
                return deploymentJobName;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create deployment pipeline {DeploymentJobName}: {Error}", deploymentJobName, error);
                throw new Exception($"Failed to create deployment pipeline: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deployment pipeline {JobName} for {Environment}", jobName, environment);
            throw;
        }
    }

    public async Task<bool> TriggerDeploymentAsync(string deploymentJobName, string environment, string buildArtifact)
    {
        try
        {
            _logger.LogInformation("Triggering deployment {DeploymentJobName} to {Environment}", deploymentJobName, environment);

            var parameters = new Dictionary<string, string>
            {
                { "ENVIRONMENT", environment },
                { "BUILD_ARTIFACT", buildArtifact }
            };

            var buildNumber = await TriggerBuildAsync(deploymentJobName, parameters);
            return buildNumber > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering deployment {DeploymentJobName} to {Environment}", deploymentJobName, environment);
            return false;
        }
    }

    public async Task<List<JenkinsDeployment>> GetDeploymentsAsync(string environment)
    {
        try
        {
            _logger.LogInformation("Getting deployments for environment {Environment}", environment);

            // This would typically query a database to find deployments for the environment
            // For now, return empty list as this requires database integration
            _logger.LogWarning("Database integration required to fetch deployments by environment");
            return new List<JenkinsDeployment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deployments for environment {Environment}", environment);
            throw;
        }
    }

    public async Task<JenkinsDeployment> GetDeploymentStatusAsync(string deploymentJobName, int buildNumber)
    {
        try
        {
            _logger.LogInformation("Getting deployment status for {DeploymentJobName} build #{BuildNumber}", deploymentJobName, buildNumber);

            var build = await GetBuildAsync(deploymentJobName, buildNumber);
            
            return new JenkinsDeployment
            {
                Id = 0, // Would be set from database
                DeploymentJobName = deploymentJobName,
                BuildNumber = buildNumber,
                Status = MapBuildStatusToDeploymentStatus(build.Status),
                StartTime = build.StartTime,
                EndTime = build.EndTime,
                Version = build.GitCommitHash,
                DeployedBy = build.TriggeredBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deployment status for {DeploymentJobName} build #{BuildNumber}", deploymentJobName, buildNumber);
            throw;
        }
    }

    #endregion

    #region Monitoring & Status

    public async Task<JenkinsServerInfo> GetServerInfoAsync()
    {
        try
        {
            _logger.LogInformation("Getting Jenkins server information");

            var response = await _httpClient.GetAsync($"{_jenkinsUrl}/api/json");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var serverData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                return new JenkinsServerInfo
                {
                    Version = GetJsonProperty(serverData, "version"),
                    Mode = GetJsonProperty(serverData, "mode"),
                    NumExecutors = GetJsonPropertyInt(serverData, "numExecutors"),
                    QuietingDown = GetJsonPropertyBool(serverData, "quietingDown"),
                    Url = _jenkinsUrl
                };
            }
            else
            {
                _logger.LogError("Failed to get Jenkins server information");
                throw new Exception("Failed to get Jenkins server information");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins server information");
            throw;
        }
    }

    public async Task<List<JenkinsNode>> GetNodesAsync()
    {
        try
        {
            _logger.LogInformation("Getting Jenkins nodes");

            var response = await _httpClient.GetAsync($"{_jenkinsUrl}/computer/api/json");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var nodesData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                var nodes = new List<JenkinsNode>();
                if (nodesData.TryGetProperty("computer", out var computersArray))
                {
                    foreach (var computerElement in computersArray.EnumerateArray())
                    {
                        nodes.Add(new JenkinsNode
                        {
                            Name = GetJsonProperty(computerElement, "displayName"),
                            Description = GetJsonProperty(computerElement, "description"),
                            NumExecutors = GetJsonPropertyInt(computerElement, "numExecutors"),
                            Online = !GetJsonPropertyBool(computerElement, "offline"),
                            TemporarilyOffline = GetJsonPropertyBool(computerElement, "temporarilyOffline"),
                            OfflineCause = GetJsonProperty(computerElement, "offlineCause")
                        });
                    }
                }

                return nodes;
            }
            else
            {
                _logger.LogError("Failed to get Jenkins nodes");
                throw new Exception("Failed to get Jenkins nodes");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins nodes");
            throw;
        }
    }

    public async Task<JenkinsSystemHealth> GetSystemHealthAsync()
    {
        try
        {
            _logger.LogInformation("Getting Jenkins system health");

            var serverInfo = await GetServerInfoAsync();
            var nodes = await GetNodesAsync();
            var queue = await GetBuildQueueAsync();

            return new JenkinsSystemHealth
            {
                Healthy = nodes.All(n => n.Online),
                Issues = nodes.Where(n => !n.Online).Select(n => $"Node {n.Name} is offline").ToList(),
                ActiveBuilds = 0, // Would need additional API call to get accurate count
                QueuedBuilds = queue.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins system health");
            throw;
        }
    }

    public async Task<List<JenkinsQueue>> GetBuildQueueAsync()
    {
        try
        {
            _logger.LogInformation("Getting Jenkins build queue");

            var response = await _httpClient.GetAsync($"{_jenkinsUrl}/queue/api/json");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var queueData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                var queue = new List<JenkinsQueue>();
                if (queueData.TryGetProperty("items", out var itemsArray))
                {
                    foreach (var itemElement in itemsArray.EnumerateArray())
                    {
                        queue.Add(new JenkinsQueue
                        {
                            Id = GetJsonPropertyInt(itemElement, "id"),
                            Task = GetJsonProperty(itemElement, "task"),
                            Why = GetJsonProperty(itemElement, "why"),
                            Stuck = GetJsonPropertyBool(itemElement, "stuck"),
                            InQueueSince = DateTimeOffset.FromUnixTimeMilliseconds(GetJsonPropertyLong(itemElement, "inQueueSince")).DateTime
                        });
                    }
                }

                return queue;
            }
            else
            {
                _logger.LogError("Failed to get Jenkins build queue");
                throw new Exception("Failed to get Jenkins build queue");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins build queue");
            throw;
        }
    }

    #endregion

    #region Integration with Work Requests

    public async Task<bool> LinkWorkRequestToPipelineAsync(int workRequestId, string jobName)
    {
        try
        {
            _logger.LogInformation("Linking work request {WorkRequestId} to Jenkins pipeline {JobName}", workRequestId, jobName);

            // This would typically update a database to link the work request to the pipeline
            // For now, just log and return success
            _logger.LogWarning("Database integration required to link work request to pipeline");
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
            _logger.LogInformation("Getting Jenkins builds for work request {WorkRequestId}", workRequestId);

            // This would typically query a database to find builds for the work request
            // For now, return empty list as this requires database integration
            _logger.LogWarning("Database integration required to fetch builds by work request ID");
            return new List<JenkinsBuild>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins builds for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    public async Task<BuildStatusSummary> GetWorkRequestBuildStatusAsync(int workRequestId)
    {
        try
        {
            _logger.LogInformation("Getting build status summary for work request {WorkRequestId}", workRequestId);

            var builds = await GetBuildsForWorkRequestAsync(workRequestId);
            
            return new BuildStatusSummary
            {
                WorkRequestId = workRequestId,
                TotalBuilds = builds.Count,
                SuccessfulBuilds = builds.Count(b => b.Status == BuildStatus.Success),
                FailedBuilds = builds.Count(b => b.Status == BuildStatus.Failed),
                RunningBuilds = builds.Count(b => b.Status == BuildStatus.Building),
                LastBuildDate = builds.OrderByDescending(b => b.StartTime).FirstOrDefault()?.StartTime,
                LastBuildStatus = builds.OrderByDescending(b => b.StartTime).FirstOrDefault()?.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting build status summary for work request {WorkRequestId}", workRequestId);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<int> GetNextBuildNumberAsync(string jobName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_jenkinsUrl}/job/{jobName}/api/json?tree=nextBuildNumber");
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var jobData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                return GetJsonPropertyInt(jobData, "nextBuildNumber");
            }
            return 1;
        }
        catch
        {
            return 1;
        }
    }

    private string GenerateJobConfig(string jobName, string gitRepositoryUrl, string jenkinsfilePath)
    {
        return $@"<?xml version='1.1' encoding='UTF-8'?>
<flow-definition plugin=""workflow-job@2.40"">
  <actions/>
  <description>Pipeline for {jobName}</description>
  <keepDependencies>false</keepDependencies>
  <properties>
    <hudson.plugins.jira.JiraProjectProperty plugin=""jira@3.7""/>
  </properties>
  <definition class=""org.jenkinsci.plugins.workflow.cps.CpsScmFlowDefinition"" plugin=""workflow-cps@2.92"">
    <scm class=""hudson.plugins.git.GitSCM"" plugin=""git@4.8.3"">
      <configVersion>2</configVersion>
      <userRemoteConfigs>
        <hudson.plugins.git.UserRemoteConfig>
          <url>{gitRepositoryUrl}</url>
        </hudson.plugins.git.UserRemoteConfig>
      </userRemoteConfigs>
      <branches>
        <hudson.plugins.git.BranchSpec>
          <name>*/main</name>
        </hudson.plugins.git.BranchSpec>
      </branches>
      <doGenerateSubmoduleConfigurations>false</doGenerateSubmoduleConfigurations>
      <submoduleCfg class=""empty-list""/>
      <extensions/>
    </scm>
    <scriptPath>{jenkinsfilePath}</scriptPath>
    <lightweight>true</lightweight>
  </definition>
  <triggers/>
  <disabled>false</disabled>
</flow-definition>";
    }

    private string GenerateDeploymentJobConfig(string jobName, DeploymentEnvironment environment, string artifactPath)
    {
        return $@"<?xml version='1.1' encoding='UTF-8'?>
<project>
  <actions/>
  <description>Deployment pipeline for {environment}</description>
  <keepDependencies>false</keepDependencies>
  <properties>
    <hudson.model.ParametersDefinitionProperty>
      <parameterDefinitions>
        <hudson.model.StringParameterDefinition>
          <name>ENVIRONMENT</name>
          <defaultValue>{environment.ToString().ToLower()}</defaultValue>
          <trim>false</trim>
        </hudson.model.StringParameterDefinition>
        <hudson.model.StringParameterDefinition>
          <name>BUILD_ARTIFACT</name>
          <defaultValue>{artifactPath}</defaultValue>
          <trim>false</trim>
        </hudson.model.StringParameterDefinition>
      </parameterDefinitions>
    </hudson.model.ParametersDefinitionProperty>
  </properties>
  <scm class=""hudson.scm.NullSCM""/>
  <builders>
    <hudson.tasks.Shell>
      <command>echo ""Deploying to $ENVIRONMENT""
echo ""Artifact: $BUILD_ARTIFACT""
# Add your deployment scripts here</command>
    </hudson.tasks.Shell>
  </builders>
  <publishers/>
  <buildWrappers/>
</project>";
    }

    private JenkinsPipeline MapToJenkinsPipeline(JsonElement jobData, string jobName)
    {
        return new JenkinsPipeline
        {
            Id = 0, // Would be set from database
            JobName = jobName,
            Description = GetJsonProperty(jobData, "description"),
            Enabled = !GetJsonPropertyBool(jobData, "disabled"),
            LastBuildDate = GetLastBuildDate(jobData),
            LastBuildStatus = GetLastBuildStatus(jobData)
        };
    }

    private JenkinsBuild MapToJenkinsBuild(JsonElement buildData, string jobName)
    {
        return new JenkinsBuild
        {
            Id = 0, // Would be set from database
            BuildNumber = GetJsonPropertyInt(buildData, "number"),
            JobName = jobName,
            Status = MapStringToBuildStatus(GetJsonProperty(buildData, "result")),
            StartTime = DateTimeOffset.FromUnixTimeMilliseconds(GetJsonPropertyLong(buildData, "timestamp")).DateTime,
            BuildUrl = GetJsonProperty(buildData, "url"),
            TriggeredBy = GetTriggeredBy(buildData)
        };
    }

    private BuildStatus MapStringToBuildStatus(string result)
    {
        return result?.ToUpperInvariant() switch
        {
            "SUCCESS" => BuildStatus.Success,
            "FAILURE" => BuildStatus.Failed,
            "ABORTED" => BuildStatus.Aborted,
            "UNSTABLE" => BuildStatus.Unstable,
            null => BuildStatus.Building,
            _ => BuildStatus.NotBuilt
        };
    }

    private DeploymentStatus MapBuildStatusToDeploymentStatus(BuildStatus buildStatus)
    {
        return buildStatus switch
        {
            BuildStatus.Success => DeploymentStatus.Deployed,
            BuildStatus.Failed => DeploymentStatus.Failed,
            BuildStatus.Building => DeploymentStatus.Deploying,
            BuildStatus.Aborted => DeploymentStatus.Failed,
            _ => DeploymentStatus.NotDeployed
        };
    }

    private DateTime GetLastBuildDate(JsonElement jobData)
    {
        if (jobData.TryGetProperty("lastBuild", out var lastBuild) &&
            lastBuild.TryGetProperty("timestamp", out var timestamp))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.GetInt64()).DateTime;
        }
        return DateTime.MinValue;
    }

    private BuildStatus GetLastBuildStatus(JsonElement jobData)
    {
        if (jobData.TryGetProperty("lastBuild", out var lastBuild) &&
            lastBuild.TryGetProperty("result", out var result))
        {
            return MapStringToBuildStatus(result.GetString());
        }
        return BuildStatus.NotBuilt;
    }

    private string GetTriggeredBy(JsonElement buildData)
    {
        // This would parse the causes array to determine who triggered the build
        return "system"; // Placeholder
    }

    private string GetJsonProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? (property.GetString() ?? string.Empty) : string.Empty;
    }

    private int GetJsonPropertyInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetInt32() : 0;
    }

    private long GetJsonPropertyLong(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetInt64() : 0;
    }

    private bool GetJsonPropertyBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.GetBoolean();
    }

    #endregion
} 