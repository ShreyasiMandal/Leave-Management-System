using NotificationService.Application.DTOs.NotificationDTOs;
using NotificationService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events.Timesheet;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Consumers;

public class TimesheetEventConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TimesheetEventConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    private static readonly string[] TimesheetRoutingKeys =
    {
         "TimesheetCreated",
        "TimesheetSubmitted",
        "TimesheetApproved",
        "TimesheetRejected",
        "TimesheetOverdue"
    };

    public TimesheetEventConsumer(
        IServiceProvider services,
        ILogger<TimesheetEventConsumer> logger,
        IConfiguration config)
    {
        _services = services;
        _logger = logger;
        InitRabbitMq(config);
    }

    private void InitRabbitMq(IConfiguration config)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
                UserName = config["RabbitMQ:Username"] ?? "guest",
                Password = config["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Exchange
            _channel.ExchangeDeclare("ltma.events", ExchangeType.Topic, durable: true);

            // Queue
            _channel.QueueDeclare(
                queue: "notification.timesheet.events",
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind routing keys
            foreach (var key in TimesheetRoutingKeys)
            {
                _channel.QueueBind(
                    queue: "notification.timesheet.events",
                    exchange: "ltma.events",
                    routingKey: key);
            }

            _logger.LogInformation("✅ Timesheet consumer connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ RabbitMQ connection failed");
        }
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        if (_channel == null) return Task.CompletedTask;

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;

                using var scope = _services.CreateScope();
                var notifService = scope.ServiceProvider
                    .GetRequiredService<INotificationService>();

                int userId = 0;
                int? entryId = null;
                string subject = string.Empty;
                string body = string.Empty;

                switch (routingKey)
                {
                    case "TimesheetCreated":
                        {
                            var evt = JsonSerializer.Deserialize<TimesheetCreatedEvent>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                            if (evt == null) break;

                            userId = evt.UserId;
                            entryId = evt.EntryId;
                            Console.WriteLine($"Received TimesheetCreated for User {evt.UserId}");

                            var (s, b) = await notifService.ResolveTemplateAsync(routingKey,
                                new Dictionary<string, string>
                                {
                                    ["EmployeeName"] = $"Employee {evt.UserId}",
                                    ["Date"] = evt.Date.ToString("yyyy-MM-dd"),
                                    ["Hours"] = evt.Hours.ToString()
                                });

                            subject = s;
                            body = b;
                            break;
                        }
                    case "TimesheetSubmitted":
                        {
                            var evt = JsonSerializer.Deserialize<TimesheetSubmittedEvent>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (evt == null) break;

                            userId = evt.UserId;

                            var (s, b) = await notifService.ResolveTemplateAsync(routingKey,
                                new Dictionary<string, string>
                                {
                                    ["EmployeeName"] = $"Employee {evt.UserId}",
                                    ["WeekStart"] = evt.WeekStart.ToString("yyyy-MM-dd"),
                                    ["TotalHours"] = evt.TotalHours.ToString()
                                });

                            subject = s;
                            body = b;
                            break;
                        }

                    case "TimesheetApproved":
                        {
                            var evt = JsonSerializer.Deserialize<TimesheetApprovedEvent>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (evt == null) break;

                            userId = evt.UserId;
                            entryId = evt.EntryId;

                            var (s, b) = await notifService.ResolveTemplateAsync(routingKey,
                                new Dictionary<string, string>
                                {
                                    ["EmployeeName"] = $"Employee {evt.UserId}",
                                    ["WeekStart"] = evt.WeekStart.ToString("yyyy-MM-dd"),
                                    ["ProjectName"] = evt.ProjectName
                                });

                            subject = s;
                            body = b;
                            break;
                        }

                    case "TimesheetRejected":
                        {
                            var evt = JsonSerializer.Deserialize<TimesheetRejectedEvent>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (evt == null) break;

                            userId = evt.UserId;
                            entryId = evt.EntryId;

                            var (s, b) = await notifService.ResolveTemplateAsync(routingKey,
                                new Dictionary<string, string>
                                {
                                    ["EmployeeName"] = $"Employee {evt.UserId}",
                                    ["WeekStart"] = evt.WeekStart.ToString("yyyy-MM-dd"),
                                    ["Comment"] = evt.Comment
                                });

                            subject = s;
                            body = b;
                            break;
                        }

                    case "TimesheetOverdue":
                        {
                            var evt = JsonSerializer.Deserialize<TimesheetOverdueEvent>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (evt == null) break;

                            userId = evt.UserId;

                            var (s, b) = await notifService.ResolveTemplateAsync(routingKey,
                                new Dictionary<string, string>
                                {
                                    ["EmployeeName"] = $"Employee {evt.UserId}",
                                    ["WeekStart"] = evt.WeekStart.ToString("yyyy-MM-dd")
                                });

                            subject = s;
                            body = b;
                            break;
                        }
                }

                if (userId > 0)
                {
                    await notifService.CreateAsync(new CreateNotificationDto
                    {
                        UserId = userId,
                        Title = subject,
                        Message = body,
                        Type = routingKey,
                        EntityId = entryId,
                        EntityType = "Timesheet"
                    });
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing timesheet event");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: "notification.timesheet.events",
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}