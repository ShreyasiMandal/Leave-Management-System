using RabbitMQ.Client;
using Shared.Events.Timesheet;
using System.Text;
using System.Text.Json;
using TimesheetService.Application.Interfaces;
using TimesheetService.Domain.Entities;

namespace TimesheetService.Infrastructure.Messaging;

public class TimesheetEventPublisher : ITimesheetEventPublisher
{
    private readonly IModel _channel;
    private readonly ILogger<TimesheetEventPublisher> _logger;
    private const string Exchange = "ltma.events";

    public TimesheetEventPublisher(IConnection connection,
        ILogger<TimesheetEventPublisher> logger)
    {
        _logger = logger;
        _channel = connection.CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
    }

    public Task PublishTimesheetSubmittedAsync(
        int userId, DateTime weekStart, decimal totalHours)
        => Publish("TimesheetSubmitted", new TimesheetSubmittedEvent
        {
            UserId = userId,
            WeekStart = weekStart,
            WeekEnd = weekStart.AddDays(4),
            TotalHours = totalHours,
            IsLate = DateTime.UtcNow.Date > weekStart.AddDays(4).Date,
            EventAt = DateTime.UtcNow
        });

    public Task PublishTimesheetApprovedAsync(TimesheetEntry entry)
        => Publish("TimesheetApproved", new TimesheetApprovedEvent
        {
            EntryId = entry.Id,
            UserId = entry.UserId,
            ApproverId = entry.ApproverId ?? 0,
            WeekStart = entry.WeekStart,
            Hours = entry.Hours,
            ProjectName = entry.Project?.Name ?? string.Empty,
            EventAt = DateTime.UtcNow
        });

    public Task PublishTimesheetRejectedAsync(TimesheetEntry entry)
        => Publish("TimesheetRejected", new TimesheetRejectedEvent
        {
            EntryId = entry.Id,
            UserId = entry.UserId,
            RejectedById = entry.ApproverId ?? 0,
            WeekStart = entry.WeekStart,
            Comment = entry.ApproverComment ?? string.Empty,
            EventAt = DateTime.UtcNow
        });

    public Task PublishTimesheetOverdueAsync(int userId, DateTime weekStart)
        => Publish("TimesheetOverdue", new TimesheetOverdueEvent
        {
            UserId = userId,
            WeekStart = weekStart,
            DaysOverdue = (int)(DateTime.UtcNow.Date - weekStart.AddDays(4).Date).TotalDays,
            EventAt = DateTime.UtcNow
        });

    public Task PublishTimesheetCreatedAsync(TimesheetEntry entry)
    => Publish("TimesheetCreated", new TimesheetCreatedEvent
    {
        EntryId = entry.Id,
        UserId = entry.UserId,
        Date = entry.Date,
        Hours = entry.Hours,
        ProjectId = entry.ProjectId,
        Description = entry.Description,
        Category = entry.Category,
        EventAt = DateTime.UtcNow
    });

    private Task Publish<T>(string routingKey, T payload)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            _channel.BasicPublish(Exchange, routingKey, props, body);
            _logger.LogInformation("Published: {Key}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish: {Key}", routingKey);
        }
        return Task.CompletedTask;
    }
}