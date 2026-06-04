# Background Worker Design

This document explains how the Background Worker consumes RabbitMQ price refresh messages in the MarketInsight Operations Tracker API.

The purpose of this document is to define how queued async refresh requests are processed without duplicating quote refresh business logic inside the worker.

---

## Purpose

The project uses a Background Worker to process asynchronous price refresh messages from RabbitMQ.

The async endpoint accepts the request and publishes a message.

The Background Worker consumes that message and triggers the existing quote refresh flow.

This keeps the HTTP request lightweight and moves the actual processing to a background execution path.

---

## Why Background Worker Is Used

The Background Worker is used to separate message consumption from HTTP request handling.

Without a worker, messages published to RabbitMQ would remain in the queue and never be processed.

With the worker:

| Step | Responsibility |
|---|---|
| API endpoint | Accepts request and publishes message |
| RabbitMQ | Stores queued work |
| Background Worker | Consumes queued messages |
| QuoteRefreshService | Executes quote refresh business flow |
| SQLite | Stores PriceSnapshot records |

The worker does not replace QuoteRefreshService.

It only triggers the existing refresh use case after reading a message from RabbitMQ.

---

## Layer Responsibility Reminder

The project keeps responsibilities separated.

| Layer | Main Responsibility |
|---|---|
| Controller | Handles HTTP requests and responses |
| Publisher | Publishes messages to RabbitMQ |
| RabbitMQ | Holds queued work |
| Background Worker | Consumes messages and coordinates processing |
| Service | Handles quote refresh business flow |
| Repository | Handles database access |
| External API Client | Communicates with the finance API |
| Redis Cache Service | Handles quote cache operations |

The worker should not become a second quote refresh service.

The refresh business logic should remain in QuoteRefreshService.

---

## Worker Responsibility

The Background Worker is responsible for:

• Connecting to RabbitMQ.
• Declaring the price refresh queue.
• Consuming PriceRefreshMessage records.
• Deserializing message content.
• Reading Symbol and CorrelationId.
• Creating a logging scope.
• Resolving IQuoteRefreshService through a service scope.
• Calling RefreshQuoteAsync.
• Acknowledging successfully processed messages.
• Logging failures.

The worker is an orchestration component.

It should not contain direct quote refresh business rules.

---

## What The Worker Does Not Do

The worker should not directly:

• Call the external finance API.
• Read or write Redis cache.
• Create PriceSnapshot entities manually.
• Access AppDbContext directly.
• Decide quote refresh business rules.
• Implement alert evaluation.
• Implement advanced retry or dead-letter queue behavior.

Those responsibilities either already belong to QuoteRefreshService or are outside the current MVP scope.

---

## Expected Worker Flow

The expected async processing flow is:

    RabbitMQ price-refresh-queue
        ↓
    PriceRefreshBackgroundWorker
        ↓
    PriceRefreshMessage
        ↓
    CorrelationId log scope
        ↓
    IQuoteRefreshService.RefreshQuoteAsync
        ↓
    QuoteRefreshService
        ↓
    WatchlistItemRepository
        ↓
    QuoteCacheService
        ↓
    QuoteProvider / FinanceQuoteClient
        ↓
    PriceSnapshotRepository
        ↓
    SQLite

This flow reuses the existing synchronous refresh business logic.

---

## Queue Standard

The worker consumes messages from:

    price-refresh-queue

This is the same queue used by the async refresh publisher.

The queue is declared by both producer and consumer sides to keep local startup predictable.

Current queue behavior:

| Situation | Expected behavior |
|---|---|
| Message published and worker is running | Message is consumed |
| Message processed successfully | Message is acknowledged |
| Worker is not running | Message remains Ready |
| Invalid message received | Message is logged and acknowledged |
| Unexpected processing failure | Message is logged and rejected without requeue |

---

## Message Contract

The worker consumes:

    PriceRefreshMessage

Current message fields:

| Field | Purpose |
|---|---|
| Symbol | Normalized symbol requested for refresh |
| RequestedAtUtc | UTC time when the async request was created |
| CorrelationId | Identifier used to track the async flow in logs |

Message shape:

    public class PriceRefreshMessage
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }

The worker must read Symbol and CorrelationId from the message.

RequestedAtUtc is useful for traceability and can support future timing analysis.

---

## Service Scope Usage

PriceRefreshBackgroundWorker is a hosted service.

Hosted services are long-running application services.

QuoteRefreshService is registered as a scoped service.

Because of this, the worker should not inject IQuoteRefreshService directly as a field dependency.

Instead, it uses IServiceScopeFactory.

Expected approach:

    using var scope = _serviceScopeFactory.CreateScope();

    var quoteRefreshService = scope.ServiceProvider
        .GetRequiredService<IQuoteRefreshService>();

    await quoteRefreshService.RefreshQuoteAsync(message.Symbol, cancellationToken);

This keeps dependency lifetimes correct.

---

## Relationship With QuoteRefreshService

QuoteRefreshService owns the quote refresh business flow.

It is responsible for:

• Checking active WatchlistItem state.
• Reading quote data from Redis if available.
• Calling the quote provider when cache is missing.
• Saving PriceSnapshot records.
• Returning refresh result information.

The worker only calls QuoteRefreshService.

This prevents business logic duplication.

The same refresh rules are used by both:

| Endpoint / Component | Processing style |
|---|---|
| POST /api/watchlist-items/{symbol}/refresh | Synchronous |
| Background Worker | Asynchronous |

Both paths rely on the same QuoteRefreshService behavior.

---

## Acknowledgement Behavior

The worker uses manual acknowledgement.

Successful processing:

    BasicAck

Unexpected failure:

    BasicNack with requeue: false

For this MVP, advanced retry and dead-letter queue behavior are intentionally not implemented.

This keeps the worker understandable and aligned with the learning-focused scope.

---

## Logging Behavior

The worker logs key processing events.

Expected logs include:

• Price refresh background worker started consuming queue price-refresh-queue.
• Async price refresh message consumed for symbol TSLA with correlation id ...
• Queued price refresh completed for symbol TSLA.
• Queued price refresh failed for symbol TSLA.
• Invalid price refresh message received from RabbitMQ.

The worker uses CorrelationId in the logging scope.

This helps connect the published message and the consumed message during async processing.

---

## Runtime Verification

The worker can be tested through Swagger and RabbitMQ Management UI.

Recommended test flow:

1. Start Docker Compose.

       docker compose up -d

2. Confirm Redis and RabbitMQ containers are running.

3. Start the API.

4. Confirm the worker startup log appears.

       Price refresh background worker started consuming queue price-refresh-queue

5. Send an async refresh request.

       POST /api/watchlist-items/TSLA/refresh-async

6. Verify response.

       202 Accepted

7. Open RabbitMQ Management UI.

       http://localhost:15672

8. Check price-refresh-queue.

9. Verify the message is consumed.

10. Verify queue counts return to zero.

       Ready: 0
       Unacked: 0

11. Check API logs for consume and completion logs.

12. Verify snapshot creation.

       GET /api/watchlist-items/TSLA/snapshots

Expected result:

The snapshot list should include a new PriceSnapshot created by the worker-triggered refresh flow.

---

## Current Implementation Files

The current implementation includes:

    src/MarketInsight.Api/Workers/PriceRefreshBackgroundWorker.cs
    src/MarketInsight.Api/Messaging/PriceRefreshMessage.cs
    src/MarketInsight.Api/Messaging/IPriceRefreshPublisher.cs
    src/MarketInsight.Api/Messaging/RabbitMqPriceRefreshPublisher.cs
    src/MarketInsight.Api/Services/Quotes/IQuoteRefreshService.cs
    src/MarketInsight.Api/Services/Quotes/QuoteRefreshService.cs
    src/MarketInsight.Api/Options/RabbitMqOptions.cs
    src/MarketInsight.Api/Program.cs
    docker-compose.yml
    appsettings.json

---

## Dependency Injection Registration

The worker is registered in Program.cs.

    builder.Services.AddHostedService<PriceRefreshBackgroundWorker>();

The worker uses IServiceScopeFactory to resolve scoped services during message processing.

This registration allows the worker to start automatically when the API application starts.

---

## Implementation Outcome

After this implementation:

• PriceRefreshBackgroundWorker was created.
• The worker consumes messages from price-refresh-queue.
• PriceRefreshMessage records are deserialized.
• Symbol and CorrelationId are read from the message.
• CorrelationId is used in the log scope.
• IQuoteRefreshService is resolved through a service scope.
• QuoteRefreshService.RefreshQuoteAsync is called by the worker.
• Existing quote refresh business logic is reused.
• Refresh logic is not duplicated inside the worker.
• Successfully processed messages are acknowledged.
• Failed processing is logged.
• The worker is registered in Program.cs.
• Runtime testing confirmed that async refresh messages are consumed.
• Runtime testing confirmed that PriceSnapshot records are created after worker processing.
• Project build completed successfully.

---

## Review Questions

1. Why does the worker use IServiceScopeFactory?
2. Why should IQuoteRefreshService not be directly stored as a scoped dependency in the worker?
3. What does the worker consume from RabbitMQ?
4. What is the purpose of CorrelationId?
5. What does BasicAck mean?
6. What happens if the worker is not running?
7. Why should the worker not call the external finance API directly?
8. Why should the worker reuse QuoteRefreshService?
9. How can RabbitMQ Management UI prove that the message was consumed?
10. How can Swagger prove that the worker created a PriceSnapshot?

---

## Summary

The Background Worker completes the asynchronous price refresh flow.

The async endpoint publishes a PriceRefreshMessage to RabbitMQ.

The worker consumes that message, creates a processing scope, resolves IQuoteRefreshService, and triggers the existing quote refresh flow.

This design keeps business logic in QuoteRefreshService and keeps the worker focused on message consumption and orchestration.

The resulting flow is:

    refresh-async endpoint
        ↓
    RabbitMQ queue
        ↓
    Background Worker
        ↓
    QuoteRefreshService
        ↓
    Redis / External API / SQLite PriceSnapshot

This makes the project ready to demonstrate queue-based async processing with RabbitMQ and Background Worker.