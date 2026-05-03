using AuthService.Application.Interfaces;  // ← references the interface
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher  // ← implements interface
{
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConnection connection,
        ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _channel = connection.CreateModel();
        _channel.ExchangeDeclare("ltma.events", ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            _channel.BasicPublish(exchange, routingKey, props, body);
            _logger.LogInformation("Published: {Key}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish: {Key}", routingKey);
        }
        return Task.CompletedTask;
    }
}