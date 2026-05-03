using FluentAssertions;
using Moq;
using System.Timers;
using TimesheetService.Application.DTOs.TimesheetDTOs;
using TimesheetService.Application.Interfaces;
using TimesheetService.Domain.Entities;
using TimesheetService.Domain.Enums;

namespace TimesheetService.Tests.Services;

public class TimesheetServiceTests
{
    private readonly Mock<ITimesheetRepository> _repoMock;
    private readonly Mock<IProjectRepository> _projectRepoMock;
    private readonly Mock<ITimesheetEventPublisher> _publisherMock;
    private readonly TimesheetService.Application.Services.TimesheetService _sut;

    public TimesheetServiceTests()
    {
        _repoMock = new Mock<ITimesheetRepository>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _publisherMock = new Mock<ITimesheetEventPublisher>();

        _sut = new TimesheetService.Application.Services.TimesheetService(
            _repoMock.Object,
            _projectRepoMock.Object,
            _publisherMock.Object);
    }

    // ── LOG ENTRY TESTS ───────────────────────────────────────────────────────

    [Fact]
    public async Task LogEntryAsync_WhenFutureDate_ThrowsException()
    {
        // Arrange
        var dto = new CreateTimesheetEntryDto
        {
            Date = DateTime.Today.AddDays(1), // Future date
            ProjectId = 1,
            Hours = 8,
            Category = "Regular"
        };

        // Act
        var act = async () => await _sut.LogEntryAsync(1, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*future date*");
    }

    [Fact]
    public async Task LogEntryAsync_WhenProjectNotFound_ThrowsException()
    {
        // Arrange
        var dto = new CreateTimesheetEntryDto
        {
            Date = DateTime.Today,
            ProjectId = 999,
            Hours = 8,
            Category = "Regular"
        };

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Project?)null);

        // Act
        var act = async () => await _sut.LogEntryAsync(1, dto);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task LogEntryAsync_WhenValid_ReturnsWarningIfHoursExceed12()
    {
        // Arrange
        int userId = 1;
        var dto = new CreateTimesheetEntryDto
        {
            Date = DateTime.Today,
            ProjectId = 1,
            Hours = 13, // Exceeds 12
            Description = "Long day",
            Category = "Overtime"
        };

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Project
            {
                Id = 1,
                Name = "General",
                IsActive = true
            });

        _repoMock
            .Setup(r => r.WeekAlreadySubmittedAsync(userId, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<TimesheetEntry>()))
            .Returns(Task.CompletedTask);

        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new TimesheetEntry
            {
                Id = 1,
                UserId = userId,
                Date = dto.Date,
                Hours = dto.Hours,
                Project = new Project { Name = "General", Code = "GEN" }
            });

        _repoMock
            .Setup(r => r.GetDailyTotalHoursAsync(userId, dto.Date))
            .ReturnsAsync(13m);

        // Act
        var result = await _sut.LogEntryAsync(userId, dto);

        // Assert
        result.Should().NotBeNull();
        result.ExceedsMaxHours.Should().BeTrue();
        result.Warning.Should().Contain("12-hour");
    }

    // ── SUBMIT WEEK TESTS ─────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitWeekAsync_WhenNoEntries_ThrowsException()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByWeekAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TimesheetEntry>());

        // Act
        var act = async () =>
            await _sut.SubmitWeekAsync(1, DateTime.Today);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*No timesheet entries*");
    }

    [Fact]
    public async Task SubmitWeekAsync_WhenDraftEntriesExist_TransitionsToSubmitted()
    {
        // Arrange
        int userId = 1;
        var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);

        var entries = new List<TimesheetEntry>
        {
            new() { Id=1, UserId=userId, Status=TimesheetStatus.Draft,
                    Hours=8, WeekStart=weekStart,
                    Project = new Project { Name="General" } },
            new() { Id=2, UserId=userId, Status=TimesheetStatus.Draft,
                    Hours=7, WeekStart=weekStart,
                    Project = new Project { Name="General" } }
        };

        _repoMock
            .Setup(r => r.GetByWeekAsync(userId, It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        _repoMock
            .Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<TimesheetEntry>>()))
            .Returns(Task.CompletedTask);

        _publisherMock
            .Setup(p => p.PublishTimesheetSubmittedAsync(
                userId, It.IsAny<DateTime>(), It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SubmitWeekAsync(userId, weekStart);

        // Assert
        result.Should().NotBeNull();
        result.WeekStatus.Should().Be(TimesheetStatus.Submitted);

        // Verify event published
        _publisherMock.Verify(
            p => p.PublishTimesheetSubmittedAsync(
                userId,
                It.IsAny<DateTime>(),
                15m), // 8 + 7
            Times.Once);
    }

    // ── APPROVE TESTS ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_WhenEntryNotSubmitted_ThrowsException()
    {
        // Arrange
        var entry = new TimesheetEntry
        {
            Id = 1,
            Status = TimesheetStatus.Draft // Not submitted
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        // Act
        var act = async () => await _sut.ApproveAsync(1, 2);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot approve*");
    }

    [Fact]
    public async Task RejectAsync_WhenCommentIsEmpty_ThrowsException()
    {
        // Act
        var act = async () => await _sut.RejectAsync(1, 2, ""); // Empty comment

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*mandatory*");
    }
}