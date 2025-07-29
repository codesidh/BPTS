using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Entities;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IUserRepository userRepository, ILogger<RoleService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> AssignRoleAsync(int userId, UserRole role)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.Role = role;
            user.ModifiedDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign role {Role} to user {UserId}", role, userId);
            return false;
        }
    }

    public async Task<bool> RemoveRoleAsync(int userId, UserRole role)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Role != role)
                return false;

            user.Role = UserRole.EndUser; // Default to EndUser
            user.ModifiedDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Role {Role} removed from user {UserId}", role, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove role {Role} from user {UserId}", role, userId);
            return false;
        }
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        var userPermissions = GetPermissionsForRole(user.Role);
        return userPermissions.Contains(permission);
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return new List<string>();

        return GetPermissionsForRole(user.Role);
    }

    public async Task<List<UserRole>> GetUserRolesAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return new List<UserRole>();

        return new List<UserRole> { user.Role };
    }

    public async Task<bool> IsInRoleAsync(int userId, UserRole role)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.Role == role;
    }

    public async Task<bool> IsInAnyRoleAsync(int userId, params UserRole[] roles)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && roles.Contains(user.Role);
    }

    private static List<string> GetPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.SystemAdministrator => GetAllPermissions(),
            UserRole.BusinessExecutive => GetBusinessExecutivePermissions(),
            UserRole.Director => GetDirectorPermissions(),
            UserRole.Manager => GetManagerPermissions(),
            UserRole.Lead => GetLeadPermissions(),
            UserRole.EndUser => GetEndUserPermissions(),
            _ => new List<string>()
        };
    }

    private static List<string> GetAllPermissions()
    {
        return new List<string>
        {
            // Work Request Permissions
            Permissions.CreateWorkRequest,
            Permissions.ReadWorkRequest,
            Permissions.UpdateWorkRequest,
            Permissions.DeleteWorkRequest,
            Permissions.ApproveWorkRequest,
            Permissions.RejectWorkRequest,

            // User Management Permissions
            Permissions.CreateUser,
            Permissions.ReadUser,
            Permissions.UpdateUser,
            Permissions.DeleteUser,
            Permissions.AssignRole,

            // System Administration Permissions
            Permissions.SystemConfiguration,
            Permissions.ViewAuditLogs,
            Permissions.ManageIntegrations,

            // Analytics and Reporting Permissions
            Permissions.ViewAnalytics,
            Permissions.GenerateReports,
            Permissions.ExportData,

            // Priority Management Permissions
            Permissions.VotePriority,
            Permissions.ManagePriority,
            Permissions.OverridePriority
        };
    }

    private static List<string> GetBusinessExecutivePermissions()
    {
        return new List<string>
        {
            Permissions.ReadWorkRequest,
            Permissions.ApproveWorkRequest,
            Permissions.RejectWorkRequest,
            Permissions.ReadUser,
            Permissions.ViewAnalytics,
            Permissions.GenerateReports,
            Permissions.ExportData,
            Permissions.OverridePriority
        };
    }

    private static List<string> GetDirectorPermissions()
    {
        return new List<string>
        {
            Permissions.CreateWorkRequest,
            Permissions.ReadWorkRequest,
            Permissions.UpdateWorkRequest,
            Permissions.ApproveWorkRequest,
            Permissions.RejectWorkRequest,
            Permissions.ReadUser,
            Permissions.ViewAnalytics,
            Permissions.GenerateReports,
            Permissions.VotePriority,
            Permissions.ManagePriority
        };
    }

    private static List<string> GetManagerPermissions()
    {
        return new List<string>
        {
            Permissions.CreateWorkRequest,
            Permissions.ReadWorkRequest,
            Permissions.UpdateWorkRequest,
            Permissions.ApproveWorkRequest,
            Permissions.ReadUser,
            Permissions.ViewAnalytics,
            Permissions.VotePriority,
            Permissions.ManagePriority
        };
    }

    private static List<string> GetLeadPermissions()
    {
        return new List<string>
        {
            Permissions.CreateWorkRequest,
            Permissions.ReadWorkRequest,
            Permissions.UpdateWorkRequest,
            Permissions.ReadUser,
            Permissions.VotePriority
        };
    }

    private static List<string> GetEndUserPermissions()
    {
        return new List<string>
        {
            Permissions.CreateWorkRequest,
            Permissions.ReadWorkRequest,
            Permissions.VotePriority
        };
    }
} 