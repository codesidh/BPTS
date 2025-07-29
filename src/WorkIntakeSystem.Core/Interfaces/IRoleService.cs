using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IRoleService
{
    Task<bool> AssignRoleAsync(int userId, UserRole role);
    Task<bool> RemoveRoleAsync(int userId, UserRole role);
    Task<bool> HasPermissionAsync(int userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task<List<UserRole>> GetUserRolesAsync(int userId);
    Task<bool> IsInRoleAsync(int userId, UserRole role);
    Task<bool> IsInAnyRoleAsync(int userId, params UserRole[] roles);
}

public static class Permissions
{
    // Work Request Permissions
    public const string CreateWorkRequest = "workrequest.create";
    public const string ReadWorkRequest = "workrequest.read";
    public const string UpdateWorkRequest = "workrequest.update";
    public const string DeleteWorkRequest = "workrequest.delete";
    public const string ApproveWorkRequest = "workrequest.approve";
    public const string RejectWorkRequest = "workrequest.reject";

    // User Management Permissions
    public const string CreateUser = "user.create";
    public const string ReadUser = "user.read";
    public const string UpdateUser = "user.update";
    public const string DeleteUser = "user.delete";
    public const string AssignRole = "user.assignrole";

    // System Administration Permissions
    public const string SystemConfiguration = "system.config";
    public const string ViewAuditLogs = "system.audit";
    public const string ManageIntegrations = "system.integrations";

    // Analytics and Reporting Permissions
    public const string ViewAnalytics = "analytics.view";
    public const string GenerateReports = "reports.generate";
    public const string ExportData = "data.export";

    // Priority Management Permissions
    public const string VotePriority = "priority.vote";
    public const string ManagePriority = "priority.manage";
    public const string OverridePriority = "priority.override";
} 