namespace MarketInsight.Api.Messaging;

/// <summary>
/// Defines message publishing operations for asynchronous price refresh requests.
/// </summary>
public interface IPriceRefreshPublisher
{
    /// <summary>
    /// Publishes a price refresh message to the configured queue.
    /// </summary>
    /// <param name="message">The price refresh message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync(PriceRefreshMessage message, CancellationToken cancellationToken = default);
}