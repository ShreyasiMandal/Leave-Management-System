using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using MassTransit;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthService.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly JwtHelper _jwtHelper;
    private readonly AuthService.Application.Services.AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _publisherMock = new Mock<IMessagePublisher>();
        _emailMock = new Mock<IEmailService>();

        // In-memory configuration for JWT
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "LTMA_SUPER_SECRET_KEY_MIN_32_CHARS!!",
                ["Jwt:Issuer"] = "LTMA-AuthService",
                ["Jwt:Audience"] = "LTMA-Users",
                ["Jwt:AccessTokenExpiryHours"] = "8",
                ["Jwt:RefreshTokenExpiryDays"] = "7"
            })
            .Build();

        _jwtHelper = new JwtHelper(config);

        _sut = new AuthService.Application.Services.AuthService(
            _userRepoMock.Object,
            _jwtHelper,
            config,
            _publisherMock.Object,
            _emailMock.Object);
    }

    // ── REGISTER TESTS ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FullName = "John Doe",
            Email = "john@test.com",
            Password = "Test@123",
            Role = "Employee"
        };

        _userRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(true); // Email already taken

        // Act
        var act = async () => await _sut.RegisterAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsNew_CreatesUserSuccessfully()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FullName = "Jane Doe",
            Email = "jane@test.com",
            Password = "Test@123",
            Role = "Employee"
        };

        _userRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(false);

        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _publisherMock
            .Setup(p => p.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().Contain("successfully");

        // Verify user was added to repository
        _userRepoMock.Verify(
            r => r.AddAsync(It.Is<User>(u =>
                u.Email == dto.Email.ToLower() &&
                u.FullName == dto.FullName &&
                u.Role == dto.Role)),
            Times.Once);

        // Verify event was published to RabbitMQ
        _publisherMock.Verify(
            p => p.PublishAsync(
                "ltma.events",
                "UserCreated",
                It.IsAny<object>()),
            Times.Once);
    }

    // ── LOGIN TESTS ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "notfound@test.com",
            Password = "wrongpass"
        };

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null); // User not found

        // Act
        var act = async () => await _sut.LoginAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid credentials*");
    }



    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "Test@123";
        var dto = new LoginDto
        {
            Email = "valid@test.com",
            Password = password
        };

        var user = new User
        {
            Id = 1,
            Email = dto.Email,
            FullName = "Valid User",
            Role = "Employee",
            IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            LockoutEnd = null
        };

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Role.Should().Be("Employee");
        result.FullName.Should().Be("Valid User");
    }


}