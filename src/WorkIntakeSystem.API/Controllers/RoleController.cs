using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.DTOs;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Authorization;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IUserRepository _userRepository;

    public RoleController(IRoleService roleService, IUserRepository userRepository)
    {
        _roleService = roleService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all roles and permissions information
    /// </summary>
    [HttpGet]
    [RequirePermission("user.read")]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var availableRoles = Enum.GetValues<UserRole>()
                .Select(r => new { Value = (int)r, Name = r.ToString() })
                .ToList();

            var allPermissions = await _roleService.GetAllPermissionsAsync();
            
            return Ok(new 
            { 
                AvailableRoles = availableRoles,
                AllPermissions = allPermissions
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving roles information");
        }
    }

    [HttpPost("assign")]
    [RequirePermission("user.assignrole")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _roleService.AssignRoleAsync(request.UserId, request.Role);
        if (!success)
            return BadRequest(new { message = "Failed to assign role" });

        return Ok(new { message = "Role assigned successfully" });
    }

    [HttpPost("remove")]
    [RequirePermission("user.assignrole")]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _roleService.RemoveRoleAsync(request.UserId, request.Role);
        if (!success)
            return BadRequest(new { message = "Failed to remove role" });

        return Ok(new { message = "Role removed successfully" });
    }

    [HttpGet("permissions/{userId}")]
    [RequirePermission("user.read")]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var permissions = await _roleService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
    }

    [HttpGet("roles/{userId}")]
    [RequirePermission("user.read")]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        var roles = await _roleService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    [HttpGet("check-permission")]
    public async Task<IActionResult> CheckPermission([FromQuery] string permission)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var hasPermission = await _roleService.HasPermissionAsync(userId, permission);
        return Ok(new { hasPermission });
    }

    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var permissions = await _roleService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
    }

    [HttpGet("available-roles")]
    public IActionResult GetAvailableRoles()
    {
        var roles = Enum.GetValues<UserRole>()
            .Select(r => new { Value = (int)r, Name = r.ToString() })
            .ToList();

        return Ok(roles);
    }
}

public class AssignRoleRequestDto
{
    public int UserId { get; set; }
    public UserRole Role { get; set; }
}

public class RemoveRoleRequestDto
{
    public int UserId { get; set; }
    public UserRole Role { get; set; }
} 