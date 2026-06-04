using System.Text;
using System.Text.Json;
using MarketInsight.Api.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MarketInsight.Api.Messaging;

/// <summary>
/// Publishes asynchronous price refresh messages to RabbitMQ.
/// </summary>
public class RabbitMqPriceRefreshPublisher : IPriceRefreshPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPriceRefreshPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqPriceRefreshPublisher"/> class.
    /// </summary>
    /// <param name="options">RabbitMQ options.</param>
    /// <param name="logger">The logger instance.</param>
    public RabbitMqPriceRefreshPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqPriceRefreshPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task PublishAsync(PriceRefreshMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(message.Symbol))
        {
            throw new ArgumentException("Price refresh message symbol cannot be empty.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(message.CorrelationId))
        {
            throw new ArgumentException("Price refresh message correlation id cannot be empty.", nameof(message));
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.CorrelationId = message.CorrelationId;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: _options.QueueName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Async price refresh message published for symbol {Symbol} with correlation id {CorrelationId}",
            message.Symbol,
            message.CorrelationId);

        return Task.CompletedTask;
    }
}