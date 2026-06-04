namespace MarketInsight.Api.Options;

/// <summary>
/// Represents RabbitMQ connection and queue configuration.
/// </summary>
public class RabbitMqOptions
{
    /// <summary>
    /// RabbitMQ host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ AMQP port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Queue name used for asynchronous price refresh requests.
    /// </summary>
    public string QueueName { get; set; } = "price-refresh-queue";
}