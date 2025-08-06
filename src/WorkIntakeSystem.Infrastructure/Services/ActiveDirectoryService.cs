using Microsoft.Extensions.Configuration;
using WorkIntakeSystem.Core.Interfaces;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace WorkIntakeSystem.Infrastructure.Services;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly IConfiguration _configuration;
    private readonly string _domain;
    private readonly string _ldapServer;
    private readonly int _ldapPort;
    private readonly string _ldapUsername;
    private readonly string _ldapPassword;
    private readonly string _searchBase;
    private readonly bool _adEnabled;

    public ActiveDirectoryService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var adConfig = configuration.GetSection("Authentication:Windows");
        _adEnabled = adConfig.GetValue<bool>("Enabled", false);
        _domain = adConfig["Domain"] ?? "";
        _ldapServer = adConfig["LdapServer"] ?? "";
        _ldapPort = adConfig.GetValue<int>("LdapPort", 389);
        _ldapUsername = adConfig["LdapUsername"] ?? "";
        _ldapPassword = adConfig["LdapPassword"] ?? "";
        _searchBase = adConfig["SearchBase"] ?? "";
    }

    public async Task<ADUserInfo?> GetUserFromActiveDirectoryAsync(string username)
    {
        if (!_adEnabled)
        {
            return await Task.FromResult<ADUserInfo?>(null);
        }

        try
        {
            // For development/testing, return mock data
            if (string.IsNullOrEmpty(_ldapServer) || _ldapServer.Contains("your-domain"))
            {
                return await GetMockAdUserAsync(username);
            }

            // Real AD implementation would go here
            return await GetRealAdUserAsync(username);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"AD lookup failed for {username}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<string>> GetUserGroupsAsync(string username)
    {
        if (!_adEnabled)
        {
            return await Task.FromResult(new List<string>());
        }

        try
        {
            var adUser = await GetUserFromActiveDirectoryAsync(username);
            return adUser?.Groups ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        if (!_adEnabled)
        {
            return await Task.FromResult(false);
        }

        try
        {
            // For development/testing, return true for known users
            if (string.IsNullOrEmpty(_ldapServer) || _ldapServer.Contains("your-domain"))
            {
                return await Task.FromResult(IsKnownUser(username));
            }

            // Real AD validation would go here
            return await ValidateRealAdCredentialsAsync(username, password);
        }
        catch
        {
            return false;
        }
    }

    public async Task<ADUserInfo?> SearchUserByEmailAsync(string email)
    {
        if (!_adEnabled)
        {
            return await Task.FromResult<ADUserInfo?>(null);
        }

        try
        {
            // For development/testing, return mock data
            if (string.IsNullOrEmpty(_ldapServer) || _ldapServer.Contains("your-domain"))
            {
                return await GetMockAdUserByEmailAsync(email);
            }

            // Real AD search would go here
            return await SearchRealAdUserByEmailAsync(email);
        }
        catch
        {
            return null;
        }
    }

    private async Task<ADUserInfo> GetMockAdUserAsync(string username)
    {
        // Extract username from Windows identity format (DOMAIN\username)
        var cleanUsername = username.Contains('\\') ? username.Split('\\')[1] : username;
        
        // Mock AD user data for development
        var mockUsers = new Dictionary<string, ADUserInfo>
        {
            ["admin"] = new ADUserInfo
            {
                SamAccountName = "admin",
                DisplayName = "System Administrator",
                Email = "admin@company.com",
                Department = "IT",
                Groups = new List<string> { "IT-Admins", "Department-Heads" },
                Attributes = new Dictionary<string, string>
                {
                    ["title"] = "System Administrator",
                    ["department"] = "IT"
                }
            },
            ["manager"] = new ADUserInfo
            {
                SamAccountName = "manager",
                DisplayName = "Department Manager",
                Email = "manager@company.com",
                Department = "Operations",
                Groups = new List<string> { "Department-Heads", "End-Users" },
                Attributes = new Dictionary<string, string>
                {
                    ["title"] = "Department Manager",
                    ["department"] = "Operations"
                }
            },
            ["user"] = new ADUserInfo
            {
                SamAccountName = "user",
                DisplayName = "Regular User",
                Email = "user@company.com",
                Department = "Sales",
                Groups = new List<string> { "End-Users" },
                Attributes = new Dictionary<string, string>
                {
                    ["title"] = "Sales Representative",
                    ["department"] = "Sales"
                }
            }
        };

        return await Task.FromResult(mockUsers.TryGetValue(cleanUsername.ToLower(), out var user) ? user : null);
    }

    private async Task<ADUserInfo> GetMockAdUserByEmailAsync(string email)
    {
        var mockUsers = new Dictionary<string, ADUserInfo>
        {
            ["admin@company.com"] = new ADUserInfo
            {
                SamAccountName = "admin",
                DisplayName = "System Administrator",
                Email = "admin@company.com",
                Department = "IT",
                Groups = new List<string> { "IT-Admins", "Department-Heads" }
            },
            ["manager@company.com"] = new ADUserInfo
            {
                SamAccountName = "manager",
                DisplayName = "Department Manager",
                Email = "manager@company.com",
                Department = "Operations",
                Groups = new List<string> { "Department-Heads", "End-Users" }
            },
            ["user@company.com"] = new ADUserInfo
            {
                SamAccountName = "user",
                DisplayName = "Regular User",
                Email = "user@company.com",
                Department = "Sales",
                Groups = new List<string> { "End-Users" }
            }
        };

        return await Task.FromResult(mockUsers.TryGetValue(email.ToLower(), out var user) ? user : null);
    }

    private bool IsKnownUser(string username)
    {
        var cleanUsername = username.Contains('\\') ? username.Split('\\')[1] : username;
        var knownUsers = new[] { "admin", "manager", "user" };
        return knownUsers.Contains(cleanUsername.ToLower());
    }

    private async Task<ADUserInfo> GetRealAdUserAsync(string username)
    {
        // This would contain real AD implementation using System.DirectoryServices
        // For now, return null to indicate no real AD connection
        return await Task.FromResult<ADUserInfo>(null);
    }

    private async Task<bool> ValidateRealAdCredentialsAsync(string username, string password)
    {
        // This would contain real AD credential validation
        // For now, return false to indicate no real AD connection
        return await Task.FromResult(false);
    }

    private async Task<ADUserInfo> SearchRealAdUserByEmailAsync(string email)
    {
        // This would contain real AD search implementation
        // For now, return null to indicate no real AD connection
        return await Task.FromResult<ADUserInfo>(null);
    }
} 