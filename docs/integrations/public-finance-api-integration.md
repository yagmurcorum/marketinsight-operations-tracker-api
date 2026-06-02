# Public Finance API Integration

This document describes the public finance API client foundation used by the MarketInsight Operations Tracker API.

The purpose of this document is to explain how external quote data is retrieved and mapped into an internal API model before Redis cache, PriceSnapshot persistence, and refresh endpoint implementation are added.

---

## Purpose

The purpose of this integration is to retrieve quote data for a financial symbol from an external finance API.

In Week 3, the project starts preparing the foundation for quote refresh operations.

This document covers only the finance API client foundation.

---

## Current Scope

This implementation includes:

- `IFinanceQuoteClient`
- `FinanceQuoteClient`
- `FinnhubQuoteResponse`
- `QuoteResponse`
- HttpClient usage
- Basic timeout configuration
- Basic failed response handling
- Mapping external API response data to an internal quote model

This implementation does not include:

- User Secrets setup
- Finance API key configuration
- Redis cache
- PriceSnapshot persistence
- Refresh endpoint
- RabbitMQ
- Background Worker

---

## External API Boundary

The external finance API response should not be exposed directly to Controllers or API consumers.

External response model:

```text
FinnhubQuoteResponse
```

Internal quote model:

```text
QuoteResponse
```

Reason:

```text
External API fields may be provider-specific, short, or unstable.
The internal API should use clear and project-specific property names.
```

---

## Expected Request Flow

```text
Application service
      ↓
IFinanceQuoteClient
      ↓
FinanceQuoteClient
      ↓
HttpClient
      ↓
External Finance API
      ↓
FinnhubQuoteResponse
      ↓
QuoteResponse
```

At this stage, no Controller calls the finance API client directly.

The client is prepared for later use by the quote provider and QuoteRefreshService.

---

## Implemented Files

```text
src/MarketInsight.Api/Clients/Finance/IFinanceQuoteClient.cs
src/MarketInsight.Api/Clients/Finance/FinanceQuoteClient.cs
src/MarketInsight.Api/Clients/Finance/FinnhubQuoteResponse.cs
src/MarketInsight.Api/DTOs/Quotes/QuoteResponse.cs
```

---

## HttpClient Usage

The finance API client uses `HttpClient`.

The client is registered through dependency injection in `Program.cs`.

Configured behavior:

| Setting | Value |
|---|---|
| Base URL | `FinanceApi:BaseUrl` |
| Timeout | 10 seconds |

The base URL is stored in `appsettings.json`.

The API key is not stored in `appsettings.json`.

API key configuration will be handled later through User Secrets.

---

## Response Mapping

The external API response uses provider-specific field names.

Example mapping:

| External Field | Internal Property |
|---|---|
| `c` | `CurrentPrice` |
| `d` | `Change` |
| `dp` | `PercentChange` |
| `h` | `HighPriceOfDay` |
| `l` | `LowPriceOfDay` |
| `o` | `OpenPriceOfDay` |
| `pc` | `PreviousClosePrice` |
| `t` | `RetrievedAtUtc` |

The internal API uses `QuoteResponse` to keep property names clear and understandable.

---

## Error Handling

FinanceQuoteClient handles basic failure scenarios.

Handled cases:

- Empty symbol
- Missing API key
- Non-success HTTP status code
- Empty external response
- Invalid quote data
- Timeout or canceled request
- HTTP request failure
- Invalid JSON response

Current behavior:

```text
The client returns null when quote data cannot be retrieved safely.
```

The later QuoteRefreshService will decide how to convert this result into an application-level outcome.

---

## Controller Responsibility

Controllers should not call the external finance API directly.

Correct direction:

```text
Controller
      ↓
Service
      ↓
Quote provider / Finance client
```

Incorrect direction:

```text
Controller
      ↓
HttpClient
      ↓
External Finance API
```

Reason:

```text
External API details should stay outside the Controller layer.
```

---

## Out of Scope

This document does not cover:

- User Secrets
- FinanceApiOptions
- Quote provider strategy
- Redis cache
- PriceSnapshot persistence
- Refresh endpoint
- Snapshot listing endpoint
- RabbitMQ
- Background Worker

These topics will be handled in later Week 3 and Week 4 issues.

---

## Verification

To verify this step:

```bash
dotnet build
```

Expected result:

```text
Build succeeded.
```

No Swagger endpoint is expected for this issue because the finance client is not connected to a Controller yet.

---

## Summary

This issue adds the public finance API client foundation.

Implemented decisions:

- External API response model is separated from the internal quote model.
- `IFinanceQuoteClient` defines the client contract.
- `FinanceQuoteClient` uses HttpClient.
- Failed external API responses are handled safely.
- `QuoteResponse` represents the internal quote model.
- Controllers do not call the external finance API directly.
- Redis, PriceSnapshot persistence, and refresh endpoints are intentionally not included in this issue.

The next step is to configure User Secrets and add a simple quote provider strategy.