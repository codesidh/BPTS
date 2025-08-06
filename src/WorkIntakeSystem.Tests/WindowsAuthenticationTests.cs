using Microsoft.Extensions.Configuration;
using Moq;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Services;
using Xunit;

namespace WorkIntakeSystem.Tests;

public class WindowsAuthenticationTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IActiveDirectoryService> _mockAdService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly WindowsAuthenticationService _windowsAuthService;

    public WindowsAuthenticationTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAdService = new Mock<IActiveDirectoryService>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["Secret"]).Returns("YourSuperSecretKeyHereThatIsAtLeast32CharactersLong");
        jwtSection.Setup(x => x["Issuer"]).Returns("WorkIntakeSystem");
        jwtSection.Setup(x => x["Audience"]).Returns("WorkIntakeSystem");
        jwtSection.Setup(x => x.GetValue<int>("ExpirationHours", 24)).Returns(24);

        var windowsSection = new Mock<IConfigurationSection>();
        windowsSection.Setup(x => x.GetValue<bool>("Enabled", false)).Returns(true);
        windowsSection.Setup(x => x.GetValue<bool>("AutoCreateUsers", true)).Returns(true);

        var groupMappingSection = new Mock<IConfigurationSection>();
        groupMappingSection.Setup(x => x.Get<Dictionary<string, string>>()).Returns(new Dictionary<string, string>
        {
            ["IT-Admins"] = "Administrator",
            ["Department-Heads"] = "Manager",
            ["End-Users"] = "EndUser"
        });

        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);
        _mockConfiguration.Setup(x => x.GetSection("Authentication:Windows")).Returns(windowsSection.Object);
        _mockConfiguration.Setup(x => x.GetSection("Authentication:Windows:GroupMapping")).Returns(groupMappingSection.Object);

        _windowsAuthService = new WindowsAuthenticationService(
            _mockUserRepository.Object,
            _mockAdService.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task AuthenticateWindowsUserAsync_WithValidWindowsIdentity_ShouldReturnSuccess()
    {
        // Arrange
        var windowsIdentity = "DOMAIN\\admin";
        var mockAdUser = new ADUserInfo
        {
            SamAccountName = "admin",
            DisplayName = "System Administrator",
            Email = "admin@company.com",
            Department = "IT",
            Groups = new List<string> { "IT-Admins", "Department-Heads" }
        };

        _mockUserRepository.Setup(x => x.GetByWindowsIdentityAsync(windowsIdentity))
            .ReturnsAsync((User?)null);

        _mockAdService.Setup(x => x.GetUserFromActiveDirectoryAsync(windowsIdentity))
            .ReturnsAsync(mockAdUser);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _windowsAuthService.AuthenticateWindowsUserAsync(windowsIdentity);

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal(windowsIdentity, result.WindowsIdentity);
        Assert.NotNull(result.User);
        Assert.NotNull(result.JwtToken);
        Assert.Equal("System Administrator", result.User.Name);
        Assert.Equal("admin@company.com", result.User.Email);
        Assert.True(result.User.IsWindowsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateWindowsUserAsync_WithExistingUser_ShouldReturnSuccess()
    {
        // Arrange
        var windowsIdentity = "DOMAIN\\manager";
        var existingUser = new User
        {
            Id = 1,
            WindowsIdentity = windowsIdentity,
            Name = "Department Manager",
            Email = "manager@company.com",
            IsWindowsAuthenticated = true,
            Role = UserRole.Manager
        };

        _mockUserRepository.Setup(x => x.GetByWindowsIdentityAsync(windowsIdentity))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _windowsAuthService.AuthenticateWindowsUserAsync(windowsIdentity);

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal(windowsIdentity, result.WindowsIdentity);
        Assert.NotNull(result.User);
        Assert.NotNull(result.JwtToken);
        Assert.Equal("Department Manager", result.User.Name);
    }

    [Fact]
    public async Task AuthenticateWindowsUserAsync_WithInvalidWindowsIdentity_ShouldReturnFailure()
    {
        // Arrange
        var invalidWindowsIdentity = "invalid-format";

        // Act
        var result = await _windowsAuthService.AuthenticateWindowsUserAsync(invalidWindowsIdentity);

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Contains("Invalid Windows identity format", result.ErrorMessage);
    }

    [Fact]
    public async Task AuthenticateWindowsUserAsync_WithEmptyWindowsIdentity_ShouldReturnFailure()
    {
        // Arrange
        var emptyWindowsIdentity = "";

        // Act
        var result = await _windowsAuthService.AuthenticateWindowsUserAsync(emptyWindowsIdentity);

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Contains("Windows identity is null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task GetOrCreateUserFromWindowsIdentityAsync_WithNewUser_ShouldCreateUser()
    {
        // Arrange
        var windowsIdentity = "DOMAIN\\user";
        var mockAdUser = new ADUserInfo
        {
            SamAccountName = "user",
            DisplayName = "Regular User",
            Email = "user@company.com",
            Department = "Sales",
            Groups = new List<string> { "End-Users" }
        };

        _mockUserRepository.Setup(x => x.GetByWindowsIdentityAsync(windowsIdentity))
            .ReturnsAsync((User?)null);

        _mockAdService.Setup(x => x.GetUserFromActiveDirectoryAsync(windowsIdentity))
            .ReturnsAsync(mockAdUser);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _windowsAuthService.GetOrCreateUserFromWindowsIdentityAsync(windowsIdentity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(windowsIdentity, result.WindowsIdentity);
        Assert.Equal("Regular User", result.Name);
        Assert.Equal("user@company.com", result.Email);
        Assert.True(result.IsWindowsAuthenticated);
        Assert.Equal(UserRole.EndUser, result.Role);
    }

    [Fact]
    public async Task SyncUserFromActiveDirectoryAsync_WithValidUser_ShouldUpdateUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            WindowsIdentity = "DOMAIN\\admin",
            Name = "Old Name",
            Email = "old@company.com",
            IsWindowsAuthenticated = true
        };

        var mockAdUser = new ADUserInfo
        {
            SamAccountName = "admin",
            DisplayName = "Updated Name",
            Email = "updated@company.com",
            Department = "IT",
            Groups = new List<string> { "IT-Admins" }
        };

        _mockAdService.Setup(x => x.GetUserFromActiveDirectoryAsync(user.WindowsIdentity))
            .ReturnsAsync(mockAdUser);

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _windowsAuthService.SyncUserFromActiveDirectoryAsync(user);

        // Assert
        Assert.True(result);
        Assert.Equal("Updated Name", user.Name);
        Assert.Equal("updated@company.com", user.Email);
        Assert.Equal(UserRole.SystemAdministrator, user.Role);
    }

    [Fact]
    public async Task GenerateJwtTokenForWindowsUserAsync_WithValidUser_ShouldReturnToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            WindowsIdentity = "DOMAIN\\admin",
            Name = "System Administrator",
            Email = "admin@company.com",
            IsWindowsAuthenticated = true,
            Role = UserRole.SystemAdministrator,
            DepartmentId = 1,
            BusinessVerticalId = 1
        };

        // Act
        var token = await _windowsAuthService.GenerateJwtTokenForWindowsUserAsync(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task ValidateWindowsAuthenticationAsync_WhenEnabled_ShouldReturnSuccess()
    {
        // Act
        var result = await _windowsAuthService.ValidateWindowsAuthenticationAsync();

        // Assert
        Assert.True(result.IsAuthenticated);
    }

    [Theory]
    [InlineData("IT-Admins", UserRole.SystemAdministrator)]
    [InlineData("Department-Heads", UserRole.Manager)]
    [InlineData("End-Users", UserRole.EndUser)]
    [InlineData("Unknown-Group", UserRole.EndUser)] // Default role
    public async Task AuthenticateWindowsUserAsync_WithDifferentGroups_ShouldMapToCorrectRole(string group, UserRole expectedRole)
    {
        // Arrange
        var windowsIdentity = "DOMAIN\\testuser";
        var mockAdUser = new ADUserInfo
        {
            SamAccountName = "testuser",
            DisplayName = "Test User",
            Email = "test@company.com",
            Department = "Test",
            Groups = new List<string> { group }
        };

        _mockUserRepository.Setup(x => x.GetByWindowsIdentityAsync(windowsIdentity))
            .ReturnsAsync((User?)null);

        _mockAdService.Setup(x => x.GetUserFromActiveDirectoryAsync(windowsIdentity))
            .ReturnsAsync(mockAdUser);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _windowsAuthService.AuthenticateWindowsUserAsync(windowsIdentity);

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.NotNull(result.User);
        Assert.Equal(expectedRole, result.User.Role);
    }
} 