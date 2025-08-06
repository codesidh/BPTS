using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Enums;
using System.Text.Json;

namespace WorkIntakeSystem.Infrastructure.Services;

public class WindowsAuthenticationService : IWindowsAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IActiveDirectoryService _adService;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationHours;
    private readonly bool _windowsAuthEnabled;
    private readonly bool _autoCreateUsers;

    public WindowsAuthenticationService(
        IUserRepository userRepository,
        IActiveDirectoryService adService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _adService = adService;
        _configuration = configuration;
        
        var jwtConfig = configuration.GetSection("JwtSettings");
        _jwtSecret = jwtConfig["Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
        _jwtIssuer = jwtConfig["Issuer"] ?? "WorkIntakeSystem";
        _jwtAudience = jwtConfig["Audience"] ?? "WorkIntakeSystem";
        _jwtExpirationHours = jwtConfig.GetValue<int>("ExpirationHours", 24);
        
        var windowsConfig = configuration.GetSection("Authentication:Windows");
        _windowsAuthEnabled = windowsConfig.GetValue<bool>("Enabled", false);
        _autoCreateUsers = windowsConfig.GetValue<bool>("AutoCreateUsers", true);
    }

    public async Task<WindowsAuthResult> AuthenticateWindowsUserAsync(string windowsIdentity)
    {
        if (!_windowsAuthEnabled)
        {
            return new WindowsAuthResult
            {
                IsAuthenticated = false,
                ErrorMessage = "Windows Authentication is not enabled"
            };
        }

        if (string.IsNullOrEmpty(windowsIdentity))
        {
            return new WindowsAuthResult
            {
                IsAuthenticated = false,
                ErrorMessage = "Windows identity is null or empty"
            };
        }

        try
        {
            // Extract domain and username from Windows identity (format: DOMAIN\username)
            var parts = windowsIdentity.Split('\\');
            if (parts.Length != 2)
            {
                return new WindowsAuthResult
                {
                    IsAuthenticated = false,
                    ErrorMessage = "Invalid Windows identity format"
                };
            }

            var domain = parts[0];
            var username = parts[1];

            // Get or create user from Windows identity
            var user = await GetOrCreateUserFromWindowsIdentityAsync(windowsIdentity);
            if (user == null)
            {
                return new WindowsAuthResult
                {
                    IsAuthenticated = false,
                    ErrorMessage = "User not found and auto-creation is disabled"
                };
            }

            // Sync user information from Active Directory
            await SyncUserFromActiveDirectoryAsync(user);

            // Generate JWT token for Windows authenticated user
            var jwtToken = await GenerateJwtTokenForWindowsUserAsync(user);

            return new WindowsAuthResult
            {
                IsAuthenticated = true,
                WindowsIdentity = windowsIdentity,
                User = user,
                JwtToken = jwtToken
            };
        }
        catch (Exception ex)
        {
            return new WindowsAuthResult
            {
                IsAuthenticated = false,
                ErrorMessage = $"Windows authentication failed: {ex.Message}"
            };
        }
    }

    public async Task<User?> GetOrCreateUserFromWindowsIdentityAsync(string windowsIdentity)
    {
        // First, try to find existing user by Windows identity
        var existingUser = await _userRepository.GetByWindowsIdentityAsync(windowsIdentity);
        if (existingUser != null)
        {
            return existingUser;
        }

        // If auto-creation is disabled, return null
        if (!_autoCreateUsers)
        {
            return null;
        }

        // Try to get user information from Active Directory
        var adUser = await _adService.GetUserFromActiveDirectoryAsync(windowsIdentity);
        if (adUser == null)
        {
            return null;
        }

        // Create new user from AD information
        var newUser = new User
        {
            WindowsIdentity = windowsIdentity,
            ActiveDirectorySid = adUser.SamAccountName,
            Name = adUser.DisplayName,
            Email = adUser.Email,
            IsWindowsAuthenticated = true,
            LastAdSync = DateTime.UtcNow,
            AdGroups = JsonSerializer.Serialize(adUser.Groups),
            Role = MapAdGroupsToRole(adUser.Groups),
            DepartmentId = 1, // Default department - should be mapped from AD
            BusinessVerticalId = 1, // Default business vertical - should be mapped from AD
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        // Save the new user
        await _userRepository.AddAsync(newUser);
        return newUser;
    }

    public async Task<bool> SyncUserFromActiveDirectoryAsync(User user)
    {
        if (string.IsNullOrEmpty(user.WindowsIdentity))
        {
            return false;
        }

        try
        {
            var adUser = await _adService.GetUserFromActiveDirectoryAsync(user.WindowsIdentity);
            if (adUser == null)
            {
                return false;
            }

            // Update user information from AD
            user.Name = adUser.DisplayName;
            user.Email = adUser.Email;
            user.AdGroups = JsonSerializer.Serialize(adUser.Groups);
            user.Role = MapAdGroupsToRole(adUser.Groups);
            user.LastAdSync = DateTime.UtcNow;
            user.ModifiedDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GenerateJwtTokenForWindowsUserAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("DepartmentId", user.DepartmentId.ToString()),
            new("BusinessVerticalId", user.BusinessVerticalId.ToString()),
            new("WindowsIdentity", user.WindowsIdentity ?? ""),
            new("AuthenticationType", "Windows"),
            new("IsWindowsAuthenticated", user.IsWindowsAuthenticated.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtExpirationHours),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<WindowsAuthResult> ValidateWindowsAuthenticationAsync()
    {
        if (!_windowsAuthEnabled)
        {
            return new WindowsAuthResult
            {
                IsAuthenticated = false,
                ErrorMessage = "Windows Authentication is not enabled"
            };
        }

        return new WindowsAuthResult
        {
            IsAuthenticated = true
        };
    }

    private UserRole MapAdGroupsToRole(List<string> adGroups)
    {
        var groupMapping = _configuration.GetSection("Authentication:Windows:GroupMapping").Get<Dictionary<string, string>>() 
            ?? new Dictionary<string, string>();

        foreach (var group in adGroups)
        {
            if (groupMapping.TryGetValue(group, out var roleName))
            {
                if (Enum.TryParse<UserRole>(roleName, out var role))
                {
                    return role;
                }
            }
        }

        return UserRole.EndUser; // Default role
    }
} 