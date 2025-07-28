using WorkIntakeSystem.Core.Interfaces;

namespace WorkIntakeSystem.Core.Entities;

// Jenkins Entities
public class JenkinsPipeline : BaseEntity
{
    public string JobName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GitRepositoryUrl { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public string JenkinsfilePath { get; set; } = "Jenkinsfile";
    public bool Enabled { get; set; } = true;
    public int WorkRequestId { get; set; }
    public WorkRequest? WorkRequest { get; set; }
    public List<JenkinsBuild> Builds { get; set; } = new();
    public DateTime LastBuildDate { get; set; }
    public BuildStatus LastBuildStatus { get; set; }
}

public class JenkinsBuild : BaseEntity
{
    public int BuildNumber { get; set; }
    public string JobName { get; set; } = string.Empty;
    public int JenkinsPipelineId { get; set; }
    public JenkinsPipeline? Pipeline { get; set; }
    public BuildStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration => EndTime - StartTime;
    public string BuildUrl { get; set; } = string.Empty;
    public string GitCommitHash { get; set; } = string.Empty;
    public string GitCommitMessage { get; set; } = string.Empty;
    public string TriggeredBy { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public List<JenkinsTestResult> TestResults { get; set; } = new();
    public List<JenkinsBuildArtifact> Artifacts { get; set; } = new();
}

public class JenkinsDeployment : BaseEntity
{
    public string DeploymentJobName { get; set; } = string.Empty;
    public int BuildNumber { get; set; }
    public DeploymentEnvironment Environment { get; set; }
    public DeploymentStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration => EndTime - StartTime;
    public string ArtifactPath { get; set; } = string.Empty;
    public string DeployedBy { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DeploymentUrl { get; set; } = string.Empty;
    public int JenkinsPipelineId { get; set; }
    public JenkinsPipeline? Pipeline { get; set; }
}

public class JenkinsTestResult : BaseEntity
{
    public int JenkinsBuildId { get; set; }
    public JenkinsBuild? Build { get; set; }
    public string TestSuite { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // PASS, FAIL, SKIP
    public TimeSpan Duration { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
}

public class JenkinsBuildArtifact : BaseEntity
{
    public int JenkinsBuildId { get; set; }
    public JenkinsBuild? Build { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class JenkinsServerInfo
{
    public string Version { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public int NumExecutors { get; set; }
    public List<string> Plugins { get; set; } = new();
    public bool QuietingDown { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class JenkinsNode
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int NumExecutors { get; set; }
    public bool Online { get; set; }
    public bool TemporarilyOffline { get; set; }
    public string OfflineCause { get; set; } = string.Empty;
    public Dictionary<string, object> MonitorData { get; set; } = new();
}

public class JenkinsSystemHealth
{
    public bool Healthy { get; set; }
    public List<string> Issues { get; set; } = new();
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public int ActiveBuilds { get; set; }
    public int QueuedBuilds { get; set; }
}

public class JenkinsQueue
{
    public int Id { get; set; }
    public string Task { get; set; } = string.Empty;
    public string Why { get; set; } = string.Empty;
    public bool Stuck { get; set; }
    public DateTime InQueueSince { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}

public class BuildStatusSummary
{
    public int WorkRequestId { get; set; }
    public int TotalBuilds { get; set; }
    public int SuccessfulBuilds { get; set; }
    public int FailedBuilds { get; set; }
    public int RunningBuilds { get; set; }
    public DateTime? LastBuildDate { get; set; }
    public BuildStatus? LastBuildStatus { get; set; }
    public double SuccessRate => TotalBuilds > 0 ? (double)SuccessfulBuilds / TotalBuilds * 100 : 0;
}

// GitLab Entities
public class GitLabProject : BaseEntity
{
    public int GitLabProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Visibility { get; set; } = "private";
    public string WebUrl { get; set; } = string.Empty; 
    public string SshUrl { get; set; } = string.Empty;
    public string HttpUrl { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = "main";
    public bool Archived { get; set; }
    public int WorkRequestId { get; set; }
    public WorkRequest? WorkRequest { get; set; }
    public List<GitLabPipeline> Pipelines { get; set; } = new();
    public List<GitLabMergeRequest> MergeRequests { get; set; } = new();
}

public class GitLabPipeline : BaseEntity
{
    public int GitLabPipelineId { get; set; }
    public int GitLabProjectId { get; set; }
    public GitLabProject? Project { get; set; }
    public GitLabPipelineStatus Status { get; set; }
    public string Ref { get; set; } = string.Empty; // branch or tag
    public string Sha { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? Duration => FinishedAt - StartedAt;
    public string TriggeredBy { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
    public List<GitLabJob> Jobs { get; set; } = new();
}

public class GitLabJob : BaseEntity
{
    public int GitLabJobId { get; set; }
    public int GitLabPipelineId { get; set; }
    public GitLabPipeline? Pipeline { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public GitLabJobStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? Duration => FinishedAt - StartedAt;
    public string WebUrl { get; set; } = string.Empty;
    public string RunnerDescription { get; set; } = string.Empty;
    public bool AllowFailure { get; set; }
    public List<GitLabJobArtifact> Artifacts { get; set; } = new();
}

public class GitLabJobArtifact : BaseEntity
{
    public int GitLabJobId { get; set; }
    public GitLabJob? Job { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class GitLabMergeRequest : BaseEntity
{
    public int GitLabMergeRequestId { get; set; }
    public int GitLabProjectId { get; set; }
    public GitLabProject? Project { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GitLabMergeRequestState State { get; set; }
    public string SourceBranch { get; set; } = string.Empty;
    public string TargetBranch { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public List<string> Reviewers { get; set; } = new();
    public string WebUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool WorkInProgress { get; set; }
    public bool HasConflicts { get; set; }
}

public class GitLabEnvironment : BaseEntity
{
    public int GitLabEnvironmentId { get; set; }
    public int GitLabProjectId { get; set; }
    public GitLabProject? Project { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExternalUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // available, stopped
    public List<GitLabDeployment> Deployments { get; set; } = new();
}

public class GitLabDeployment : BaseEntity
{
    public int GitLabDeploymentId { get; set; }
    public int GitLabEnvironmentId { get; set; }
    public GitLabEnvironment? Environment { get; set; }
    public string Status { get; set; } = string.Empty; // created, running, success, failed, canceled
    public string Ref { get; set; } = string.Empty;
    public string Sha { get; set; } = string.Empty;
    public string DeployedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeployedAt { get; set; }
}

public class GitLabVariable : BaseEntity
{
    public int GitLabProjectId { get; set; }
    public GitLabProject? Project { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Masked { get; set; }
    public bool Protected { get; set; }
    public string VariableType { get; set; } = "env_var"; // env_var, file
}

public class GitLabWebhook : BaseEntity
{
    public int GitLabWebhookId { get; set; }
    public int GitLabProjectId { get; set; }
    public GitLabProject? Project { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public bool PushEvents { get; set; }
    public bool MergeRequestsEvents { get; set; }
    public bool PipelineEvents { get; set; }
    public bool JobEvents { get; set; }
    public bool DeploymentEvents { get; set; }
    public string Token { get; set; } = string.Empty;
    public bool EnableSslVerification { get; set; } = true;
}

public class GitLabCiLint
{
    public bool Valid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class PipelineStatusSummary
{
    public int WorkRequestId { get; set; }
    public int TotalPipelines { get; set; }
    public int SuccessfulPipelines { get; set; }
    public int FailedPipelines { get; set; }
    public int RunningPipelines { get; set; }
    public DateTime? LastPipelineDate { get; set; }
    public GitLabPipelineStatus? LastPipelineStatus { get; set; }
    public double SuccessRate => TotalPipelines > 0 ? (double)SuccessfulPipelines / TotalPipelines * 100 : 0;
}

// CI/CD Integration Summary
public class CiCdIntegrationSummary : BaseEntity
{
    public int WorkRequestId { get; set; }
    public WorkRequest? WorkRequest { get; set; }
    public List<JenkinsPipeline> JenkinsPipelines { get; set; } = new();
    public List<GitLabProject> GitLabProjects { get; set; } = new();
    public DateTime LastActivity { get; set; }
    public string OverallStatus { get; set; } = string.Empty; // healthy, warning, error
    public Dictionary<string, object> Metrics { get; set; } = new();
} 