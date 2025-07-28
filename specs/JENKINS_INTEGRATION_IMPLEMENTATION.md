# Jenkins Integration Implementation Guide

## Overview

The Work Intake System includes comprehensive Jenkins integration capabilities that allow seamless management of CI/CD pipelines, builds, deployments, and monitoring directly from the application. This integration follows Clean Architecture principles and provides a robust API for Jenkins operations.

## Architecture

### Core Components

1. **Interface Layer** (`IJenkinsIntegrationService`)
   - Defines the contract for all Jenkins operations
   - Located in `WorkIntakeSystem.Core/Interfaces/`
   - Ensures loose coupling and testability

2. **Entity Layer** (`JenkinsPipeline`, `JenkinsBuild`, etc.)
   - Domain entities representing Jenkins concepts
   - Located in `WorkIntakeSystem.Core/Entities/CiCdEntities.cs`
   - Maps to Jenkins API responses

3. **Service Layer** (`JenkinsIntegrationService`)
   - Concrete implementation of Jenkins operations
   - Located in `WorkIntakeSystem.Infrastructure/Services/`
   - Handles HTTP communication with Jenkins API

4. **API Layer** (`CiCdController`)
   - RESTful endpoints for external access
   - Located in `WorkIntakeSystem.API/Controllers/`
   - Provides web interface for Jenkins operations

## Configuration

### AppSettings Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "Jenkins": {
    "ServerUrl": "http://your-jenkins-server:8080",
    "Username": "your-jenkins-username",
    "ApiToken": "your-jenkins-api-token",
    "DefaultTimeout": 30000,
    "EnableRetryOnFailure": true,
    "MaxRetryAttempts": 3
  }
}
```

### Production Configuration

For production environments, update `appsettings.Production.json`:

```json
{
  "Jenkins": {
    "ServerUrl": "https://your-production-jenkins-server",
    "Username": "production-jenkins-user",
    "ApiToken": "production-jenkins-api-token",
    "DefaultTimeout": 60000,
    "EnableRetryOnFailure": true,
    "MaxRetryAttempts": 3
  }
}
```

### Service Registration

Register the Jenkins service in your `Program.cs`:

```csharp
// Add HTTP client for Jenkins
builder.Services.AddHttpClient<IJenkinsIntegrationService, JenkinsIntegrationService>();

// Register Jenkins integration service
builder.Services.AddScoped<IJenkinsIntegrationService, JenkinsIntegrationService>();
```

## API Endpoints

### Pipeline Management

#### Create Pipeline
```http
POST /api/cicd/jenkins/pipelines
Content-Type: application/json

{
  "jobName": "my-pipeline",
  "gitRepositoryUrl": "https://github.com/company/repo.git",
  "jenkinsfilePath": "Jenkinsfile",
  "workRequestId": 123
}
```

#### Get Pipeline
```http
GET /api/cicd/jenkins/pipelines/{jobName}
```

#### Update Pipeline Configuration
```http
PUT /api/cicd/jenkins/pipelines/{jobName}
Content-Type: application/json

{
  "description": "Updated pipeline description",
  "gitRepositoryUrl": "https://github.com/company/new-repo.git",
  "branch": "develop",
  "jenkinsfilePath": "Jenkinsfile",
  "parameters": {
    "ENVIRONMENT": "staging"
  },
  "triggers": ["pollscm", "github"],
  "enabled": true
}
```

#### Delete Pipeline
```http
DELETE /api/cicd/jenkins/pipelines/{jobName}
```

### Build Management

#### Trigger Build
```http
POST /api/cicd/jenkins/builds/{jobName}
Content-Type: application/json

{
  "parameters": {
    "BRANCH": "feature/new-feature",
    "ENVIRONMENT": "testing"
  }
}
```

#### Get Build Information
```http
GET /api/cicd/jenkins/builds/{jobName}/{buildNumber}
```

#### Get Build Logs
```http
GET /api/cicd/jenkins/builds/{jobName}/{buildNumber}/logs
```

#### Abort Build
```http
POST /api/cicd/jenkins/builds/{jobName}/{buildNumber}/abort
```

### Deployment Management

#### Create Deployment Pipeline
```http
POST /api/cicd/jenkins/deployments
Content-Type: application/json

{
  "jobName": "deploy-to-production",
  "environment": "Production",
  "artifactPath": "builds/artifacts"
}
```

#### Trigger Deployment
```http
POST /api/cicd/jenkins/deployments/{deploymentJobName}
Content-Type: application/json

{
  "environment": "production",
  "buildArtifact": "build-123"
}
```

### Monitoring & Status

#### Get Server Information
```http
GET /api/cicd/jenkins/server/info
```

#### Get System Health
```http
GET /api/cicd/jenkins/server/health
```

#### Get Build Queue
```http
GET /api/cicd/jenkins/queue
```

## Usage Examples

### Creating a Pipeline for a Work Request

```csharp
// Inject the service
private readonly IJenkinsIntegrationService _jenkinsService;

// Create pipeline
var pipelineId = await _jenkinsService.CreatePipelineAsync(
    jobName: $"work-request-{workRequest.Id}",
    gitRepositoryUrl: "https://github.com/company/work-intake-system.git",
    jenkinsfilePath: "Jenkinsfile",
    workRequestId: workRequest.Id
);
```

### Triggering a Build with Parameters

```csharp
var buildParameters = new Dictionary<string, string>
{
    ["BRANCH"] = "feature/new-feature",
    ["ENVIRONMENT"] = "testing",
    ["WORK_REQUEST_ID"] = workRequest.Id.ToString()
};

var buildNumber = await _jenkinsService.TriggerBuildAsync(
    jobName: "work-request-123",
    parameters: buildParameters
);
```

### Monitoring Build Status

```csharp
var build = await _jenkinsService.GetBuildAsync("work-request-123", buildNumber);

if (build.Status == BuildStatus.Success)
{
    // Proceed with deployment
    await _jenkinsService.TriggerDeploymentAsync(
        deploymentJobName: "deploy-to-staging",
        environment: "staging",
        buildArtifact: $"build-{buildNumber}"
    );
}
```

### Getting Build Logs for Debugging

```csharp
var logs = await _jenkinsService.GetBuildLogsAsync("work-request-123", buildNumber);
_logger.LogInformation("Build logs: {Logs}", logs);
```

## Error Handling

The Jenkins integration includes comprehensive error handling:

```csharp
try
{
    var pipeline = await _jenkinsService.CreatePipelineAsync(jobName, repoUrl, jenkinsfile, workRequestId);
    return Ok(new { PipelineId = pipeline, JobName = jobName });
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "Network error connecting to Jenkins");
    return StatusCode(503, new { Error = "Jenkins server unavailable" });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating Jenkins pipeline {JobName}", jobName);
    return StatusCode(500, new { Error = "Failed to create Jenkins pipeline", Details = ex.Message });
}
```

## Security Considerations

1. **API Token Management**
   - Store Jenkins API tokens securely in configuration
   - Use environment variables for production tokens
   - Rotate tokens regularly

2. **Authentication**
   - The service uses Basic Authentication with username and API token
   - Ensure HTTPS is used in production environments

3. **Access Control**
   - Implement proper authorization in the API controller
   - Validate user permissions before allowing Jenkins operations

## Testing

### Unit Testing

```csharp
[Test]
public async Task CreatePipelineAsync_ValidParameters_ReturnsPipelineId()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    var mockLogger = new Mock<ILogger<JenkinsIntegrationService>>();
    var mockConfiguration = new Mock<IConfiguration>();
    
    // Setup configuration
    mockConfiguration.Setup(x => x["Jenkins:ServerUrl"]).Returns("http://jenkins:8080");
    mockConfiguration.Setup(x => x["Jenkins:Username"]).Returns("admin");
    mockConfiguration.Setup(x => x["Jenkins:ApiToken"]).Returns("token");
    
    var service = new JenkinsIntegrationService(mockLogger.Object, mockConfiguration.Object, mockHttpClient.Object);
    
    // Act
    var result = await service.CreatePipelineAsync("test-job", "https://github.com/test/repo.git", "Jenkinsfile", 1);
    
    // Assert
    Assert.IsNotNull(result);
}
```

### Integration Testing

```csharp
[Test]
public async Task JenkinsIntegration_EndToEnd_Workflow()
{
    // Create pipeline
    var pipelineId = await _jenkinsService.CreatePipelineAsync("test-pipeline", repoUrl, "Jenkinsfile", 1);
    Assert.IsNotNull(pipelineId);
    
    // Trigger build
    var buildNumber = await _jenkinsService.TriggerBuildAsync("test-pipeline");
    Assert.Greater(buildNumber, 0);
    
    // Get build status
    var build = await _jenkinsService.GetBuildAsync("test-pipeline", buildNumber);
    Assert.IsNotNull(build);
    
    // Cleanup
    await _jenkinsService.DeletePipelineAsync("test-pipeline");
}
```

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   - Verify username and API token are correct
   - Ensure the API token has sufficient permissions
   - Check if Jenkins requires CSRF protection

2. **Network Connectivity**
   - Verify Jenkins server URL is accessible
   - Check firewall settings
   - Ensure proper DNS resolution

3. **Job Configuration Errors**
   - Validate Jenkinsfile syntax
   - Check Git repository accessibility
   - Verify branch names exist

### Debugging

Enable detailed logging by setting the log level to Debug:

```json
{
  "Logging": {
    "LogLevel": {
      "WorkIntakeSystem.Infrastructure.Services.JenkinsIntegrationService": "Debug"
    }
  }
}
```

## Best Practices

1. **Pipeline Naming**
   - Use consistent naming conventions
   - Include work request IDs in pipeline names
   - Avoid special characters in job names

2. **Error Handling**
   - Always implement proper exception handling
   - Log errors with sufficient context
   - Provide meaningful error messages to users

3. **Performance**
   - Use appropriate timeouts for long-running operations
   - Implement retry logic for transient failures
   - Cache frequently accessed data when appropriate

4. **Monitoring**
   - Monitor Jenkins server health regularly
   - Track build success/failure rates
   - Set up alerts for critical failures

## Future Enhancements

1. **Pipeline Templates**
   - Predefined pipeline configurations for common scenarios
   - Template-based pipeline generation

2. **Advanced Monitoring**
   - Real-time build status updates
   - Webhook integration for build notifications
   - Dashboard for pipeline metrics

3. **Multi-Environment Support**
   - Environment-specific pipeline configurations
   - Automated promotion between environments
   - Environment-specific deployment strategies

4. **Integration with Work Requests**
   - Automatic pipeline creation for new work requests
   - Build status tracking in work request details
   - Deployment approval workflows 