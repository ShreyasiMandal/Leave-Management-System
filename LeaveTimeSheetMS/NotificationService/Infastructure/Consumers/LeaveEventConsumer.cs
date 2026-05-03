using NotificationService.Application.DTOs.NotificationDTOs;
using NotificationService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events.Leave;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Consumers;

public class LeaveEventConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LeaveEventConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    // ✅ FIX 1: Routing keys WITHOUT "Event" suffix
    // Must match exactly what LeaveEventPublisher.cs sends
    private static readonly string[] LeaveRoutingKeys =
    {
        "LeaveCreated",
        "LeaveApproved",
        "LeaveRejected",
        "LeaveCancelled",
        "LeaveEscalatedToHR"
    };

    public LeaveEventConsumer(
        IServiceProvider services,
        ILogger<LeaveEventConsumer> logger,
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

            // ✅ FIX 2: Same exchange name and type as LeaveService publisher
            _channel.ExchangeDeclare(
                exchange: "ltma.events",
                type: ExchangeType.Topic,
                durable: true);

            // Declare queue
            _channel.QueueDeclare(
                queue: "notification.leave.events",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // ✅ FIX 3: Bind ALL routing keys to the queue
            foreach (var key in LeaveRoutingKeys)
            {
                _channel.QueueBind(
                    queue: "notification.leave.events",
                    exchange: "ltma.events",
                    routingKey: key);

                _logger.LogInformation(
                    "✅ Bound routing key: {Key}", key);
            }

            _logger.LogInformation("✅ LeaveEventConsumer connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "❌ RabbitMQ connection failed: {Msg}", ex.Message);
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

                _logger.LogInformation(
                    "📨 Leave event received: {Key}", routingKey);

                using var scope = _services.CreateScope();
                var notifService = scope.ServiceProvider
                    .GetRequiredService<INotificationService>();

                await HandleLeaveEventAsync("LeaveCreated", json, notifService);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing leave event");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: "notification.leave.events",
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task HandleLeaveEventAsync(
        string routingKey,
        string json,
        INotificationService notifService)
    {
        int userId = 0;
        int? leaveId = null;
        string subject = string.Empty;
        string body = string.Empty;

        // ✅ FIX 4: Switch cases match routing keys WITHOUT "Event" suffix
        switch (routingKey)
        {
            case "LeaveCreated":
                {
                    var evt = JsonSerializer.Deserialize<LeaveCreatedEvent>(json,
                        new JsonSerializerOptions
                        { PropertyNameCaseInsensitive = true });

                    if (evt == null) return;

                    userId = evt.UserId;
                    leaveId = evt.LeaveId;

                    var (s, b) = await notifService.ResolveTemplateAsync(
                        routingKey,
                        new Dictionary<string, string>
                        {
                            ["EmployeeName"] = $"Employee {evt.UserId}",
                            ["LeaveType"] = evt.LeaveTypeName,
                            ["StartDate"] = evt.StartDate.ToString("yyyy-MM-dd"),
                            ["EndDate"] = evt.EndDate.ToString("yyyy-MM-dd"),
                            ["Days"] = evt.Days.ToString()
                        });

                    subject = s; body = b;
                    break;
                }

            case "LeaveApproved":
                {
                    var evt = JsonSerializer.Deserialize<LeaveApprovedEvent>(json,
                        new JsonSerializerOptions
                        { PropertyNameCaseInsensitive = true });

                    if (evt == null) return;

                    userId = evt.UserId;
                    leaveId = evt.LeaveId;

                    var (s, b) = await notifService.ResolveTemplateAsync(
                        routingKey,
                        new Dictionary<string, string>
                        {
                            ["EmployeeName"] = $"Employee {evt.UserId}",
                            ["LeaveType"] = evt.LeaveTypeName,
                            ["StartDate"] = evt.StartDate.ToString("yyyy-MM-dd"),
                            ["EndDate"] = evt.EndDate.ToString("yyyy-MM-dd"),
                            ["Days"] = evt.Days.ToString(),
                            ["ApprovedBy"] = evt.ApprovedBy
                        });

                    subject = s; body = b;
                    break;
                }

            case "LeaveRejected":
                {
                    var evt = JsonSerializer.Deserialize<LeaveRejectedEvent>(json,
                        new JsonSerializerOptions
                        { PropertyNameCaseInsensitive = true });

                    if (evt == null) return;

                    userId = evt.UserId;
                    leaveId = evt.LeaveId;

                    var (s, b) = await notifService.ResolveTemplateAsync(
                        routingKey,
                        new Dictionary<string, string>
                        {
                            ["EmployeeName"] = $"Employee {evt.UserId}",
                            ["LeaveType"] = evt.LeaveTypeName,
                            ["StartDate"] = evt.StartDate.ToString("yyyy-MM-dd"),
                            ["EndDate"] = evt.EndDate.ToString("yyyy-MM-dd"),
                            ["Comment"] = evt.Comment
                        });

                    subject = s; body = b;
                    break;
                }

            case "LeaveCancelled":
                {
                    var evt = JsonSerializer.Deserialize<LeaveCancelledEvent>(json,
                        new JsonSerializerOptions
                        { PropertyNameCaseInsensitive = true });

                    if (evt == null) return;

                    userId = evt.UserId;
                    leaveId = evt.LeaveId;
                    subject = "Leave Request Cancelled";
                    body = $"Your {evt.LeaveTypeName} leave from " +
                              $"{evt.StartDate:yyyy-MM-dd} to " +
                              $"{evt.EndDate:yyyy-MM-dd} has been cancelled.";
                    break;
                }

            case "LeaveEscalatedToHR":
                {
                    var evt = JsonSerializer.Deserialize<LeaveEscalatedEvent>(json,
                        new JsonSerializerOptions
                        { PropertyNameCaseInsensitive = true });

                    if (evt == null) return;

                    userId = evt.UserId;
                    leaveId = evt.LeaveId;
                    subject = "Leave Request Escalated to HR";
                    body = $"Your {evt.LeaveTypeName} leave ({evt.Days} days) " +
                              "is now under HR review.";
                    break;
                }

            default:
                _logger.LogWarning(
                    "⚠ Unknown routing key: '{Key}'", routingKey);
                return;
        }

        if (userId > 0)
        {
            await notifService.CreateAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = subject,
                Message = body,
                Type = routingKey,
                EntityId = leaveId,
                EntityType = "Leave"
            });

            _logger.LogInformation(
                "✅ Notification created → UserId: {UserId}, Type: {Type}",
                userId, routingKey);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}