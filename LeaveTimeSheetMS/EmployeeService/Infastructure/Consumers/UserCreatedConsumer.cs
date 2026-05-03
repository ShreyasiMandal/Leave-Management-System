using EmployeeService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Shared.Events.Auth;

namespace EmployeeService.Infrastructure.Consumers;

public class UserCreatedConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<UserCreatedConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public UserCreatedConsumer(
        IServiceProvider services,
        ILogger<UserCreatedConsumer> logger,
        IConfiguration config)
    {
        _services = services;
        _logger = logger;

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

            _channel.ExchangeDeclare("ltma.events", ExchangeType.Topic, durable: true);

            _channel.QueueDeclare(
                queue: "employee.user_created",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: "employee.user_created",
                exchange: "ltma.events",
                routingKey: "UserCreated");

            _logger.LogInformation("UserCreatedConsumer connected to RabbitMQ.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "RabbitMQ unavailable. Consumer inactive. Error: {Msg}", ex.Message);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null) return Task.CompletedTask;

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<UserCreatedEvent>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (evt == null)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    return;
                }

                using var scope = _services.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IEmployeeService>();

                var exists = await svc.ExistsByUserIdAsync(evt.UserId);
                if (exists)
                {
                    _logger.LogInformation(
                        "Employee already exists for UserId {UserId}, skipping.", evt.UserId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                // Pass gender from event so Maternity Leave is assigned correctly
                await svc.CreateFromEventAsync(
                    evt.UserId, evt.FullName, evt.Email, evt.Gender);

                _channel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation(
                    "Employee profile created for UserId={UserId} Gender={Gender}",
                    evt.UserId, evt.Gender);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserCreated event.");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume("employee.user_created", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}