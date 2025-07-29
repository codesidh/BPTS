using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IJwtAuthenticationService
{
    Task<string> GenerateJwtTokenAsync(User user);
    Task<User?> ValidateUserAsync(string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> ValidateJwtTokenAsync(string token);
    Task<User?> GetUserFromTokenAsync(string token);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string email);
    Task<bool> IsEmailUniqueAsync(string email);
} 