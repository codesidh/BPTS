using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IWindowsAuthenticationService
{
    Task<User?> AuthenticateUserAsync(string username, string? domain = null);
    Task<User?> GetUserByWindowsIdentityAsync(string windowsIdentity);
    Task<bool> ValidateUserCredentialsAsync(string username, string password, string? domain = null);
    Task<IEnumerable<string>> GetUserGroupsAsync(string username, string? domain = null);
    Task<User?> GetUserFromLdapAsync(string username, string? domain = null);
    Task<bool> IsUserInGroupAsync(string username, string groupName, string? domain = null);
    void ClearUserCache(string username);
    Task<User> SynchronizeUserAsync(string username, string? domain = null);
} 