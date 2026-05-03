using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    // =========================
    // 🔓 PUBLIC (No Auth Required)
    // =========================
    Task<string> RegisterAsync(RegisterDto dto);
    Task<TokenResponseDto> LoginAsync(LoginDto dto);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);

    // =========================
    // 🔐 AUTHENTICATED USER
    // =========================
    Task LogoutAsync(int userId);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);

    // =========================
    // 🧑‍💼 HR ADMIN
    // =========================
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task DeactivateUserAsync(int userId);
    Task ReactivateUserAsync(int userId);
    Task UpdateUserRoleAsync(int userId, string role);

    // =========================
    // 🛠 SYSTEM ADMIN
    // =========================
    Task UnlockAccountAsync(int userId);

    Task SendOtpAsync(string email);
    Task<string> VerifyOtpAndResetAsync(VerifyOtpDto dto);
    Task<TokenResponseDto> FirstLoginChangeAsync(FirstLoginChangeDto dto);
    Task<string> CreateUserByHrAsync(RegisterDto dto);
}