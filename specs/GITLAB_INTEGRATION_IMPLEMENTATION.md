# GitLab Integration Implementation Guide

## Overview

The Work Intake System includes comprehensive GitLab integration capabilities that enable seamless management of repositories, CI/CD pipelines, merge requests, environments, and deployments. This integration follows Clean Architecture principles and provides a robust API for GitLab operations.

## Architecture

### Core Components

1. **Interface Layer** (`IGitLabIntegrationService`)
   - Defines the contract for all GitLab operations
   - Located in `WorkIntakeSystem.Core/Interfaces/`
   - Ensures loose coupling and testability

2. **Entity Layer** (`GitLabProject`, `GitLabPipeline`, etc.)
   - Domain entities representing GitLab concepts
   - Located in `WorkIntakeSystem.Core/Entities/CiCdEntities.cs`
   - Maps to GitLab API responses

3. **Service Layer** (`GitLabIntegrationService`)
   - Concrete implementation of GitLab operations
   - Located in `WorkIntakeSystem.Infrastructure/Services/`
   - Handles HTTP communication with GitLab API

4. **API Layer** (`CiCdController`)
   - RESTful endpoints for external access
   - Located in `WorkIntakeSystem.API/Controllers/`
   - Provides web interface for GitLab operations

## Configuration

### AppSettings Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "GitLab": {
    "ServerUrl": "https://your-gitlab-server",
    "PrivateToken": "your-gitlab-private-token",
    "DefaultTimeout": 30000,
    "EnableRetryOnFailure": true,
    "MaxRetryAttempts": 3,
    "DefaultVisibility": "private",
    "DefaultBranch": "main"
  }
}
```

### Production Configuration

For production environments, update `appsettings.Production.json`:

```json
{
  "GitLab": {
    "ServerUrl": "https://your-production-gitlab-server",
    "PrivateToken": "production-gitlab-private-token",
    "DefaultTimeout": 60000,
    "EnableRetryOnFailure": true,
    "MaxRetryAttempts": 3,
    "DefaultVisibility": "private",
    "DefaultBranch": "main"
  }
}
```

### Service Registration

Register the GitLab service in your `Program.cs`:

```csharp
// Add HTTP client for GitLab
builder.Services.AddHttpClient<IGitLabIntegrationService, GitLabIntegrationService>();

// Register GitLab integration service
builder.Services.AddScoped<IGitLabIntegrationService, GitLabIntegrationService>();
```

## API Endpoints

### Repository Management

#### Create Project
```http
POST /api/cicd/gitlab/projects
Content-Type: application/json

{
  "name": "work-request-123",
  "description": "Repository for work request #123",
  "visibility": "private"
}
```

#### Get Project
```http
GET /api/cicd/gitlab/projects/{projectId}
```

#### Archive Project
```http
POST /api/cicd/gitlab/projects/{projectId}/archive
```

### Pipeline Management

#### Trigger Pipeline
```http
POST /api/cicd/gitlab/projects/{projectId}/pipelines
Content-Type: application/json

{
  "branch": "feature/new-feature",
  "variables": {
    "ENVIRONMENT": "testing",
    "WORK_REQUEST_ID": "123"
  }
}
```

#### Get Pipeline
```http
GET /api/cicd/gitlab/projects/{projectId}/pipelines/{pipelineId}
```

#### Get Pipelines
```http
GET /api/cicd/gitlab/projects/{projectId}/pipelines?status=running&limit=10
```

#### Cancel Pipeline
```http
POST /api/cicd/gitlab/projects/{projectId}/pipelines/{pipelineId}/cancel
```

#### Retry Pipeline
```http
POST /api/cicd/gitlab/projects/{projectId}/pipelines/{pipelineId}/retry
```

### Job Management

#### Get Job
```http
GET /api/cicd/gitlab/projects/{projectId}/jobs/{jobId}
```

#### Get Job Logs
```http
GET /api/cicd/gitlab/projects/{projectId}/jobs/{jobId}/logs
```

#### Get Job Artifacts
```http
GET /api/cicd/gitlab/projects/{projectId}/jobs/{jobId}/artifacts
```

#### Retry Job
```http
POST /api/cicd/gitlab/projects/{projectId}/jobs/{jobId}/retry
```

### Merge Request Management

#### Create Merge Request
```http
POST /api/cicd/gitlab/projects/{projectId}/merge-requests
Content-Type: application/json

{
  "sourceBranch": "feature/new-feature",
  "targetBranch": "main",
  "title": "Implement new feature for work request #123",
  "description": "This MR implements the requirements from work request #123"
}
```

#### Get Merge Request
```http
GET /api/cicd/gitlab/projects/{projectId}/merge-requests/{mergeRequestId}
```

#### Approve Merge Request
```http
POST /api/cicd/gitlab/projects/{projectId}/merge-requests/{mergeRequestId}/approve
```

#### Merge Merge Request
```http
POST /api/cicd/gitlab/projects/{projectId}/merge-requests/{mergeRequestId}/merge
```

### Environment & Deployment Management

#### Get Environments
```http
GET /api/cicd/gitlab/projects/{projectId}/environments
```

#### Get Deployments
```http
GET /api/cicd/gitlab/projects/{projectId}/deployments?environment=production
```

#### Create Deployment
```http
POST /api/cicd/gitlab/projects/{projectId}/deployments
Content-Type: application/json

{
  "environment": "production",
  "sha": "abc123def456",
  "variables": {
    "DEPLOYMENT_TYPE": "manual"
  }
}
```

### CI/CD Configuration

#### Get CI Configuration
```http
GET /api/cicd/gitlab/projects/{projectId}/ci-config?ref=main
```

#### Update CI Configuration
```http
PUT /api/cicd/gitlab/projects/{projectId}/ci-config
Content-Type: application/json

{
  "ciConfigContent": "stages:\n  - build\n  - test\n  - deploy",
  "commitMessage": "Update CI/CD configuration"
}
```

#### Lint CI Configuration
```http
POST /api/cicd/gitlab/ci-lint
Content-Type: application/json

{
  "ciConfigContent": "stages:\n  - build\n  - test\n  - deploy"
}
```

### Variables & Secrets Management

#### Create Variable
```http
POST /api/cicd/gitlab/projects/{projectId}/variables
Content-Type: application/json

{
  "key": "DATABASE_URL",
  "value": "postgresql://user:pass@host:5432/db",
  "masked": true,
  "protectedVar": true
}
```

#### Get Variables
```http
GET /api/cicd/gitlab/projects/{projectId}/variables
```

#### Update Variable
```http
PUT /api/cicd/gitlab/projects/{projectId}/variables/{key}
Content-Type: application/json

{
  "value": "new-database-url",
  "masked": true
}
```

#### Delete Variable
```http
DELETE /api/cicd/gitlab/projects/{projectId}/variables/{key}
```

### Webhooks & Integration

#### Create Webhook
```http
POST /api/cicd/gitlab/projects/{projectId}/webhooks
Content-Type: application/json

{
  "url": "https://your-app.com/webhooks/gitlab",
  "events": ["push", "merge_request", "pipeline"]
}
```

#### Get Webhooks
```http
GET /api/cicd/gitlab/projects/{projectId}/webhooks
```

#### Delete Webhook
```http
DELETE /api/cicd/gitlab/projects/{projectId}/webhooks/{hookId}
```

## Usage Examples

### Creating a Project for a Work Request

```csharp
// Inject the service
private readonly IGitLabIntegrationService _gitLabService;

// Create project
var project = await _gitLabService.CreateProjectAsync(
    name: $"work-request-{workRequest.Id}",
    description: $"Repository for work request #{workRequest.Id}",
    visibility: "private"
);

// Link to work request
await _gitLabService.LinkWorkRequestToProjectAsync(workRequest.Id, project.GitLabProjectId);
```

### Triggering a Pipeline with Variables

```csharp
var pipelineVariables = new Dictionary<string, string>
{
    ["ENVIRONMENT"] = "testing",
    ["WORK_REQUEST_ID"] = workRequest.Id.ToString(),
    ["FEATURE_FLAG"] = "enabled"
};

var pipeline = await _gitLabService.TriggerPipelineAsync(
    projectId: project.GitLabProjectId,
    branch: "feature/new-feature",
    variables: pipelineVariables
);
```

### Creating a Merge Request

```csharp
var mergeRequest = await _gitLabService.CreateMergeRequestAsync(
    projectId: project.GitLabProjectId,
    sourceBranch: "feature/new-feature",
    targetBranch: "main",
    title: $"Implement feature for work request #{workRequest.Id}",
    description: $"This merge request implements the requirements from work request #{workRequest.Id}.\n\n**Work Request Details:**\n- Title: {workRequest.Title}\n- Priority: {workRequest.Priority}\n- Assigned To: {workRequest.AssignedTo}"
);
```

### Managing CI/CD Variables

```csharp
// Create masked variable for sensitive data
await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "DATABASE_PASSWORD",
    value: "secure-password-123",
    masked: true,
    protectedVar: true
);

// Create regular variable
await _gitLabService.CreateVariableAsync(
    projectId: project.GitLabProjectId,
    key: "DEPLOYMENT_ENVIRONMENT",
    value: "staging",
    masked: false,
    protectedVar: false
);
```

### Monitoring Pipeline Status

```csharp
var pipelines = await _gitLabService.GetPipelinesAsync(
    projectId: project.GitLabProjectId,
    status: "running",
    limit: 5
);

foreach (var pipeline in pipelines)
{
    _logger.LogInformation("Pipeline {PipelineId} status: {Status}", 
        pipeline.GitLabPipelineId, pipeline.Status);
    
    if (pipeline.Status == GitLabPipelineStatus.Success)
    {
        // Trigger deployment
        await _gitLabService.CreateDeploymentAsync(
            projectId: project.GitLabProjectId,
            environment: "staging",
            sha: pipeline.Sha,
            variables: new Dictionary<string, string> { ["AUTO_DEPLOY"] = "true" }
        );
    }
}
```

### Setting up Webhooks

```csharp
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

## Error Handling

The GitLab integration includes comprehensive error handling:

```csharp
try
{
    var project = await _gitLabService.CreateProjectAsync(name, description, visibility);
    return Ok(new { ProjectId = project.GitLabProjectId, Name = project.Name });
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    _logger.LogError(ex, "GitLab authentication failed");
    return StatusCode(401, new { Error = "GitLab authentication failed" });
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
{
    _logger.LogError(ex, "Project {ProjectName} already exists", name);
    return StatusCode(409, new { Error = "Project already exists" });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating GitLab project {ProjectName}", name);
    return StatusCode(500, new { Error = "Failed to create GitLab project", Details = ex.Message });
}
```

## Security Considerations

1. **Private Token Management**
   - Store GitLab private tokens securely in configuration
   - Use environment variables for production tokens
   - Rotate tokens regularly
   - Use project-specific tokens when possible

2. **Authentication**
   - The service uses Private Token authentication
   - Ensure HTTPS is used in production environments
   - Validate token permissions before operations

3. **Access Control**
   - Implement proper authorization in the API controller
   - Validate user permissions before allowing GitLab operations
   - Use project-specific tokens for enhanced security

4. **Variable Security**
   - Always mask sensitive variables (passwords, API keys)
   - Use protected variables for production environments
   - Regularly audit variable access

## Testing

### Unit Testing

```csharp
[Test]
public async Task CreateProjectAsync_ValidParameters_ReturnsProject()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    var mockLogger = new Mock<ILogger<GitLabIntegrationService>>();
    var mockConfiguration = new Mock<IConfiguration>();
    
    // Setup configuration
    mockConfiguration.Setup(x => x["GitLab:ServerUrl"]).Returns("https://gitlab.com");
    mockConfiguration.Setup(x => x["GitLab:PrivateToken"]).Returns("token");
    
    var service = new GitLabIntegrationService(mockLogger.Object, mockConfiguration.Object, mockHttpClient.Object);
    
    // Act
    var result = await service.CreateProjectAsync("test-project", "Test description", "private");
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual("test-project", result.Name);
}
```

### Integration Testing

```csharp
[Test]
public async Task GitLabIntegration_EndToEnd_Workflow()
{
    // Create project
    var project = await _gitLabService.CreateProjectAsync("test-project", "Test project", "private");
    Assert.IsNotNull(project);
    
    // Trigger pipeline
    var pipeline = await _gitLabService.TriggerPipelineAsync(project.GitLabProjectId, "main");
    Assert.IsNotNull(pipeline);
    
    // Create merge request
    var mergeRequest = await _gitLabService.CreateMergeRequestAsync(
        project.GitLabProjectId, "feature/test", "main", "Test MR", "Test description");
    Assert.IsNotNull(mergeRequest);
    
    // Cleanup
    await _gitLabService.ArchiveProjectAsync(project.GitLabProjectId);
}
```

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   - Verify private token is correct and has sufficient permissions
   - Check if token has expired
   - Ensure token has the required scopes (api, read_user, etc.)

2. **Network Connectivity**
   - Verify GitLab server URL is accessible
   - Check firewall settings
   - Ensure proper DNS resolution

3. **Project Creation Errors**
   - Check if project name already exists
   - Verify user has permission to create projects
   - Ensure project name follows GitLab naming conventions

4. **Pipeline Errors**
   - Validate `.gitlab-ci.yml` syntax
   - Check if required variables are set
   - Verify runner availability

### Debugging

Enable detailed logging by setting the log level to Debug:

```json
{
  "Logging": {
    "LogLevel": {
      "WorkIntakeSystem.Infrastructure.Services.GitLabIntegrationService": "Debug"
    }
  }
}
```

## Best Practices

1. **Project Naming**
   - Use consistent naming conventions
   - Include work request IDs in project names
   - Avoid special characters in project names

2. **Variable Management**
   - Always mask sensitive variables
   - Use descriptive variable names
   - Group related variables with prefixes

3. **Pipeline Design**
   - Keep pipelines simple and focused
   - Use stages for logical grouping
   - Implement proper error handling in CI/CD scripts

4. **Security**
   - Regularly rotate private tokens
   - Use project-specific tokens when possible
   - Implement proper access controls

5. **Monitoring**
   - Monitor pipeline success/failure rates
   - Track deployment status
   - Set up alerts for critical failures

## Future Enhancements

1. **Advanced CI/CD Features**
   - Multi-project pipelines
   - Cross-project dependencies
   - Advanced deployment strategies

2. **Integration Enhancements**
   - Real-time pipeline status updates
   - Webhook-based notifications
   - Advanced merge request workflows

3. **Security Improvements**
   - OAuth2 integration
   - Fine-grained permission management
   - Audit logging for all operations

4. **Workflow Automation**
   - Automatic project creation for work requests
   - Automated merge request creation
   - Integration with approval workflows

5. **Monitoring & Analytics**
   - Pipeline performance metrics
   - Deployment success rates
   - Resource utilization tracking 