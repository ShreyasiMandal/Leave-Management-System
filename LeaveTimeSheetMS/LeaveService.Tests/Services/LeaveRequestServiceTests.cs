using FluentAssertions;
using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.LeaveRequestDtos;
using LeaveService.Application.Interfaces;
using LeaveService.Application.Services;
using LeaveService.Domain.Entities;
using LeaveService.Domain.Enums;
using Moq;

namespace LeaveService.Tests.Services;

public class LeaveRequestServiceTests
{
    private readonly Mock<ILeaveRequestRepository> _leaveRepoMock;
    private readonly Mock<ILeaveBalanceRepository> _balanceRepoMock;
    private readonly Mock<ILeaveTypeRepository> _typeRepoMock;
    private readonly Mock<IHolidayRepository> _holidayRepoMock;
    private readonly Mock<ILeaveEventPublisher> _publisherMock;
    private readonly LeaveRequestService _sut;

    public LeaveRequestServiceTests()
    {
        _leaveRepoMock = new Mock<ILeaveRequestRepository>();
        _balanceRepoMock = new Mock<ILeaveBalanceRepository>();
        _typeRepoMock = new Mock<ILeaveTypeRepository>();
        _holidayRepoMock = new Mock<IHolidayRepository>();
        _publisherMock = new Mock<ILeaveEventPublisher>();

        _sut = new LeaveRequestService(
            _leaveRepoMock.Object,
            _balanceRepoMock.Object,
            _typeRepoMock.Object,
            _holidayRepoMock.Object,
            _publisherMock.Object);
    }

    // ── APPLY LEAVE TESTS ─────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyAsync_WhenFutureEndDateBeforeStartDate_ThrowsException()
    {
        // Arrange
        var dto = new CreateLeaveRequestDto
        {
            LeaveTypeId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(2), // End before start
            HalfDaySession = "Full",
            Reason = "Test"
        };

        _typeRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new LeaveType
            {
                Id = 1,
                Name = "Annual Leave",
                IsActive = true
            });

        // Act
        var act = async () => await _sut.ApplyAsync(1, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*End date cannot be before start date*");
    }

    [Fact]
    public async Task ApplyAsync_WhenLeaveTypeNotActive_ThrowsException()
    {
        // Arrange
        var dto = new CreateLeaveRequestDto
        {
            LeaveTypeId = 99,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Reason = "Test"
        };

        _typeRepoMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((LeaveType?)null); // Not found

        // Act
        var act = async () => await _sut.ApplyAsync(1, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ApplyAsync_WhenDatesOverlap_ThrowsException()
    {
        // Arrange
        var dto = new CreateLeaveRequestDto
        {
            LeaveTypeId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Reason = "Test"
        };

        _typeRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new LeaveType
            {
                Id = 1,
                Name = "Annual Leave",
                IsActive = true
            });

        _leaveRepoMock
            .Setup(r => r.HasOverlapAsync(
                1,
                dto.StartDate,
                dto.EndDate,
                null))
            .ReturnsAsync(true); // Overlap exists

        // Act
        var act = async () => await _sut.ApplyAsync(1, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already have a leave*");
    }

    [Fact]
    public async Task ApplyAsync_WhenValidRequest_CreatesLeaveAndPublishesEvent()
    {
        int userId = 1;
        var dto = new CreateLeaveRequestDto
        {
            LeaveTypeId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            HalfDaySession = "Full",
            Reason = "Annual vacation"
        };

        _typeRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new LeaveType
            {
                Id = 1,
                Name = "Annual Leave",
                IsActive = true,
                IsAutoApprove = false
                
            });

        _leaveRepoMock
            .Setup(r => r.HasOverlapAsync(
                userId, dto.StartDate, dto.EndDate, null))
            .ReturnsAsync(false);

        _leaveRepoMock
            .Setup(r => r.AddAsync(It.IsAny<LeaveRequest>()))
            .Returns(Task.CompletedTask);

        _balanceRepoMock
            .Setup(r => r.GetAsync(userId, 1, DateTime.UtcNow.Year))
            .ReturnsAsync(new LeaveBalance
            {
                UserId = userId,
                LeaveTypeId = 1,
                Year = DateTime.UtcNow.Year,
                Entitled = 21,
                Used = 0,
                Pending = 0
            });

        _balanceRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<LeaveBalance>()))
            .Returns(Task.CompletedTask);

        // ✅ FIX: Verify SAGA publisher instead of LeaveCreated
        // Because after SAGA implementation, service calls PublishStartSagaAsync
        _publisherMock
            .Setup(p => p.PublishLeaveCreatedAsync(It.IsAny<LeaveRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ApplyAsync(userId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Leave.Status.Should().Be(LeaveStatus.Pending);
        result.Leave.Days.Should().BeGreaterThan(0);

        // ✅ Verify leave was saved
        _leaveRepoMock.Verify(
            r => r.AddAsync(It.Is<LeaveRequest>(l =>
                l.UserId == userId &&
                l.LeaveTypeId == dto.LeaveTypeId &&
                l.Status == LeaveStatus.Pending)),
            Times.Once);

        // ✅ Verify balance was updated
        _balanceRepoMock.Verify(
            r => r.UpdateAsync(It.IsAny<LeaveBalance>()),
            Times.Once);
    }

    // ── APPROVE TESTS ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_WhenLeaveNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _leaveRepoMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((LeaveRequest?)null);

        // Act
        var act = async () => await _sut.ApproveAsync(999, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ApproveAsync_WhenLeaveAlreadyApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var leave = new LeaveRequest
        {
            Id = 1,
            Status = LeaveStatus.Approved // Already approved
        };

        _leaveRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(leave);

        // Act
        var act = async () => await _sut.ApproveAsync(1, 1);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot approve*");
    }

    // ── CANCEL TESTS ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_WhenNotOwner_ThrowsUnauthorizedException()
    {
        // Arrange
        var leave = new LeaveRequest
        {
            Id = 1,
            UserId = 5,   // Belongs to user 5
            Status = LeaveStatus.Pending
        };

        _leaveRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(leave);

        // Act — user 1 tries to cancel user 5's leave
        var act = async () => await _sut.CancelAsync(1, userId: 1);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}