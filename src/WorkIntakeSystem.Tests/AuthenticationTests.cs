using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WorkIntakeSystem.API;
using WorkIntakeSystem.Core.DTOs;
using WorkIntakeSystem.API.DTOs;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Data;
using Xunit;

namespace WorkIntakeSystem.Tests;

public class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Configure test host builder with JWT settings and service overrides
            TestConfiguration.ConfigureTestHostBuilder(builder);
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("Test User", result.User.Name);
        Assert.Equal("test@example.com", result.User.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Test User",
            Email = "duplicate@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        // Register first user
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Try to register with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Login Test User",
            Email = "login@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "login@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("login@example.com", result.User.Email);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUser()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Me Test User",
            Email = "me@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal("me@example.com", user.Email);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsUser()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Token Test User",
            Email = "token@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        var validateRequest = new ValidateTokenRequestDto
        {
            Token = authResult!.Token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", validateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal("token@example.com", user.Email);
    }

    [Fact]
    public async Task ValidateToken_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var validateRequest = new ValidateTokenRequestDto
        {
            Token = "invalid.token.here"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", validateRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ValidEmail_ReturnsSuccess()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Reset Test User",
            Email = "reset@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var resetRequest = new ResetPasswordRequestDto
        {
            Email = "reset@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ResetPassword_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var resetRequest = new ResetPasswordRequestDto
        {
            Email = "nonexistent@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Change Password User",
            Email = "changepass@example.com",
            Password = "OldPassword123!",
            ConfirmPassword = "OldPassword123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        var changePasswordRequest = new ChangePasswordRequestDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", changePasswordRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ChangePassword_InvalidCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Name = "Change Password User 2",
            Email = "changepass2@example.com",
            Password = "OldPassword123!",
            ConfirmPassword = "OldPassword123!",
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        var changePasswordRequest = new ChangePasswordRequestDto
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", changePasswordRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
} 