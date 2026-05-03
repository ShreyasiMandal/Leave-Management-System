using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ============================================================
    // PUBLIC ENDPOINTS (NO TOKEN REQUIRED)
    // ============================================================

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        return Ok(token);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var token = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(token);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(new { message = "If email exists, reset link sent." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(new { message = "Password reset successful." });
    }

    // ⭐ Bootstrap Admin
    [AllowAnonymous]
    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateSystemAdmin([FromBody] RegisterDto dto)
    {
        dto.Role = UserRoles.SystemAdmin;
        var result = await _authService.RegisterAsync(dto);
        return StatusCode(201, new { message = result });
    }

    // ================= NEW FEATURES =================

    // 🔐 SEND OTP
    [AllowAnonymous]
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        await _authService.SendOtpAsync(dto.Email);
        return Ok(new
        {
            message = "If this email is registered, OTP sent (valid 10 min)."
        });
    }

    // 🔐 VERIFY OTP
    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var result = await _authService.VerifyOtpAndResetAsync(dto);
        return Ok(new { message = result });
    }

    // 🔐 FIRST LOGIN PASSWORD CHANGE
    [AllowAnonymous]
    [HttpPost("first-login")]
    public async Task<IActionResult> FirstLogin([FromBody] FirstLoginChangeDto dto)
    {
        var token = await _authService.FirstLoginChangeAsync(dto);
        return Ok(token);
    }

    // ============================================================
    // AUTHENTICATED USER
    // ============================================================

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid token claims." });
        await _authService.LogoutAsync(userId);
        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        return Ok(new
        {
            UserId = User.FindFirst("userId")?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Name = User.FindFirst(ClaimTypes.Name)?.Value,
            Role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(new { message = "Password changed successfully." });
    }

    // ============================================================
    // HR ADMIN
    // ============================================================

    // 🔥 UPDATED CREATE USER (IMPORTANT CHANGE)
    [Authorize(Policy = "HROrAbove")]
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] RegisterDto dto)
    {
        var allowedRoles = new[]
        {
            UserRoles.Employee,
            UserRoles.Manager,
            UserRoles.HRAdmin
        };

        if (!allowedRoles.Contains(dto.Role))
            return BadRequest(new
            {
                message = $"Invalid role. Allowed: {string.Join(", ", allowedRoles)}"
            });

        var result = await _authService.CreateUserByHrAsync(dto);
        return StatusCode(201, new { message = result });
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("deactivate/{userId}")]
    public async Task<IActionResult> DeactivateUser(int userId)
    {
        await _authService.DeactivateUserAsync(userId);
        return Ok(new { message = "User deactivated" });
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("reactivate/{userId}")]
    public async Task<IActionResult> ReactivateUser(int userId)
    {
        await _authService.ReactivateUserAsync(userId);
        return Ok(new { message = "User reactivated" });
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("update-role/{userId}")]
    public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateRoleDto dto)
    {
        await _authService.UpdateUserRoleAsync(userId, dto.Role);
        return Ok(new { message = "Role updated" });
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(user);
    }

    // ============================================================
    // SYSTEM ADMIN
    // ============================================================

    [Authorize(Policy = "SystemAdminOnly")]
    [HttpPut("unlock/{userId}")]
    public async Task<IActionResult> UnlockAccount(int userId)
    {
        await _authService.UnlockAccountAsync(userId);
        return Ok(new { message = "Account unlocked" });
    }
}