using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Helpers;
using Shared.Events.Auth;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly JwtHelper _jwtHelper;
    private readonly IConfiguration _config;
    private readonly IMessagePublisher _publisher;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepo,
        JwtHelper jwtHelper,
        IConfiguration config,
        IMessagePublisher publisher,
        IEmailService emailService)
    {
        _userRepo = userRepo;
        _jwtHelper = jwtHelper;
        _config = config;
        _publisher = publisher;
        _emailService = emailService;
    }

    // ================= REGISTER =================
    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            IsActive = true
        };

        await _userRepo.AddAsync(user);

        await _publisher.PublishAsync("ltma.events", "UserCreated",
            new UserCreatedEvent
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Gender = dto.Gender,
                EventAt = DateTime.UtcNow
            });

        return "User registered successfully.";
    }

    // ================= LOGIN =================
    public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var accessToken = _jwtHelper.GenerateAccessToken(user);
        var refreshToken = _jwtHelper.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _userRepo.UpdateAsync(user);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            Role = user.Role,
            FullName = user.FullName
        };
    }

    // ================= REFRESH =================
    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepo.GetByRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        var newAccess = _jwtHelper.GenerateAccessToken(user);
        var newRefresh = _jwtHelper.GenerateRefreshToken();

        user.RefreshToken = newRefresh;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _userRepo.UpdateAsync(user);

        return new TokenResponseDto
        {
            AccessToken = newAccess,
            RefreshToken = newRefresh,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            Role = user.Role,
            FullName = user.FullName
        };
    }

    // ================= FORGOT PASSWORD =================
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email);
        if (user == null) return;

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(30);

        await _userRepo.UpdateAsync(user);

        Console.WriteLine($"Reset token: {user.PasswordResetToken}");
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userRepo.GetByResetTokenAsync(dto.Token)
            ?? throw new InvalidOperationException("Invalid token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;

        await _userRepo.UpdateAsync(user);
    }

    // ================= LOGOUT =================
    public async Task LogoutAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;

        user.RefreshToken = null;
        await _userRepo.UpdateAsync(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepo.UpdateAsync(user);
    }

    // ================= ADMIN =================
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var u = await _userRepo.GetByIdAsync(userId);
        if (u == null) return null;

        return new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive
        };
    }

    public async Task DeactivateUserAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        user.IsActive = false;
        await _userRepo.UpdateAsync(user);
    }

    public async Task ReactivateUserAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        user.IsActive = true;
        await _userRepo.UpdateAsync(user);
    }

    public async Task UpdateUserRoleAsync(int userId, string role)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        user.Role = role;
        await _userRepo.UpdateAsync(user);
    }

    public async Task UnlockAccountAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        user.LockoutEnd = null;
        user.FailedLoginAttempts = 0;
        await _userRepo.UpdateAsync(user);
    }

    // ================= NEW FEATURES =================

    public async Task<string> CreateUserByHrAsync(RegisterDto dto)
    {
        var tempPassword = GenerateTempPassword();

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            Role = dto.Role,
            MustChangePassword = true
        };

        await _userRepo.AddAsync(user);
        await _publisher.PublishAsync("ltma.events", "UserCreated",
       new UserCreatedEvent
       {
           UserId = user.Id,
           FullName = user.FullName,
           Email = user.Email,
           Role = user.Role,
           Gender = dto.Gender,
           EventAt = DateTime.UtcNow
       });

        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, tempPassword);

        return "User created & email sent.";
    }

    public async Task SendOtpAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email);
        if (user == null) return;

        var otp = new Random().Next(100000, 999999).ToString();

        user.OtpCode = BCrypt.Net.BCrypt.HashPassword(otp);
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

        await _userRepo.UpdateAsync(user);

        await _emailService.SendOtpEmailAsync(user.Email, user.FullName, otp);
    }

    public async Task<string> VerifyOtpAndResetAsync(VerifyOtpDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);

        if (!BCrypt.Net.BCrypt.Verify(dto.Otp, user.OtpCode))
            throw new Exception("Invalid OTP");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepo.UpdateAsync(user);

        await _emailService.SendPasswordChangedEmailAsync(user.Email, user.FullName);

        return "Password reset successful";
    }

    public async Task<TokenResponseDto> FirstLoginChangeAsync(FirstLoginChangeDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.MustChangePassword = false;

        await _userRepo.UpdateAsync(user);

        return new TokenResponseDto
        {
            AccessToken = _jwtHelper.GenerateAccessToken(user),
            RefreshToken = _jwtHelper.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };
    }

    private static string GenerateTempPassword() => "Temp@1234";
}