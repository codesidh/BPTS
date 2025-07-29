using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.DTOs;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.API.DTOs;
using AutoMapper;
using WorkIntakeSystem.Infrastructure.Services;

namespace WorkIntakeSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtAuthenticationService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AuthController(
        IJwtAuthenticationService authService,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _authService = authService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.ValidateUserAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = await _authService.GenerateJwtTokenAsync(user);
        var userDto = _mapper.Map<UserDto>(user);

        var response = new AuthenticationResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // Should match JWT expiration
            User = userDto
        };

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _authService.IsEmailUniqueAsync(request.Email))
            return BadRequest(new { message = "Email already exists" });

        var (passwordHash, passwordSalt) = JwtAuthenticationService.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            DepartmentId = request.DepartmentId,
            BusinessVerticalId = request.BusinessVerticalId,
            Role = UserRole.EndUser, // Default role for new users
            Capacity = 40,
            CurrentWorkload = 0.0m,
            SkillSet = "{}"
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var token = await _authService.GenerateJwtTokenAsync(createdUser);
        var userDto = _mapper.Map<UserDto>(createdUser);

        var response = new AuthenticationResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = userDto
        };

        return CreatedAtAction(nameof(GetMe), response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!success)
            return BadRequest(new { message = "Current password is incorrect" });

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _authService.ResetPasswordAsync(request.Email);
        if (!success)
            return BadRequest(new { message = "Email not found" });

        return Ok(new { message = "Password reset email sent" });
    }

    [HttpPost("confirm-reset-password")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _authService.ConfirmPasswordResetAsync(request.Token, request.NewPassword);
        if (!success)
            return BadRequest(new { message = "Invalid or expired reset token" });

        return Ok(new { message = "Password reset successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDto request)
    {
        var isValid = await _authService.ValidateJwtTokenAsync(request.Token);
        if (!isValid)
            return Unauthorized(new { message = "Invalid token" });

        var user = await _authService.GetUserFromTokenAsync(request.Token);
        if (user == null)
            return Unauthorized(new { message = "User not found" });

        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }
} 