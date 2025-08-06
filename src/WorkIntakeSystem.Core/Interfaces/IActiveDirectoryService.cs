namespace WorkIntakeSystem.Core.Interfaces;

public interface IActiveDirectoryService
{
    Task<ADUserInfo?> GetUserFromActiveDirectoryAsync(string username);
    Task<List<string>> GetUserGroupsAsync(string username);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
    Task<ADUserInfo?> SearchUserByEmailAsync(string email);
}

public class ADUserInfo
{
    public string SamAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new List<string>();
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
} 