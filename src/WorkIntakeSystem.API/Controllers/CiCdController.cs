using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CiCdController : ControllerBase
{
    private readonly IJenkinsIntegrationService _jenkinsService;
    private readonly IGitLabIntegrationService _gitLabService;
    private readonly ILogger<CiCdController> _logger;

    public CiCdController(
        IJenkinsIntegrationService jenkinsService,
        IGitLabIntegrationService gitLabService,
        ILogger<CiCdController> logger)
    {
        _jenkinsService = jenkinsService;
        _gitLabService = gitLabService;
        _logger = logger;
    }

    #region Jenkins Pipeline Management

    [HttpPost("jenkins/pipelines")]
    public async Task<ActionResult<string>> CreateJenkinsPipeline([FromBody] CreateJenkinsPipelineRequest request)
    {
        try
        {
            var pipelineId = await _jenkinsService.CreatePipelineAsync(
                request.JobName, 
                request.GitRepositoryUrl, 
                request.JenkinsfilePath, 
                request.WorkRequestId);
            
            return Ok(new { PipelineId = pipelineId, JobName = request.JobName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Jenkins pipeline {JobName}", request.JobName);
            return StatusCode(500, new { Error = "Failed to create Jenkins pipeline", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/pipelines/{jobName}")]
    public async Task<ActionResult<JenkinsPipeline>> GetJenkinsPipeline(string jobName)
    {
        try
        {
            var pipeline = await _jenkinsService.GetPipelineAsync(jobName);
            return Ok(pipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins pipeline {JobName}", jobName);
            return NotFound(new { Error = "Jenkins pipeline not found", JobName = jobName });
        }
    }

    [HttpPut("jenkins/pipelines/{jobName}")]
    public async Task<ActionResult> UpdateJenkinsPipeline(string jobName, [FromBody] JenkinsPipelineConfig config)
    {
        try
        {
            var success = await _jenkinsService.UpdatePipelineConfigurationAsync(jobName, config);
            if (success)
                return Ok(new { Message = "Pipeline configuration updated successfully" });
            else
                return BadRequest(new { Error = "Failed to update pipeline configuration" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Jenkins pipeline {JobName}", jobName);
            return StatusCode(500, new { Error = "Failed to update Jenkins pipeline", Details = ex.Message });
        }
    }

    [HttpDelete("jenkins/pipelines/{jobName}")]
    public async Task<ActionResult> DeleteJenkinsPipeline(string jobName)
    {
        try
        {
            var success = await _jenkinsService.DeletePipelineAsync(jobName);
            if (success)
                return Ok(new { Message = "Pipeline deleted successfully" });
            else
                return BadRequest(new { Error = "Failed to delete pipeline" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Jenkins pipeline {JobName}", jobName);
            return StatusCode(500, new { Error = "Failed to delete Jenkins pipeline", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/work-requests/{workRequestId}/pipelines")]
    public async Task<ActionResult<List<JenkinsPipeline>>> GetJenkinsPipelinesByWorkRequest(int workRequestId)
    {
        try
        {
            var pipelines = await _jenkinsService.GetPipelinesByWorkRequestAsync(workRequestId);
            return Ok(pipelines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins pipelines for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Error = "Failed to get Jenkins pipelines", Details = ex.Message });
        }
    }

    #endregion

    #region Jenkins Build Management

    [HttpPost("jenkins/pipelines/{jobName}/builds")]
    public async Task<ActionResult<int>> TriggerJenkinsBuild(string jobName, [FromBody] TriggerBuildRequest? request = null)
    {
        try
        {
            var buildNumber = await _jenkinsService.TriggerBuildAsync(jobName, request?.Parameters);
            return Ok(new { BuildNumber = buildNumber, JobName = jobName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering Jenkins build for {JobName}", jobName);
            return StatusCode(500, new { Error = "Failed to trigger Jenkins build", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/pipelines/{jobName}/builds/{buildNumber}")]
    public async Task<ActionResult<JenkinsBuild>> GetJenkinsBuild(string jobName, int buildNumber)
    {
        try
        {
            var build = await _jenkinsService.GetBuildAsync(jobName, buildNumber);
            return Ok(build);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins build {JobName}#{BuildNumber}", jobName, buildNumber);
            return NotFound(new { Error = "Jenkins build not found", JobName = jobName, BuildNumber = buildNumber });
        }
    }

    [HttpGet("jenkins/pipelines/{jobName}/builds")]
    public async Task<ActionResult<List<JenkinsBuild>>> GetJenkinsBuilds(string jobName, [FromQuery] int? limit = null)
    {
        try
        {
            var builds = await _jenkinsService.GetBuildsAsync(jobName, limit);
            return Ok(builds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins builds for {JobName}", jobName);
            return StatusCode(500, new { Error = "Failed to get Jenkins builds", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/pipelines/{jobName}/builds/{buildNumber}/logs")]
    public async Task<ActionResult<string>> GetJenkinsBuildLogs(string jobName, int buildNumber)
    {
        try
        {
            var logs = await _jenkinsService.GetBuildLogsAsync(jobName, buildNumber);
            return Ok(new { Logs = logs });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins build logs {JobName}#{BuildNumber}", jobName, buildNumber);
            return StatusCode(500, new { Error = "Failed to get Jenkins build logs", Details = ex.Message });
        }
    }

    [HttpPost("jenkins/pipelines/{jobName}/builds/{buildNumber}/abort")]
    public async Task<ActionResult> AbortJenkinsBuild(string jobName, int buildNumber)
    {
        try
        {
            var success = await _jenkinsService.AbortBuildAsync(jobName, buildNumber);
            if (success)
                return Ok(new { Message = "Build aborted successfully" });
            else
                return BadRequest(new { Error = "Failed to abort build" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting Jenkins build {JobName}#{BuildNumber}", jobName, buildNumber);
            return StatusCode(500, new { Error = "Failed to abort Jenkins build", Details = ex.Message });
        }
    }

    #endregion

    #region Jenkins Deployment Management

    [HttpPost("jenkins/deployments")]
    public async Task<ActionResult<string>> CreateJenkinsDeploymentPipeline([FromBody] CreateDeploymentPipelineRequest request)
    {
        try
        {
            var deploymentJobName = await _jenkinsService.CreateDeploymentPipelineAsync(
                request.JobName, 
                request.Environment, 
                request.ArtifactPath);
            
            return Ok(new { DeploymentJobName = deploymentJobName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Jenkins deployment pipeline for {JobName}", request.JobName);
            return StatusCode(500, new { Error = "Failed to create Jenkins deployment pipeline", Details = ex.Message });
        }
    }

    [HttpPost("jenkins/deployments/{deploymentJobName}/trigger")]
    public async Task<ActionResult> TriggerJenkinsDeployment(string deploymentJobName, [FromBody] TriggerDeploymentRequest request)
    {
        try
        {
            var success = await _jenkinsService.TriggerDeploymentAsync(
                deploymentJobName, 
                request.Environment, 
                request.BuildArtifact);
            
            if (success)
                return Ok(new { Message = "Deployment triggered successfully" });
            else
                return BadRequest(new { Error = "Failed to trigger deployment" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering Jenkins deployment {DeploymentJobName}", deploymentJobName);
            return StatusCode(500, new { Error = "Failed to trigger Jenkins deployment", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/deployments")]
    public async Task<ActionResult<List<JenkinsDeployment>>> GetJenkinsDeployments([FromQuery] string? environment = null)
    {
        try
        {
            var deployments = await _jenkinsService.GetDeploymentsAsync(environment ?? "");
            return Ok(deployments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins deployments for environment {Environment}", environment);
            return StatusCode(500, new { Error = "Failed to get Jenkins deployments", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/deployments/{deploymentJobName}/builds/{buildNumber}")]
    public async Task<ActionResult<JenkinsDeployment>> GetJenkinsDeploymentStatus(string deploymentJobName, int buildNumber)
    {
        try
        {
            var deployment = await _jenkinsService.GetDeploymentStatusAsync(deploymentJobName, buildNumber);
            return Ok(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins deployment status {DeploymentJobName}#{BuildNumber}", deploymentJobName, buildNumber);
            return StatusCode(500, new { Error = "Failed to get Jenkins deployment status", Details = ex.Message });
        }
    }

    #endregion

    #region Jenkins Monitoring

    [HttpGet("jenkins/server-info")]
    public async Task<ActionResult<JenkinsServerInfo>> GetJenkinsServerInfo()
    {
        try
        {
            var serverInfo = await _jenkinsService.GetServerInfoAsync();
            return Ok(serverInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins server information");
            return StatusCode(500, new { Error = "Failed to get Jenkins server information", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/nodes")]
    public async Task<ActionResult<List<JenkinsNode>>> GetJenkinsNodes()
    {
        try
        {
            var nodes = await _jenkinsService.GetNodesAsync();
            return Ok(nodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins nodes");
            return StatusCode(500, new { Error = "Failed to get Jenkins nodes", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/system-health")]
    public async Task<ActionResult<JenkinsSystemHealth>> GetJenkinsSystemHealth()
    {
        try
        {
            var health = await _jenkinsService.GetSystemHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins system health");
            return StatusCode(500, new { Error = "Failed to get Jenkins system health", Details = ex.Message });
        }
    }

    [HttpGet("jenkins/build-queue")]
    public async Task<ActionResult<List<JenkinsQueue>>> GetJenkinsBuildQueue()
    {
        try
        {
            var queue = await _jenkinsService.GetBuildQueueAsync();
            return Ok(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Jenkins build queue");
            return StatusCode(500, new { Error = "Failed to get Jenkins build queue", Details = ex.Message });
        }
    }

    #endregion

    #region GitLab Project Management

    [HttpPost("gitlab/projects")]
    public async Task<ActionResult<GitLabProject>> CreateGitLabProject([FromBody] CreateGitLabProjectRequest request)
    {
        try
        {
            var project = await _gitLabService.CreateProjectAsync(
                request.Name, 
                request.Description, 
                request.Visibility);
            
            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitLab project {Name}", request.Name);
            return StatusCode(500, new { Error = "Failed to create GitLab project", Details = ex.Message });
        }
    }

    [HttpGet("gitlab/projects/{projectId}")]
    public async Task<ActionResult<GitLabProject>> GetGitLabProject(int projectId)
    {
        try
        {
            var project = await _gitLabService.GetProjectAsync(projectId);
            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab project {ProjectId}", projectId);
            return NotFound(new { Error = "GitLab project not found", ProjectId = projectId });
        }
    }

    [HttpPost("gitlab/projects/{projectId}/archive")]
    public async Task<ActionResult> ArchiveGitLabProject(int projectId)
    {
        try
        {
            var success = await _gitLabService.ArchiveProjectAsync(projectId);
            if (success)
                return Ok(new { Message = "Project archived successfully" });
            else
                return BadRequest(new { Error = "Failed to archive project" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving GitLab project {ProjectId}", projectId);
            return StatusCode(500, new { Error = "Failed to archive GitLab project", Details = ex.Message });
        }
    }

    [HttpGet("gitlab/work-requests/{workRequestId}/projects")]
    public async Task<ActionResult<List<GitLabProject>>> GetGitLabProjectsByWorkRequest(int workRequestId)
    {
        try
        {
            var projects = await _gitLabService.GetProjectsByWorkRequestAsync(workRequestId);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab projects for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Error = "Failed to get GitLab projects", Details = ex.Message });
        }
    }

    #endregion

    #region GitLab Pipeline Management

    [HttpPost("gitlab/projects/{projectId}/pipelines")]
    public async Task<ActionResult<GitLabPipeline>> TriggerGitLabPipeline(int projectId, [FromBody] TriggerGitLabPipelineRequest request)
    {
        try
        {
            var pipeline = await _gitLabService.TriggerPipelineAsync(
                projectId, 
                request.Branch, 
                request.Variables);
            
            return Ok(pipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering GitLab pipeline for project {ProjectId}", projectId);
            return StatusCode(500, new { Error = "Failed to trigger GitLab pipeline", Details = ex.Message });
        }
    }

    [HttpGet("gitlab/projects/{projectId}/pipelines/{pipelineId}")]
    public async Task<ActionResult<GitLabPipeline>> GetGitLabPipeline(int projectId, int pipelineId)
    {
        try
        {
            var pipeline = await _gitLabService.GetPipelineAsync(projectId, pipelineId);
            return Ok(pipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab pipeline {PipelineId} for project {ProjectId}", pipelineId, projectId);
            return NotFound(new { Error = "GitLab pipeline not found", ProjectId = projectId, PipelineId = pipelineId });
        }
    }

    [HttpGet("gitlab/projects/{projectId}/pipelines")]
    public async Task<ActionResult<List<GitLabPipeline>>> GetGitLabPipelines(int projectId, [FromQuery] string? status = null, [FromQuery] int? limit = null)
    {
        try
        {
            var pipelines = await _gitLabService.GetPipelinesAsync(projectId, status, limit);
            return Ok(pipelines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab pipelines for project {ProjectId}", projectId);
            return StatusCode(500, new { Error = "Failed to get GitLab pipelines", Details = ex.Message });
        }
    }

    [HttpPost("gitlab/projects/{projectId}/pipelines/{pipelineId}/cancel")]
    public async Task<ActionResult> CancelGitLabPipeline(int projectId, int pipelineId)
    {
        try
        {
            var success = await _gitLabService.CancelPipelineAsync(projectId, pipelineId);
            if (success)
                return Ok(new { Message = "Pipeline canceled successfully" });
            else
                return BadRequest(new { Error = "Failed to cancel pipeline" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling GitLab pipeline {PipelineId}", pipelineId);
            return StatusCode(500, new { Error = "Failed to cancel GitLab pipeline", Details = ex.Message });
        }
    }

    [HttpPost("gitlab/projects/{projectId}/pipelines/{pipelineId}/retry")]
    public async Task<ActionResult> RetryGitLabPipeline(int projectId, int pipelineId)
    {
        try
        {
            var success = await _gitLabService.RetryPipelineAsync(projectId, pipelineId);
            if (success)
                return Ok(new { Message = "Pipeline retried successfully" });
            else
                return BadRequest(new { Error = "Failed to retry pipeline" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying GitLab pipeline {PipelineId}", pipelineId);
            return StatusCode(500, new { Error = "Failed to retry GitLab pipeline", Details = ex.Message });
        }
    }

    #endregion

    #region GitLab Job Management

    [HttpGet("gitlab/projects/{projectId}/jobs/{jobId}")]
    public async Task<ActionResult<GitLabJob>> GetGitLabJob(int projectId, int jobId)
    {
        try
        {
            var job = await _gitLabService.GetJobAsync(projectId, jobId);
            return Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab job {JobId} for project {ProjectId}", jobId, projectId);
            return NotFound(new { Error = "GitLab job not found", ProjectId = projectId, JobId = jobId });
        }
    }

    [HttpGet("gitlab/projects/{projectId}/pipelines/{pipelineId}/jobs")]
    public async Task<ActionResult<List<GitLabJob>>> GetGitLabJobsForPipeline(int projectId, int pipelineId)
    {
        try
        {
            var jobs = await _gitLabService.GetJobsForPipelineAsync(projectId, pipelineId);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab jobs for pipeline {PipelineId}", pipelineId);
            return StatusCode(500, new { Error = "Failed to get GitLab jobs", Details = ex.Message });
        }
    }

    [HttpGet("gitlab/projects/{projectId}/jobs/{jobId}/logs")]
    public async Task<ActionResult<string>> GetGitLabJobLogs(int projectId, int jobId)
    {
        try
        {
            var logs = await _gitLabService.GetJobLogsAsync(projectId, jobId);
            return Ok(new { Logs = logs });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab job logs for job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to get GitLab job logs", Details = ex.Message });
        }
    }

    [HttpGet("gitlab/projects/{projectId}/jobs/{jobId}/artifacts")]
    public async Task<ActionResult> GetGitLabJobArtifacts(int projectId, int jobId)
    {
        try
        {
            var artifacts = await _gitLabService.GetJobArtifactsAsync(projectId, jobId);
            if (artifacts.Length > 0)
                return File(artifacts, "application/zip", $"job-{jobId}-artifacts.zip");
            else
                return NotFound(new { Error = "No artifacts found for this job" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitLab job artifacts for job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to get GitLab job artifacts", Details = ex.Message });
        }
    }

    [HttpPost("gitlab/projects/{projectId}/jobs/{jobId}/retry")]
    public async Task<ActionResult> RetryGitLabJob(int projectId, int jobId)
    {
        try
        {
            var success = await _gitLabService.RetryJobAsync(projectId, jobId);
            if (success)
                return Ok(new { Message = "Job retried successfully" });
            else
                return BadRequest(new { Error = "Failed to retry job" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying GitLab job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to retry GitLab job", Details = ex.Message });
        }
    }

    [HttpPost("gitlab/projects/{projectId}/jobs/{jobId}/cancel")]
    public async Task<ActionResult> CancelGitLabJob(int projectId, int jobId)
    {
        try
        {
            var success = await _gitLabService.CancelJobAsync(projectId, jobId);
            if (success)
                return Ok(new { Message = "Job canceled successfully" });
            else
                return BadRequest(new { Error = "Failed to cancel job" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling GitLab job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to cancel GitLab job", Details = ex.Message });
        }
    }

    #endregion

    #region Work Request Integration

    [HttpGet("work-requests/{workRequestId}/build-status")]
    public async Task<ActionResult<BuildStatusSummary>> GetWorkRequestBuildStatus(int workRequestId)
    {
        try
        {
            var buildStatus = await _jenkinsService.GetWorkRequestBuildStatusAsync(workRequestId);
            return Ok(buildStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting build status for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Error = "Failed to get build status", Details = ex.Message });
        }
    }

    [HttpGet("work-requests/{workRequestId}/pipeline-status")]
    public async Task<ActionResult<PipelineStatusSummary>> GetWorkRequestPipelineStatus(int workRequestId)
    {
        try
        {
            var pipelineStatus = await _gitLabService.GetWorkRequestPipelineStatusAsync(workRequestId);
            return Ok(pipelineStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pipeline status for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Error = "Failed to get pipeline status", Details = ex.Message });
        }
    }

    [HttpPost("work-requests/{workRequestId}/link-jenkins-pipeline")]
    public async Task<ActionResult> LinkWorkRequestToJenkinsPipeline(int workRequestId, [FromBody] LinkPipelineRequest request)
    {
        try
        {
            var success = await _jenkinsService.LinkWorkRequestToPipelineAsync(workRequestId, request.JobName);
            if (success)
                return Ok(new { Message = "Work request linked to Jenkins pipeline successfully" });
            else
                return BadRequest(new { Error = "Failed to link work request to Jenkins pipeline" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking work request {WorkRequestId} to Jenkins pipeline {JobName}", workRequestId, request.JobName);
            return StatusCode(500, new { Error = "Failed to link work request to Jenkins pipeline", Details = ex.Message });
        }
    }

    [HttpPost("work-requests/{workRequestId}/link-gitlab-project")]
    public async Task<ActionResult> LinkWorkRequestToGitLabProject(int workRequestId, [FromBody] LinkProjectRequest request)
    {
        try
        {
            var success = await _gitLabService.LinkWorkRequestToProjectAsync(workRequestId, request.ProjectId);
            if (success)
                return Ok(new { Message = "Work request linked to GitLab project successfully" });
            else
                return BadRequest(new { Error = "Failed to link work request to GitLab project" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking work request {WorkRequestId} to GitLab project {ProjectId}", workRequestId, request.ProjectId);
            return StatusCode(500, new { Error = "Failed to link work request to GitLab project", Details = ex.Message });
        }
    }

    [HttpPost("work-requests/{workRequestId}/create-branch")]
    public async Task<ActionResult> CreateWorkRequestBranch(int workRequestId, [FromBody] CreateBranchRequest request)
    {
        try
        {
            var success = await _gitLabService.CreateWorkRequestBranchAsync(workRequestId, request.ProjectId, request.BranchName);
            if (success)
                return Ok(new { Message = "Branch created successfully for work request" });
            else
                return BadRequest(new { Error = "Failed to create branch for work request" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch for work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new { Error = "Failed to create branch for work request", Details = ex.Message });
        }
    }

    #endregion
}

#region Request DTOs

public class CreateJenkinsPipelineRequest
{
    [Required]
    public string JobName { get; set; } = string.Empty;
    
    [Required]
    public string GitRepositoryUrl { get; set; } = string.Empty;
    
    public string JenkinsfilePath { get; set; } = "Jenkinsfile";
    
    [Required]
    public int WorkRequestId { get; set; }
}

public class TriggerBuildRequest
{
    public Dictionary<string, string>? Parameters { get; set; }
}

public class CreateDeploymentPipelineRequest
{
    [Required]
    public string JobName { get; set; } = string.Empty;
    
    [Required]
    public DeploymentEnvironment Environment { get; set; }
    
    [Required]
    public string ArtifactPath { get; set; } = string.Empty;
}

public class TriggerDeploymentRequest
{
    [Required]
    public string Environment { get; set; } = string.Empty;
    
    [Required]
    public string BuildArtifact { get; set; } = string.Empty;
}

public class CreateGitLabProjectRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Visibility { get; set; } = "private";
}

public class TriggerGitLabPipelineRequest
{
    [Required]
    public string Branch { get; set; } = string.Empty;
    
    public Dictionary<string, string>? Variables { get; set; }
}

public class LinkPipelineRequest
{
    [Required]
    public string JobName { get; set; } = string.Empty;
}

public class LinkProjectRequest
{
    [Required]
    public int ProjectId { get; set; }
}

public class CreateBranchRequest
{
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    public string BranchName { get; set; } = string.Empty;
}

#endregion 