# CI/CD Integration Usage Guide

## Overview

This guide provides comprehensive instructions for using the Jenkins and GitLab integration features in the Work Intake System. The integration enables seamless CI/CD workflows, automated deployments, and comprehensive monitoring of your development processes.

## Quick Start

### Prerequisites

1. **Jenkins Server**
   - Jenkins instance running and accessible
   - Admin user with API token
   - Required plugins installed (Git, Pipeline, etc.)

2. **GitLab Server**
   - GitLab instance running and accessible
   - User account with API access
   - Private token with appropriate permissions

3. **Work Intake System**
   - Application configured with Jenkins and GitLab settings
   - Services registered in dependency injection
   - API endpoints accessible

### Initial Setup

1. **Configure Jenkins Settings**
   ```json
   {
     "Jenkins": {
       "ServerUrl": "http://your-jenkins-server:8080",
       "Username": "your-jenkins-username",
       "ApiToken": "your-jenkins-api-token"
     }
   }
   ```

2. **Configure GitLab Settings**
   ```json
   {
     "GitLab": {
       "ServerUrl": "https://your-gitlab-server",
       "PrivateToken": "your-gitlab-private-token"
     }
   }
   ```

3. **Register Services**
   ```csharp
   // In Program.cs
   builder.Services.AddHttpClient<IJenkinsIntegrationService, JenkinsIntegrationService>();
   builder.Services.AddHttpClient<IGitLabIntegrationService, GitLabIntegrationService>();
   builder.Services.AddScoped<IJenkinsIntegrationService, JenkinsIntegrationService>();
   builder.Services.AddScoped<IGitLabIntegrationService, GitLabIntegrationService>();
   ```

## Workflow Examples

### Complete Development Workflow

#### 1. Create Work Request
```csharp
// Create a new work request
var workRequest = new WorkRequest
{
    Title = "Implement User Authentication",
    Description = "Add OAuth2 authentication to the application",
    Priority = Priority.High,
    AssignedTo = "developer@company.com"
};
```

#### 2. Create GitLab Project
```csharp
// Create GitLab repository for the work request
var project = await _gitLabService.CreateProjectAsync(
    name: $"auth-feature-{workRequest.Id}",
    description: $"Repository for work request #{workRequest.Id}: {workRequest.Title}",
    visibility: "private"
);

// Link project to work request
await _gitLabService.LinkWorkRequestToProjectAsync(workRequest.Id, project.GitLabProjectId);
```

#### 3. Create Jenkins Pipeline
```csharp
// Create Jenkins pipeline for CI/CD
var pipelineId = await _jenkinsService.CreatePipelineAsync(
    jobName: $"work-request-{workRequest.Id}",
    gitRepositoryUrl: project.HttpUrl,
    jenkinsfilePath: "Jenkinsfile",
    workRequestId: workRequest.Id
);
```

#### 4. Set Up CI/CD Variables
```csharp
// Configure GitLab variables for the project
await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "JENKINS_URL",
    value: "http://jenkins:8080",
    masked: false,
    protectedVar: false
);

await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "DEPLOYMENT_ENVIRONMENT",
    value: "staging",
    masked: false,
    protectedVar: false
);

// Add sensitive variables (masked)
await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "DATABASE_PASSWORD",
    value: "secure-password",
    masked: true,
    protectedVar: true
);
```

#### 5. Create Feature Branch
```csharp
// Create feature branch for development
await _gitLabService.CreateWorkRequestBranchAsync(
    workRequestId: workRequest.Id,
    projectId: project.GitLabProjectId,
    branchName: "feature/oauth2-authentication"
);
```

#### 6. Trigger Initial Pipeline
```csharp
// Trigger pipeline for the feature branch
var pipeline = await _gitLabService.TriggerPipelineAsync(
    projectId: project.GitLabProjectId,
    branch: "feature/oauth2-authentication",
    variables: new Dictionary<string, string>
    {
        ["WORK_REQUEST_ID"] = workRequest.Id.ToString(),
        ["FEATURE_BRANCH"] = "feature/oauth2-authentication"
    }
);
```

#### 7. Monitor Pipeline Status
```csharp
// Monitor pipeline execution
var pipelineStatus = await _gitLabService.GetPipelineAsync(
    projectId: project.GitLabProjectId,
    pipelineId: pipeline.GitLabPipelineId
);

if (pipelineStatus.Status == GitLabPipelineStatus.Success)
{
    // Pipeline succeeded - proceed with testing
    _logger.LogInformation("Pipeline {PipelineId} completed successfully", pipeline.GitLabPipelineId);
}
else if (pipelineStatus.Status == GitLabPipelineStatus.Failed)
{
    // Pipeline failed - get logs for debugging
    var jobs = await _gitLabService.GetJobsForPipelineAsync(
        projectId: project.GitLabProjectId,
        pipelineId: pipeline.GitLabPipelineId
    );
    
    foreach (var job in jobs.Where(j => j.Status == GitLabJobStatus.Failed))
    {
        var logs = await _gitLabService.GetJobLogsAsync(
            projectId: project.GitLabProjectId,
            jobId: job.GitLabJobId
        );
        _logger.LogError("Job {JobId} failed: {Logs}", job.GitLabJobId, logs);
    }
}
```

#### 8. Create Merge Request
```csharp
// Create merge request when feature is complete
var mergeRequest = await _gitLabService.CreateMergeRequestAsync(
    projectId: project.GitLabProjectId,
    sourceBranch: "feature/oauth2-authentication",
    targetBranch: "main",
    title: $"Implement OAuth2 Authentication - Work Request #{workRequest.Id}",
    description: $@"
This merge request implements OAuth2 authentication as requested in work request #{workRequest.Id}.

**Changes:**
- Added OAuth2 provider integration
- Implemented user authentication flow
- Added JWT token handling
- Updated user management system

**Testing:**
- Unit tests: ✅ Passed
- Integration tests: ✅ Passed
- Security tests: ✅ Passed

**Deployment:**
- Staging environment: Ready for deployment
- Production environment: Requires approval

**Work Request Details:**
- ID: {workRequest.Id}
- Title: {workRequest.Title}
- Priority: {workRequest.Priority}
- Assigned To: {workRequest.AssignedTo}
    "
);
```

#### 9. Approve and Merge
```csharp
// Approve the merge request
await _gitLabService.ApproveMergeRequestAsync(
    projectId: project.GitLabProjectId,
    mergeRequestId: mergeRequest.GitLabMergeRequestId
);

// Merge the request
var mergedRequest = await _gitLabService.MergeMergeRequestAsync(
    projectId: project.GitLabProjectId,
    mergeRequestId: mergeRequest.GitLabMergeRequestId
);
```

#### 10. Deploy to Staging
```csharp
// Trigger deployment to staging environment
await _jenkinsService.TriggerDeploymentAsync(
    deploymentJobName: "deploy-to-staging",
    environment: "staging",
    buildArtifact: $"build-{pipeline.GitLabPipelineId}"
);
```

#### 11. Monitor Deployment
```csharp
// Monitor deployment status
var deployment = await _jenkinsService.GetDeploymentStatusAsync(
    deploymentJobName: "deploy-to-staging",
    buildNumber: pipeline.GitLabPipelineId
);

if (deployment.Status == DeploymentStatus.Deployed)
{
    _logger.LogInformation("Deployment to staging completed successfully");
    
    // Update work request status
    workRequest.Status = WorkRequestStatus.InTesting;
    // Save work request changes...
}
```

### Automated Testing Workflow

#### 1. Set Up Test Pipeline
```csharp
// Create dedicated test pipeline
var testPipelineId = await _jenkinsService.CreatePipelineAsync(
    jobName: $"test-work-request-{workRequest.Id}",
    gitRepositoryUrl: project.HttpUrl,
    jenkinsfilePath: "Jenkinsfile.test",
    workRequestId: workRequest.Id
);
```

#### 2. Configure Test Environment
```csharp
// Set up test-specific variables
await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "TEST_DATABASE_URL",
    value: "postgresql://test:test@test-db:5432/test_db",
    masked: true,
    protectedVar: false
);

await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "TEST_ENVIRONMENT",
    value: "testing",
    masked: false,
    protectedVar: false
);
```

#### 3. Run Automated Tests
```csharp
// Trigger test pipeline
var testPipeline = await _gitLabService.TriggerPipelineAsync(
    projectId: project.GitLabProjectId,
    branch: "main",
    variables: new Dictionary<string, string>
    {
        ["TEST_TYPE"] = "automated",
        ["WORK_REQUEST_ID"] = workRequest.Id.ToString()
    }
);
```

#### 4. Process Test Results
```csharp
// Monitor test execution
var testJobs = await _gitLabService.GetJobsForPipelineAsync(
    projectId: project.GitLabProjectId,
    pipelineId: testPipeline.GitLabPipelineId
);

var testResults = new List<TestResult>();

foreach (var job in testJobs)
{
    if (job.Status == GitLabJobStatus.Success)
    {
        // Get test artifacts
        var artifacts = await _gitLabService.GetJobArtifactsAsync(
            projectId: project.GitLabProjectId,
            jobId: job.GitLabJobId
        );
        
        // Process test results from artifacts
        // Parse JUnit XML, coverage reports, etc.
    }
}
```

### Production Deployment Workflow

#### 1. Create Production Deployment Pipeline
```csharp
// Create production deployment pipeline
var productionPipelineId = await _jenkinsService.CreateDeploymentPipelineAsync(
    jobName: "deploy-to-production",
    environment: DeploymentEnvironment.Production,
    artifactPath: "builds/artifacts"
);
```

#### 2. Set Up Production Variables
```csharp
// Configure production-specific variables
await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "PRODUCTION_DATABASE_URL",
    value: "postgresql://prod:prod@prod-db:5432/prod_db",
    masked: true,
    protectedVar: true
);

await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "PRODUCTION_API_KEY",
    value: "prod-api-key-123",
    masked: true,
    protectedVar: true
);
```

#### 3. Trigger Production Deployment
```csharp
// Trigger production deployment (requires approval)
await _jenkinsService.TriggerDeploymentAsync(
    deploymentJobName: "deploy-to-production",
    environment: "production",
    buildArtifact: $"build-{pipeline.GitLabPipelineId}"
);
```

#### 4. Monitor Production Deployment
```csharp
// Monitor deployment progress
var productionDeployment = await _jenkinsService.GetDeploymentStatusAsync(
    deploymentJobName: "deploy-to-production",
    buildNumber: pipeline.GitLabPipelineId
);

switch (productionDeployment.Status)
{
    case DeploymentStatus.Deploying:
        _logger.LogInformation("Production deployment in progress...");
        break;
    case DeploymentStatus.Deployed:
        _logger.LogInformation("Production deployment completed successfully");
        workRequest.Status = WorkRequestStatus.Completed;
        break;
    case DeploymentStatus.Failed:
        _logger.LogError("Production deployment failed");
        // Trigger rollback or alert team
        break;
}
```

## Integration Patterns

### Webhook Integration

#### 1. Set Up Webhooks
```csharp
// Create webhook for real-time updates
var webhook = await _gitLabService.CreateWebhookAsync(
    projectId: project.GitLabProjectId,
    url: "https://your-app.com/api/webhooks/gitlab",
    events: new List<string>
    {
        "push",
        "merge_request",
        "pipeline",
        "deployment"
    }
);
```

#### 2. Handle Webhook Events
```csharp
[HttpPost("webhooks/gitlab")]
public async Task<IActionResult> HandleGitLabWebhook([FromBody] GitLabWebhookPayload payload)
{
    switch (payload.ObjectKind)
    {
        case "push":
            await HandlePushEvent(payload);
            break;
        case "merge_request":
            await HandleMergeRequestEvent(payload);
            break;
        case "pipeline":
            await HandlePipelineEvent(payload);
            break;
        case "deployment":
            await HandleDeploymentEvent(payload);
            break;
    }
    
    return Ok();
}
```

### Cross-Service Integration

#### 1. Jenkins-GitLab Coordination
```csharp
// When GitLab pipeline succeeds, trigger Jenkins deployment
public async Task HandleSuccessfulPipeline(int projectId, int pipelineId)
{
    var pipeline = await _gitLabService.GetPipelineAsync(projectId, pipelineId);
    
    if (pipeline.Status == GitLabPipelineStatus.Success)
    {
        // Trigger Jenkins deployment
        await _jenkinsService.TriggerDeploymentAsync(
            deploymentJobName: "deploy-to-staging",
            environment: "staging",
            buildArtifact: $"gitlab-pipeline-{pipelineId}"
        );
    }
}
```

#### 2. Status Synchronization
```csharp
// Sync status between services
public async Task SyncWorkRequestStatus(int workRequestId)
{
    var gitLabPipelines = await _gitLabService.GetPipelinesForWorkRequestAsync(workRequestId);
    var jenkinsBuilds = await _jenkinsService.GetBuildsForWorkRequestAsync(workRequestId);
    
    var overallStatus = DetermineOverallStatus(gitLabPipelines, jenkinsBuilds);
    
    // Update work request status
    var workRequest = await GetWorkRequestAsync(workRequestId);
    workRequest.Status = overallStatus;
    await UpdateWorkRequestAsync(workRequest);
}
```

## Monitoring and Alerting

### Health Checks

#### 1. Service Health Monitoring
```csharp
[HttpGet("health/cicd")]
public async Task<IActionResult> GetCiCdHealth()
{
    var healthStatus = new
    {
        Jenkins = await CheckJenkinsHealth(),
        GitLab = await CheckGitLabHealth(),
        Timestamp = DateTime.UtcNow
    };
    
    return Ok(healthStatus);
}

private async Task<object> CheckJenkinsHealth()
{
    try
    {
        var serverInfo = await _jenkinsService.GetServerInfoAsync();
        return new { Status = "Healthy", Version = serverInfo.Version };
    }
    catch (Exception ex)
    {
        return new { Status = "Unhealthy", Error = ex.Message };
    }
}
```

#### 2. Pipeline Monitoring
```csharp
public async Task MonitorPipelineHealth()
{
    var projects = await _gitLabService.GetProjectsByWorkRequestAsync(workRequestId);
    
    foreach (var project in projects)
    {
        var pipelines = await _gitLabService.GetPipelinesAsync(project.GitLabProjectId, limit: 10);
        
        var failedPipelines = pipelines.Where(p => p.Status == GitLabPipelineStatus.Failed);
        
        if (failedPipelines.Any())
        {
            // Send alert
            await SendPipelineFailureAlert(project, failedPipelines);
        }
    }
}
```

### Metrics Collection

#### 1. Build Metrics
```csharp
public async Task CollectBuildMetrics()
{
    var metrics = new BuildMetrics
    {
        TotalBuilds = 0,
        SuccessfulBuilds = 0,
        FailedBuilds = 0,
        AverageBuildTime = TimeSpan.Zero
    };
    
    var builds = await _jenkinsService.GetBuildsAsync("work-request-123");
    
    foreach (var build in builds)
    {
        metrics.TotalBuilds++;
        
        if (build.Status == BuildStatus.Success)
            metrics.SuccessfulBuilds++;
        else if (build.Status == BuildStatus.Failed)
            metrics.FailedBuilds++;
        
        if (build.Duration.HasValue)
            metrics.AverageBuildTime += build.Duration.Value;
    }
    
    if (metrics.TotalBuilds > 0)
        metrics.AverageBuildTime = TimeSpan.FromTicks(metrics.AverageBuildTime.Ticks / metrics.TotalBuilds);
    
    // Store metrics for reporting
    await StoreBuildMetrics(metrics);
}
```

## Best Practices

### 1. Security

- **Token Management**: Rotate API tokens regularly
- **Access Control**: Use least-privilege access for tokens
- **Variable Security**: Always mask sensitive variables
- **HTTPS**: Use HTTPS for all production communications

### 2. Performance

- **Caching**: Cache frequently accessed data
- **Timeouts**: Set appropriate timeouts for long-running operations
- **Retry Logic**: Implement retry logic for transient failures
- **Async Operations**: Use async/await for all I/O operations

### 3. Reliability

- **Error Handling**: Implement comprehensive error handling
- **Logging**: Log all operations for debugging
- **Monitoring**: Set up health checks and alerts
- **Backup**: Regularly backup configuration and data

### 4. Maintainability

- **Naming Conventions**: Use consistent naming for projects and pipelines
- **Documentation**: Document all custom configurations
- **Version Control**: Version control all CI/CD configurations
- **Testing**: Test all automation scripts thoroughly

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   - Verify API tokens are valid and not expired
   - Check token permissions
   - Ensure correct server URLs

2. **Pipeline Failures**
   - Check CI/CD configuration syntax
   - Verify required variables are set
   - Review build logs for specific errors

3. **Network Issues**
   - Verify network connectivity between services
   - Check firewall settings
   - Ensure DNS resolution works correctly

4. **Resource Issues**
   - Monitor Jenkins executor availability
   - Check GitLab runner status
   - Verify sufficient disk space and memory

### Debugging Steps

1. **Enable Debug Logging**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "WorkIntakeSystem.Infrastructure.Services": "Debug"
       }
     }
   }
   ```

2. **Check Service Health**
   ```bash
   curl -X GET "https://your-app.com/api/cicd/health"
   ```

3. **Verify API Connectivity**
   ```bash
   # Test Jenkins API
   curl -u username:token "http://jenkins:8080/api/json"
   
   # Test GitLab API
   curl -H "Private-Token: your-token" "https://gitlab.com/api/v4/projects"
   ```

4. **Review Logs**
   - Check application logs for errors
   - Review Jenkins build logs
   - Examine GitLab pipeline logs

## Support and Resources

### Documentation
- [Jenkins API Documentation](https://www.jenkins.io/doc/book/using/remote-access-api/)
- [GitLab API Documentation](https://docs.gitlab.com/ee/api/)
- [Work Intake System Documentation](./README.md)

### Community
- Jenkins Community: [jenkins.io/community](https://jenkins.io/community/)
- GitLab Community: [gitlab.com/community](https://gitlab.com/community/)

### Support Channels
- Internal Support: support@company.com
- Emergency Contact: oncall@company.com
- Documentation Issues: docs@company.com 