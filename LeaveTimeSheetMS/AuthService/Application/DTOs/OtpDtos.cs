namespace AuthService.Application.DTOs;

public class SendOtpDto
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class FirstLoginChangeDto
{
    public string Email { get; set; } = string.Empty;
    public string TempPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}