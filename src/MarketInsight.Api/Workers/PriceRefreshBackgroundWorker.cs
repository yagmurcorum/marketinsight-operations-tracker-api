using System.Text;
using System.Text.Json;
using MarketInsight.Api.Messaging;
using MarketInsight.Api.Options;
using MarketInsight.Api.Services.Quotes;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MarketInsight.Api.Workers;

/// <summary>
/// Consumes queued price refresh messages from RabbitMQ and triggers the quote refresh flow.
/// </summary>
public class PriceRefreshBackgroundWorker : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PriceRefreshBackgroundWorker> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceRefreshBackgroundWorker"/> class.
    /// </summary>
    /// <param name="options">RabbitMQ options.</param>
    /// <param name="serviceScopeFactory">Service scope factory used to resolve scoped services.</param>
    /// <param name="logger">The logger instance.</param>
    public PriceRefreshBackgroundWorker(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PriceRefreshBackgroundWorker> logger)
    {
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitializeRabbitMq();

        if (_channel is null)
        {
            _logger.LogError("RabbitMQ channel could not be initialized for price refresh worker.");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, eventArgs) =>
        {
            await ProcessMessageAsync(eventArgs, stoppingToken);
        };

        _channel.BasicConsume(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Price refresh background worker started consuming queue {QueueName}",
            _options.QueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Price refresh background worker is stopping.");
        }
    }

    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            _logger.LogError("RabbitMQ channel is not available while processing price refresh message.");
            return;
        }

        PriceRefreshMessage? message = null;

        try
        {
            var body = eventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            message = JsonSerializer.Deserialize<PriceRefreshMessage>(json);

            if (message is null || string.IsNullOrWhiteSpace(message.Symbol))
            {
                _logger.LogWarning("Invalid price refresh message received from RabbitMQ.");

                _channel.BasicAck(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);

                return;
            }

            using var logScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = message.CorrelationId,
                ["Symbol"] = message.Symbol
            });

            _logger.LogInformation(
                "Async price refresh message consumed for symbol {Symbol} with correlation id {CorrelationId}",
                message.Symbol,
                message.CorrelationId);

            using var scope = _serviceScopeFactory.CreateScope();

            var quoteRefreshService = scope.ServiceProvider.GetRequiredService<IQuoteRefreshService>();

            var result = await quoteRefreshService.RefreshQuoteAsync(message.Symbol, cancellationToken);

            if (result.IsNotFound)
            {
                _logger.LogWarning(
                    "Queued price refresh skipped because active watchlist item was not found for symbol {Symbol}",
                    message.Symbol);
            }
            else if (result.IsExternalFailure)
            {
                _logger.LogError(
                    "Queued price refresh failed for symbol {Symbol}. Reason: {Message}",
                    message.Symbol,
                    result.Message);
            }
            else
            {
                _logger.LogInformation(
                    "Queued price refresh completed for symbol {Symbol}",
                    message.Symbol);
            }

            _channel.BasicAck(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error occurred while processing queued price refresh message for symbol {Symbol}",
                message?.Symbol ?? "Unknown");

            _channel.BasicNack(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: false);
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();

        _connection?.Close();
        _connection?.Dispose();

        base.Dispose();
    }
}