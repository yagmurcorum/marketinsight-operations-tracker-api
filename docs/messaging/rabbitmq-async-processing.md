# RabbitMQ Async Processing

This document explains how RabbitMQ is used for asynchronous price refresh requests in the MarketInsight Operations Tracker API.

The purpose of this document is to define how the API queues a price refresh request without executing the full quote refresh flow inside the HTTP request.

---

## Purpose

The project uses RabbitMQ to support asynchronous price refresh processing.

The async refresh endpoint accepts a request for an active watchlist symbol and publishes a message to RabbitMQ.

This keeps the HTTP request lightweight.

The endpoint does not execute the full quote refresh operation directly.

The actual refresh processing will be handled by a Background Worker in the next step.

---

## Why RabbitMQ Is Used

RabbitMQ is used to separate request acceptance from background processing.

Without RabbitMQ, the API would need to complete the full refresh flow during the HTTP request.

That means the endpoint would directly wait for:

- External finance API communication
- Redis cache operations
- PriceSnapshot persistence
- Error handling around external dependencies

For the async flow, the API only accepts the request and places a message into a queue.

This creates a cleaner separation between:

| Responsibility | Owner |
|---|---|
| Accept async refresh request | API endpoint |
| Validate active WatchlistItem | API / Service layer |
| Publish message | RabbitMQ publisher |
| Store pending work | RabbitMQ queue |
| Execute refresh logic | Background Worker |
| Reuse business flow | QuoteRefreshService |

---

## Layer Responsibility Reminder

The project keeps responsibilities separated.

| Layer | Main Responsibility |
|---|---|
| Controller | Handles HTTP request and response flow |
| Service | Handles use cases and business rules |
| Publisher | Publishes messages to RabbitMQ |
| RabbitMQ | Stores async work messages |
| Background Worker | Consumes messages and triggers processing |
| QuoteRefreshService | Executes quote refresh business flow |

The async endpoint should not become responsible for the full price refresh use case.

Its responsibility is limited to request validation and message publishing.

---

## Async Refresh Endpoint

The async refresh endpoint is:

    POST /api/watchlist-items/{symbol}/refresh-async

Example:

    POST /api/watchlist-items/TSLA/refresh-async

Expected successful response:

    202 Accepted

Response body:

    {
      "message": "Price refresh request accepted.",
      "symbol": "TSLA",
      "status": "Queued"
    }

This response means the request was accepted and queued.

It does not mean the price was already refreshed.

---

## Queue Standard

The RabbitMQ queue used by this feature is:

    price-refresh-queue

The queue is declared by the publisher when a valid async refresh request is published.

RabbitMQ does not need a manually created queue for this MVP flow.

If the endpoint is never called successfully, the queue may not appear in the RabbitMQ management panel yet.

---

## Message Contract

The async refresh flow uses the following message model:

    PriceRefreshMessage

The message contains:

| Field | Purpose |
|---|---|
| Symbol | Normalized symbol requested for refresh |
| RequestedAtUtc | UTC time when the async request was created |
| CorrelationId | Identifier used to track the async flow through logs |

Current message shape:

    public class PriceRefreshMessage
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }

For the MVP, these fields are enough.

Additional fields can be introduced later if the async flow needs more context.

---

## Producer Flow

The async producer flow is:

    HTTP Request
        ↓
    WatchlistItemsController
        ↓
    Active WatchlistItem check
        ↓
    PriceRefreshMessage creation
        ↓
    IPriceRefreshPublisher
        ↓
    RabbitMqPriceRefreshPublisher
        ↓
    RabbitMQ
        ↓
    price-refresh-queue

The Controller receives the HTTP request.

The symbol is normalized.

The endpoint checks whether the requested symbol exists as an active WatchlistItem.

If the symbol is missing or inactive, the endpoint returns 404 Not Found.

If the symbol is active, the endpoint publishes a PriceRefreshMessage to RabbitMQ and returns 202 Accepted.

---

## Active WatchlistItem Check

The async endpoint only publishes messages for active watchlist items.

This prevents unnecessary queue messages for symbols that the application should not process.

Expected behavior:

| Symbol state | Endpoint result | Message published |
|---|---|---|
| Active WatchlistItem exists | 202 Accepted | Yes |
| Symbol does not exist | 404 Not Found | No |
| Symbol exists but inactive | 404 Not Found | No |

This keeps the queue clean and avoids invalid background work.

---

## What This Endpoint Does Not Do

The async endpoint does not execute the full quote refresh flow.

It does not:

- Call the external finance API
- Fetch current market price directly
- Read quote data from Redis
- Write quote data to Redis
- Create a PriceSnapshot
- Save new price history to SQLite
- Evaluate price alerts
- Consume RabbitMQ messages

Those responsibilities belong to the Background Worker and QuoteRefreshService flow.

The async endpoint only queues work.

---

## Relationship With QuoteRefreshService

The existing synchronous refresh endpoint uses QuoteRefreshService directly:

    POST /api/watchlist-items/{symbol}/refresh

That endpoint performs the full refresh flow immediately.

The async endpoint is different:

    POST /api/watchlist-items/{symbol}/refresh-async

It only publishes a message.

In the next step, the Background Worker will consume the message and call QuoteRefreshService.

This keeps quote refresh business logic in one place.

The worker should reuse QuoteRefreshService instead of duplicating refresh logic.

---

## Relationship With Background Worker

The Background Worker is not part of this implementation yet.

The current implementation prepares the producer side of the async flow.

Current state:

    API publishes message
    RabbitMQ stores message
    Worker does not consume it yet

This means messages can remain in the queue as Ready messages.

That is expected for this step.

The consumer side will be implemented in the Background Worker issue.

---

## RabbitMQ Configuration

RabbitMQ is configured through appsettings.json.

Configuration section:

    "RabbitMq": {
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest",
      "QueueName": "price-refresh-queue"
    }

RabbitMQ runs through Docker Compose with the management UI enabled.

Local management panel:

    http://localhost:15672

Default local credentials:

    Username: guest
    Password: guest

---

## Docker Compose

RabbitMQ is added to the existing docker-compose.yml file.

It is added next to Redis because both services are local infrastructure dependencies for the project.

Expected services:

| Service | Purpose |
|---|---|
| Redis | Short-term quote cache |
| RabbitMQ | Async price refresh queue |

RabbitMQ ports:

| Port | Purpose |
|---|---|
| 5672 | AMQP communication |
| 15672 | Management UI |

---

## Publisher Responsibility

The publisher implementation is:

    RabbitMqPriceRefreshPublisher

It is responsible for:

- Reading RabbitMQ configuration
- Creating the RabbitMQ connection
- Declaring the queue
- Serializing PriceRefreshMessage
- Publishing the message
- Adding message metadata such as CorrelationId
- Logging successful publish operation

The publisher should not:

- Check WatchlistItem state
- Call external finance API
- Use Redis
- Create PriceSnapshot records
- Decide HTTP response codes

Those responsibilities belong to other layers.

---

## Dependency Injection

RabbitMQ options and publisher are registered in Program.cs.

Registration:

    builder.Services.Configure<RabbitMqOptions>(
        builder.Configuration.GetSection("RabbitMq"));

    builder.Services.AddScoped<IPriceRefreshPublisher, RabbitMqPriceRefreshPublisher>();

This allows the Controller to depend on the interface:

    IPriceRefreshPublisher

instead of depending directly on RabbitMqPriceRefreshPublisher.

This keeps the Controller easier to understand and easier to change later.

---

## Swagger Test Flow

The async refresh endpoint can be tested from Swagger.

Recommended test flow:

1. Check active watchlist items:

       GET /api/watchlist-items

2. Create a symbol if needed:

       POST /api/watchlist-items

   Example body:

       {
         "symbol": "TSLA",
         "displayName": "Tesla Inc.",
         "market": "NASDAQ"
       }

3. Send async refresh request:

       POST /api/watchlist-items/TSLA/refresh-async

4. Verify response:

       202 Accepted

       {
         "message": "Price refresh request accepted.",
         "symbol": "TSLA",
         "status": "Queued"
       }

5. Open RabbitMQ management panel.

6. Verify that price-refresh-queue exists.

7. Verify that the message is waiting in the queue.

Since the Background Worker is not implemented yet, the message is expected to remain in the queue.

---

## Missing Symbol Test

If the requested symbol does not exist as an active WatchlistItem, the endpoint should return 404 Not Found.

Example:

    POST /api/watchlist-items/FAKESYMBOL/refresh-async

Expected result:

    404 Not Found

No RabbitMQ message should be published.

---

## Inactive Symbol Test

If the symbol exists but is inactive because of soft delete, the endpoint should also return 404 Not Found.

Example flow:

1. Create a temporary symbol.
2. Delete it with the soft delete endpoint.
3. Call refresh-async for the same symbol.

Expected result:

    404 Not Found

No RabbitMQ message should be published.

---

## RabbitMQ Verification

After a successful async refresh request, RabbitMQ should show:

    price-refresh-queue

Expected message state before the worker is implemented:

| Metric | Expected value |
|---|---|
| Ready | 1 or more |
| Unacked | 0 |

If the endpoint is called multiple times successfully, the Ready count may increase.

That is expected because there is no consumer yet.

---

## Expected Logs

A successful publish should produce a structured log similar to:

    Async price refresh message published for symbol TSLA with correlation id ...

For this issue, the async endpoint should not produce logs related to actual quote refresh execution.

The following logs are not expected from refresh-async itself:

- Cache hit for symbol TSLA
- Cache miss for symbol TSLA
- Quote fetched from external API for symbol TSLA
- Price snapshot saved for symbol TSLA

Those logs belong to the actual refresh processing flow.

---

## Current Implementation Files

The current implementation includes:

    src/MarketInsight.Api/Options/RabbitMqOptions.cs
    src/MarketInsight.Api/Messaging/PriceRefreshMessage.cs
    src/MarketInsight.Api/Messaging/IPriceRefreshPublisher.cs
    src/MarketInsight.Api/Messaging/RabbitMqPriceRefreshPublisher.cs
    src/MarketInsight.Api/DTOs/Quotes/AsyncPriceRefreshResponse.cs
    src/MarketInsight.Api/Controllers/WatchlistItemsController.cs
    src/MarketInsight.Api/Program.cs
    docker-compose.yml
    appsettings.json

---

## Implementation Outcome

After this implementation:

- RabbitMQ was added to the local Docker Compose setup.
- RabbitMQ configuration was added to application settings.
- PriceRefreshMessage was created.
- IPriceRefreshPublisher was created.
- RabbitMqPriceRefreshPublisher was implemented.
- AsyncPriceRefreshResponse was created.
- POST /api/watchlist-items/{symbol}/refresh-async was added.
- The endpoint validates active WatchlistItem records before publishing.
- Missing and inactive symbols return 404 Not Found.
- Active symbols publish a message to RabbitMQ.
- Active symbols receive 202 Accepted.
- The async endpoint does not execute the full quote refresh flow directly.
- Project build completed successfully.

---

## Review Questions

1. Why does refresh-async return 202 Accepted instead of 200 OK?
2. What is the difference between refresh and refresh-async?
3. Why should the async endpoint check active WatchlistItem before publishing a message?
4. What does RabbitMQ store in this flow?
5. What fields does PriceRefreshMessage contain?
6. Why does the endpoint not call the external finance API directly?
7. Why should the Background Worker reuse QuoteRefreshService?
8. What does it mean if a message stays as Ready in RabbitMQ?
9. Why is CorrelationId useful in async processing?
10. Which part of the system will consume price-refresh-queue?

---

## Summary

RabbitMQ is used to introduce asynchronous price refresh processing into the project.

The refresh-async endpoint accepts valid requests for active watchlist symbols and publishes PriceRefreshMessage records to price-refresh-queue.

The endpoint does not perform the full quote refresh operation directly.

It does not call the external finance API, use Redis, or create PriceSnapshot records.

This keeps the HTTP request lightweight and prepares the project for Background Worker based processing.

In the next step, the Background Worker will consume messages from price-refresh-queue and reuse QuoteRefreshService to execute the existing refresh business flow.