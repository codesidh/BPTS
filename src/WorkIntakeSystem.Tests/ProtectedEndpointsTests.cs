using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using WorkIntakeSystem.API;
using WorkIntakeSystem.Core.DTOs;
using WorkIntakeSystem.API.DTOs;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Data;
using Xunit;

namespace WorkIntakeSystem.Tests;

public class ProtectedEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProtectedEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Configure test host builder with JWT settings and service overrides
            TestConfiguration.ConfigureTestHostBuilder(builder);
        });

        _client = _factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequestDto
        {
            Name = "Protected Endpoints User",
            Email = "protected@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        return authResult!.Token;
    }

    [Fact]
    public async Task WorkRequestsController_WithoutAuth_ReturnsUnauthorized()
    {
        // Test GET /api/workrequests
        var response = await _client.GetAsync("/api/workrequests");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // Test POST /api/workrequests
        var workRequest = new WorkRequestDto
        {
            Title = "Test Request",
            Description = "Test Description",
            Priority = 5.0m,
            Category = WorkCategory.Project
        };
        var postResponse = await _client.PostAsJsonAsync("/api/workrequests", workRequest);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, postResponse.StatusCode);
    }

    [Fact]
    public async Task WorkRequestsController_WithAuth_ReturnsSuccess()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Test GET /api/workrequests
        var response = await _client.GetAsync("/api/workrequests");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // Test POST /api/workrequests
        var workRequest = new WorkRequestDto
        {
            Title = "Test Request",
            Description = "Test Description",
            Priority = 5.0m,
            Category = WorkCategory.Project
        };
        var postResponse = await _client.PostAsJsonAsync("/api/workrequests", workRequest);
        Assert.Equal(System.Net.HttpStatusCode.Created, postResponse.StatusCode);
    }

    [Fact]
    public async Task UsersController_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/users");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UsersController_WithAuth_ReturnsSuccess()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/users");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PriorityController_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/priority");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PriorityController_WithAuth_ReturnsSuccess()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/priority");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnalyticsController_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/analytics");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AnalyticsController_WithAuth_ReturnsSuccess()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/analytics");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SystemConfigurationController_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/systemconfiguration");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SystemConfigurationController_WithAuth_ReturnsSuccess()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/systemconfiguration");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RoleController_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/role");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RoleController_WithAuth_ReturnsSuccess()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/role");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidToken_AllProtectedEndpoints_ReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

        var endpoints = new[]
        {
            "/api/workrequests",
            "/api/users",
            "/api/priority",
            "/api/analytics",
            "/api/systemconfiguration",
            "/api/role"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    [Fact]
    public async Task ExpiredToken_AllProtectedEndpoints_ReturnUnauthorized()
    {
        // Create a token and then wait for it to expire (in a real scenario)
        // For testing, we'll use an expired token format
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        var endpoints = new[]
        {
            "/api/workrequests",
            "/api/users",
            "/api/priority",
            "/api/analytics",
            "/api/systemconfiguration",
            "/api/role"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
} 