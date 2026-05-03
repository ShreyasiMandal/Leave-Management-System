using TimesheetService.Domain.Entities;

namespace TimesheetService.Application.Interfaces;

public interface ITimesheetEventPublisher
{
    Task PublishTimesheetSubmittedAsync(int userId, DateTime weekStart,
        decimal totalHours);
    Task PublishTimesheetApprovedAsync(TimesheetEntry entry);
    Task PublishTimesheetRejectedAsync(TimesheetEntry entry);
    Task PublishTimesheetOverdueAsync(int userId, DateTime weekStart);

    Task PublishTimesheetCreatedAsync(TimesheetEntry entry);
}