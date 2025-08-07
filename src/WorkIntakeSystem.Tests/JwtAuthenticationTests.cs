using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using WorkIntakeSystem.API;
using WorkIntakeSystem.API.DTOs;
using WorkIntakeSystem.Core.DTOs;
using WorkIntakeSystem.Infrastructure.Data;
using Xunit;

namespace WorkIntakeSystem.Tests;

public class JwtAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public JwtAuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Configure test host builder with JWT settings and service overrides
            TestConfiguration.ConfigureTestHostBuilder(builder);
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ValidateToken_WithStaticTestToken_ReturnsSuccess()
    {
        // Arrange - Create a token using our static provider
        var token = JwtTokenProvider.GenerateJwtToken("testuser", "test@example.com", "User");

        var validateRequest = new ValidateTokenRequestDto
        {
            Token = token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", validateRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success but got {response.StatusCode}");
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithStaticTestToken_ReturnsSuccess()
    {
        // Arrange - Create a token using our static provider
        var token = JwtTokenProvider.GenerateJwtToken("testuser", "test@example.com", "User");

        // Set authorization header
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/protected");

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success but got {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello testuser", content);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/protected");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
