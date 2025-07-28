using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IJenkinsIntegrationService
{
    // Pipeline Management
    Task<string> CreatePipelineAsync(string jobName, string gitRepositoryUrl, string jenkinsfilePath, int workRequestId);
    Task<bool> UpdatePipelineConfigurationAsync(string jobName, JenkinsPipelineConfig config);
    Task<bool> DeletePipelineAsync(string jobName);
    Task<JenkinsPipeline> GetPipelineAsync(string jobName);
    Task<List<JenkinsPipeline>> GetPipelinesByWorkRequestAsync(int workRequestId);
    
    // Build Management
    Task<int> TriggerBuildAsync(string jobName, Dictionary<string, string>? parameters = null);
    Task<JenkinsBuild> GetBuildAsync(string jobName, int buildNumber);
    Task<List<JenkinsBuild>> GetBuildsAsync(string jobName, int? limit = null);
    Task<string> GetBuildLogsAsync(string jobName, int buildNumber);
    Task<bool> AbortBuildAsync(string jobName, int buildNumber);
    
    // Deployment Management
    Task<string> CreateDeploymentPipelineAsync(string jobName, DeploymentEnvironment environment, string artifactPath);
    Task<bool> TriggerDeploymentAsync(string deploymentJobName, string environment, string buildArtifact);
    Task<List<JenkinsDeployment>> GetDeploymentsAsync(string environment);
    Task<JenkinsDeployment> GetDeploymentStatusAsync(string deploymentJobName, int buildNumber);
    
    // Monitoring & Status
    Task<JenkinsServerInfo> GetServerInfoAsync();
    Task<List<JenkinsNode>> GetNodesAsync();
    Task<JenkinsSystemHealth> GetSystemHealthAsync();
    Task<List<JenkinsQueue>> GetBuildQueueAsync();
    
    // Integration with Work Requests
    Task<bool> LinkWorkRequestToPipelineAsync(int workRequestId, string jobName);
    Task<List<JenkinsBuild>> GetBuildsForWorkRequestAsync(int workRequestId);
    Task<BuildStatusSummary> GetWorkRequestBuildStatusAsync(int workRequestId);
}

public class JenkinsPipelineConfig
{
    public string JobName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GitRepositoryUrl { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public string JenkinsfilePath { get; set; } = "Jenkinsfile";
    public Dictionary<string, string> Parameters { get; set; } = new();
    public List<string> Triggers { get; set; } = new();
    public bool Enabled { get; set; } = true;
}

public enum DeploymentEnvironment
{
    Development,
    Testing,
    Staging,
    Production
}

public enum BuildStatus
{
    NotBuilt,
    Building,
    Success,
    Failed,
    Aborted,
    Unstable
}

public enum DeploymentStatus
{
    NotDeployed,
    Deploying,
    Deployed,
    Failed,
    RolledBack
} 