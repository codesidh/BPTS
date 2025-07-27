using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Infrastructure.Services;

public class WindowsAuthenticationService : IWindowsAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WindowsAuthenticationService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _ldapServer;
    private readonly string _ldapBaseDn;
    private readonly string _domain;
    private readonly string? _serviceAccount;
    private readonly string? _servicePassword;
    private readonly bool _useSSL;
    private readonly int _cacheExpirationMinutes;

    public WindowsAuthenticationService(
        IConfiguration configuration, 
        ILogger<WindowsAuthenticationService> logger,
        IMemoryCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
        
        var authConfig = configuration.GetSection("WindowsAuthentication");
        _ldapServer = authConfig["LdapServer"] ?? throw new InvalidOperationException("LDAP server not configured");
        _ldapBaseDn = authConfig["LdapBaseDn"] ?? throw new InvalidOperationException("LDAP base DN not configured");
        _domain = authConfig["Domain"] ?? throw new InvalidOperationException("Domain not configured");
        _serviceAccount = authConfig["LdapServiceAccount"];
        _servicePassword = authConfig["LdapServicePassword"];
        _useSSL = authConfig.GetValue<bool>("UseSSL", true);
        _cacheExpirationMinutes = authConfig.GetValue<int>("CacheExpirationMinutes", 30);
    }

    public async Task<User?> AuthenticateUserAsync(string username, string? domain = null)
    {
        try
        {
            domain ??= _domain;
            var cacheKey = $"user_{domain}_{username}";
            
            if (_cache.TryGetValue(cacheKey, out User? cachedUser))
            {
                _logger.LogDebug("Retrieved user {Username} from cache", username);
                return cachedUser;
            }

            var user = await GetUserFromLdapAsync(username, domain);
            if (user != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheExpirationMinutes)
                };
                _cache.Set(cacheKey, user, cacheOptions);
                _logger.LogInformation("Successfully authenticated user {Username} from domain {Domain}", username, domain);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate user {Username} from domain {Domain}", username, domain);
            return null;
        }
    }

    public async Task<User?> GetUserByWindowsIdentityAsync(string windowsIdentity)
    {
        try
        {
            // Parse domain\username format
            var parts = windowsIdentity.Split('\\');
            if (parts.Length == 2)
            {
                return await AuthenticateUserAsync(parts[1], parts[0]);
            }
            else
            {
                return await AuthenticateUserAsync(windowsIdentity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by Windows identity {Identity}", windowsIdentity);
            return null;
        }
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password, string? domain = null)
    {
        try
        {
            domain ??= _domain;
            
            using var context = new PrincipalContext(ContextType.Domain, domain);
            var isValid = context.ValidateCredentials(username, password);
            
            _logger.LogDebug("Credential validation for user {Username} in domain {Domain}: {IsValid}", 
                username, domain, isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate credentials for user {Username} in domain {Domain}", 
                username, domain);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserGroupsAsync(string username, string? domain = null)
    {
        try
        {
            domain ??= _domain;
            var groups = new List<string>();

            using var context = new PrincipalContext(ContextType.Domain, domain);
            using var user = UserPrincipal.FindByIdentity(context, username);
            
            if (user != null)
            {
                var userGroups = user.GetAuthorizationGroups();
                groups.AddRange(userGroups.Select(g => g.Name).Where(name => !string.IsNullOrEmpty(name))!);
            }

            _logger.LogDebug("Retrieved {GroupCount} groups for user {Username}", groups.Count, username);
            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get groups for user {Username} in domain {Domain}", username, domain);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<User?> GetUserFromLdapAsync(string username, string? domain = null)
    {
        try
        {
            domain ??= _domain;
            DirectoryEntry? entry = null;
            DirectorySearcher? searcher = null;

            try
            {
                // Create directory entry
                if (!string.IsNullOrEmpty(_serviceAccount) && !string.IsNullOrEmpty(_servicePassword))
                {
                    entry = new DirectoryEntry(_ldapServer, _serviceAccount, _servicePassword);
                }
                else
                {
                    entry = new DirectoryEntry(_ldapServer);
                }

                searcher = new DirectorySearcher(entry)
                {
                    Filter = $"(&(objectClass=user)(sAMAccountName={username}))",
                    PropertiesToLoad = { "sAMAccountName", "displayName", "mail", "memberOf", "department" }
                };

                var result = searcher.FindOne();
                if (result?.Properties != null)
                {
                    var user = new User
                    {
                        Email = GetPropertyValue(result.Properties, "mail") ?? $"{username}@{domain}",
                        Name = GetPropertyValue(result.Properties, "displayName") ?? username,
                        WindowsIdentity = $"{domain}\\{username}",
                        Role = await DetermineUserRoleAsync(username, domain),
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = "System",
                        ModifiedBy = "System"
                    };

                    // Set department if available
                    var departmentName = GetPropertyValue(result.Properties, "department");
                    if (!string.IsNullOrEmpty(departmentName))
                    {
                        // TODO: Map to actual department entity
                        _logger.LogDebug("User {Username} belongs to department {Department}", username, departmentName);
                    }

                    return user;
                }
            }
            finally
            {
                searcher?.Dispose();
                entry?.Dispose();
            }

            _logger.LogWarning("User {Username} not found in LDAP directory", username);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user {Username} from LDAP", username);
            return null;
        }
    }

    public async Task<bool> IsUserInGroupAsync(string username, string groupName, string? domain = null)
    {
        try
        {
            var groups = await GetUserGroupsAsync(username, domain);
            return groups.Contains(groupName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user {Username} is in group {GroupName}", username, groupName);
            return false;
        }
    }

    public void ClearUserCache(string username)
    {
        var cacheKey = $"user_{_domain}_{username}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("Cleared cache for user {Username}", username);
    }

    public async Task<User> SynchronizeUserAsync(string username, string? domain = null)
    {
        ClearUserCache(username);
        var user = await AuthenticateUserAsync(username, domain);
        if (user == null)
        {
            throw new InvalidOperationException($"User {username} not found in directory");
        }
        return user;
    }

    private static string? GetPropertyValue(ResultPropertyCollection properties, string propertyName)
    {
        if (properties.Contains(propertyName) && properties[propertyName].Count > 0)
        {
            return properties[propertyName][0]?.ToString();
        }
        return null;
    }

    private async Task<UserRole> DetermineUserRoleAsync(string username, string domain)
    {
        try
        {
            var groups = await GetUserGroupsAsync(username, domain);
            
            // Map AD groups to application roles
            if (groups.Any(g => g.Contains("System Administrator", StringComparison.OrdinalIgnoreCase) ||
                               g.Contains("IT Admin", StringComparison.OrdinalIgnoreCase)))
            {
                return UserRole.SystemAdministrator;
            }
            
            if (groups.Any(g => g.Contains("Department Manager", StringComparison.OrdinalIgnoreCase) ||
                               g.Contains("Manager", StringComparison.OrdinalIgnoreCase)))
            {
                return UserRole.DepartmentManager;
            }
            
            if (groups.Any(g => g.Contains("Business Analyst", StringComparison.OrdinalIgnoreCase) ||
                               g.Contains("Analyst", StringComparison.OrdinalIgnoreCase)))
            {
                return UserRole.BusinessAnalyst;
            }

            // Default role
            return UserRole.Submitter;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine role for user {Username}", username);
            return UserRole.Submitter;
        }
    }
} 