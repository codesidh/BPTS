using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IWindowsAuthenticationService
{
    Task<WindowsAuthResult> AuthenticateWindowsUserAsync(string windowsIdentity);
    Task<User?> GetOrCreateUserFromWindowsIdentityAsync(string windowsIdentity);
    Task<bool> SyncUserFromActiveDirectoryAsync(User user);
    Task<string> GenerateJwtTokenForWindowsUserAsync(User user);
    Task<WindowsAuthResult> ValidateWindowsAuthenticationAsync();
}

public class WindowsAuthResult
{
    public bool IsAuthenticated { get; set; }
    public string? WindowsIdentity { get; set; }
    public User? User { get; set; }
    public string? JwtToken { get; set; }
    public string? ErrorMessage { get; set; }
} 