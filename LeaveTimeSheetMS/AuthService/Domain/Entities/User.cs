namespace AuthService.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // ── OLD token reset (keep for backwards compat) ──
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }

    // ── NEW OTP fields ───────────────────────────────
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }
    public int OtpAttempts { get; set; } = 0;

    // ── First login flag ─────────────────────────────
    // true = HR created this account with temp password
    public bool MustChangePassword { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}