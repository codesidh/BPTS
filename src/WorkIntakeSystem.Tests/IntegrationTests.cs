using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkIntakeSystem.API;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Data;
using Xunit;

namespace WorkIntakeSystem.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                // Configure test host builder with JWT settings and service overrides
                TestConfiguration.ConfigureTestHostBuilder(builder);
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateWorkRequest_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var workRequest = new
            {
                title = "Test Integration Work Request",
                description = "This is a test work request for integration testing",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = 40,
                businessValue = 0.8m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/workrequests", workRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<WorkRequest>();
            Assert.NotNull(result);
            Assert.Equal(workRequest.title, result.Title);
            Assert.Equal(workRequest.description, result.Description);
        }

        [Fact]
        public async Task GetWorkRequests_ReturnsWorkRequests()
        {
            // Arrange - Create a work request first
            var workRequest = new
            {
                title = "Test Work Request for List",
                description = "Test description",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = 20,
                businessValue = 0.6m
            };

            await _client.PostAsJsonAsync("/api/workrequests", workRequest);

            // Act
            var response = await _client.GetAsync("/api/workrequests");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var workRequests = await response.Content.ReadFromJsonAsync<List<WorkRequest>>();
            Assert.NotNull(workRequests);
            Assert.True(workRequests.Count > 0);
        }

        [Fact]
        public async Task PriorityVoting_CompleteWorkflow_ReturnsSuccess()
        {
            // Arrange - Create a work request
            var workRequest = new
            {
                title = "Test Priority Voting Work Request",
                description = "Test description for priority voting",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = 30,
                businessValue = 0.7m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/workrequests", workRequest);
            var createdWorkRequest = await createResponse.Content.ReadFromJsonAsync<WorkRequest>();

            // Act - Submit a priority vote
            var vote = new
            {
                workRequestId = createdWorkRequest.Id,
                vote = PriorityVote.High,
                businessValueScore = 0.8m,
                strategicAlignment = 0.7m,
                comments = "High priority for business impact"
            };

            var voteResponse = await _client.PostAsJsonAsync("/api/priority/vote", vote);

            // Assert
            Assert.Equal(HttpStatusCode.OK, voteResponse.StatusCode);
            
            var voteResult = await voteResponse.Content.ReadFromJsonAsync<PriorityVoteResponse>();
            Assert.NotNull(voteResult);
            Assert.True(voteResult.Success);
        }

        [Fact]
        public async Task GetVotingStatus_ValidWorkRequest_ReturnsStatus()
        {
            // Arrange - Create a work request and submit a vote
            var workRequest = new
            {
                title = "Test Voting Status Work Request",
                description = "Test description",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = 25,
                businessValue = 0.6m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/workrequests", workRequest);
            var createdWorkRequest = await createResponse.Content.ReadFromJsonAsync<WorkRequest>();

            var vote = new
            {
                workRequestId = createdWorkRequest.Id,
                vote = PriorityVote.Medium,
                businessValueScore = 0.6m,
                strategicAlignment = 0.5m,
                comments = "Medium priority"
            };

            await _client.PostAsJsonAsync("/api/priority/vote", vote);

            // Act - Get voting status
            var statusResponse = await _client.GetAsync($"/api/priority/status/{createdWorkRequest.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
            
            var status = await statusResponse.Content.ReadFromJsonAsync<PriorityVotingStatus>();
            Assert.NotNull(status);
            Assert.Equal(createdWorkRequest.Id, status.WorkRequestId);
            Assert.True(status.VotedDepartments > 0);
        }

        [Fact]
        public async Task Analytics_Dashboard_ReturnsAnalytics()
        {
            // Arrange - Create some work requests first
            var workRequests = new[]
            {
                new { title = "Analytics Test 1", description = "Test 1", category = WorkCategory.WorkRequest, businessVerticalId = 1, departmentId = 1, estimatedEffort = 20, businessValue = 0.7m },
                new { title = "Analytics Test 2", description = "Test 2", category = WorkCategory.Project, businessVerticalId = 1, departmentId = 2, estimatedEffort = 40, businessValue = 0.8m },
                new { title = "Analytics Test 3", description = "Test 3", category = WorkCategory.BreakFix, businessVerticalId = 1, departmentId = 1, estimatedEffort = 10, businessValue = 0.5m }
            };

            foreach (var wr in workRequests)
            {
                await _client.PostAsJsonAsync("/api/workrequests", wr);
            }

            // Act
            var response = await _client.GetAsync("/api/analytics/dashboard");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var analytics = await response.Content.ReadFromJsonAsync<DashboardAnalytics>();
            Assert.NotNull(analytics);
            Assert.True(analytics.TotalActiveRequests >= 3);
        }

        [Fact]
        public async Task Workflow_AdvanceWorkflow_ReturnsSuccess()
        {
            // Arrange - Create a work request
            var workRequest = new
            {
                title = "Test Workflow Work Request",
                description = "Test description for workflow",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = 35,
                businessValue = 0.8m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/workrequests", workRequest);
            var createdWorkRequest = await createResponse.Content.ReadFromJsonAsync<WorkRequest>();

            // Act - Advance workflow
            var advanceRequest = new
            {
                nextStage = WorkflowStage.BusinessReview,
                comments = "Advancing to business review"
            };

            var advanceResponse = await _client.PostAsJsonAsync($"/api/workrequests/{createdWorkRequest.Id}/advance-workflow", advanceRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, advanceResponse.StatusCode);
        }

        [Fact]
        public async Task Configuration_GetConfiguration_ReturnsConfiguration()
        {
            // Act
            var response = await _client.GetAsync("/api/configuration");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var configurations = await response.Content.ReadFromJsonAsync<List<SystemConfiguration>>();
            Assert.NotNull(configurations);
        }

        [Fact]
        public async Task EventStore_GetEvents_ReturnsEvents()
        {
            // Arrange - Create a work request to generate events
            var workRequest = new
            {
                title = "Test Event Store Work Request",
                description = "Test description for event store",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = 25,
                businessValue = 0.6m
            };

            await _client.PostAsJsonAsync("/api/workrequests", workRequest);

            // Act
            var response = await _client.GetAsync("/api/eventstore");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var events = await response.Content.ReadFromJsonAsync<List<EventStore>>();
            Assert.NotNull(events);
        }

        [Fact]
        public async Task ExternalIntegrations_GetIntegrations_ReturnsIntegrations()
        {
            // Act
            var response = await _client.GetAsync("/api/externalintegrations");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var integrations = await response.Content.ReadFromJsonAsync<List<object>>();
            Assert.NotNull(integrations);
        }

        [Fact]
        public async Task InvalidWorkRequest_ReturnsBadRequest()
        {
            // Arrange - Invalid work request (missing required fields)
            var invalidWorkRequest = new
            {
                title = "", // Empty title
                description = "Test description",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = -10, // Invalid effort
                businessValue = 1.5m // Invalid business value
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/workrequests", invalidWorkRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task NonExistentWorkRequest_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/workrequests/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PriorityVoting_DuplicateVote_ReturnsBadRequest()
        {
            // Arrange - Create a work request
            var workRequest = new
            {
                title = "Test Duplicate Vote Work Request",
                description = "Test description",
                category = WorkCategory.WorkRequest,
                businessVerticalId = 1,
                departmentId = 1,
                estimatedEffort = 20,
                businessValue = 0.6m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/workrequests", workRequest);
            var createdWorkRequest = await createResponse.Content.ReadFromJsonAsync<WorkRequest>();

            var vote = new
            {
                workRequestId = createdWorkRequest.Id,
                vote = PriorityVote.High,
                businessValueScore = 0.8m,
                strategicAlignment = 0.7m,
                comments = "First vote"
            };

            // Submit first vote
            await _client.PostAsJsonAsync("/api/priority/vote", vote);

            // Act - Submit duplicate vote
            var duplicateVoteResponse = await _client.PostAsJsonAsync("/api/priority/vote", vote);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, duplicateVoteResponse.StatusCode);
        }
    }

    // Helper classes for JSON deserialization
    public class PriorityVoteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NewPriorityScore { get; set; }
    }

    public class PriorityVotingStatus
    {
        public int WorkRequestId { get; set; }
        public string WorkRequestTitle { get; set; } = string.Empty;
        public int TotalDepartments { get; set; }
        public int VotedDepartments { get; set; }
        public int PendingDepartments { get; set; }
        public decimal CurrentPriorityScore { get; set; }
        public PriorityLevel CurrentPriorityLevel { get; set; }
        public List<object> Votes { get; set; } = new();
    }

    public class DashboardAnalytics
    {
        public int TotalActiveRequests { get; set; }
        public int TotalCompletedRequests { get; set; }
        public decimal AverageCompletionTime { get; set; }
        public decimal SLAComplianceRate { get; set; }
        public decimal ResourceUtilization { get; set; }
        public Dictionary<string, int> RequestsByCategory { get; set; } = new();
        public Dictionary<string, int> RequestsByPriority { get; set; } = new();
        public Dictionary<string, int> RequestsByStatus { get; set; } = new();
        public List<object> RecentActivities { get; set; } = new();
    }
} 