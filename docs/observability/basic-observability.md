# Basic Observability

This document explains the basic observability behavior used in the MarketInsight Operations Tracker API.

The goal is to make application health and dependency status visible through Swagger in a beginner-friendly way.

---

## Purpose

The project uses basic observability endpoints to show whether the API and its main dependencies are available.

The current observability scope includes:

- Basic API health.
- Basic system information.
- Dependency status checks.
- SQLite status.
- Redis status.
- RabbitMQ status.
- External finance API status.
- Observability feature summary.

These endpoints are designed for local development, learning and final demo purposes.

---

## Why Basic Observability Is Used

Basic observability helps explain what is happening inside the application without adding production-grade monitoring tools.

In this project, observability answers three simple questions:

- Is the API running?
- Which environment is the API running in?
- Are the main dependencies reachable?

This keeps the system easier to test, explain and demonstrate.

---

## Observability Endpoints

The project exposes the following observability-related endpoints:

| Endpoint | Purpose |
|---|---|
| GET /api/health | Returns basic API health information |
| GET /api/system/info | Returns basic application and environment information |
| GET /api/system/dependencies | Returns dependency status information |
| GET /api/observability/summary | Returns a summary of observability features |

These endpoints are visible and testable from Swagger.

---

## Health Endpoint

The health endpoint is:

    GET /api/health

Purpose:

    Checks whether the API application is running.

Expected response:

    {
        "status": "Healthy",
        "application": "MarketInsight Operations Tracker API",
        "timestamp": "..."
    }

This endpoint only confirms that the API application is reachable.

It does not check SQLite, Redis, RabbitMQ or the external finance API.

Implementation file:

    src/MarketInsight.Api/Controllers/HealthController.cs

---

## System Info Endpoint

The system info endpoint is:

    GET /api/system/info

Purpose:

    Returns basic application information.

Expected response:

    {
        "applicationName": "MarketInsight Operations Tracker API",
        "version": "1.0.0",
        "environment": "Development"
    }

This endpoint is useful for confirming the running application name, version and environment during local testing.

Implementation file:

    src/MarketInsight.Api/Controllers/SystemController.cs

---

## Dependency Status Endpoint

The dependency status endpoint is:

    GET /api/system/dependencies

Purpose:

    Returns readable status information for the application's main dependencies.

Checked dependencies:

- SQLite.
- Redis.
- RabbitMQ.
- External finance API.

Expected response shape:

    {
        "overallStatus": "Healthy",
        "checkedAtUtc": "...",
        "dependencies": [
            {
                "name": "SQLite",
                "status": "Healthy",
                "message": "SQLite database connection is available.",
                "responseTimeMilliseconds": 1
            },
            {
                "name": "Redis",
                "status": "Healthy",
                "message": "Redis ping completed successfully.",
                "responseTimeMilliseconds": 1
            },
            {
                "name": "RabbitMQ",
                "status": "Healthy",
                "message": "RabbitMQ connection is available.",
                "responseTimeMilliseconds": 30
            },
            {
                "name": "ExternalFinanceApi",
                "status": "Healthy",
                "message": "External finance API is reachable.",
                "responseTimeMilliseconds": 300
            }
        ]
    }

Implementation file:

    src/MarketInsight.Api/Controllers/SystemController.cs

---

## Dependency Status Values

The project uses simple dependency status values.

| Status | Meaning |
|---|---|
| Healthy | Dependency is available |
| Unhealthy | Dependency check failed |
| NotConfigured | Required configuration is missing |

These values keep the response easy to understand in Swagger.

---

## Overall Status Calculation

The dependency endpoint calculates an overall status from individual dependency results.

Current rules:

- If any dependency is Unhealthy, overallStatus becomes Unhealthy.
- If no dependency is Unhealthy but at least one dependency is NotConfigured, overallStatus becomes NotConfigured.
- If all dependencies are Healthy, overallStatus becomes Healthy.

This gives a quick summary while still showing detailed dependency results.

---

## SQLite Dependency Check

SQLite is checked through Entity Framework Core.

Check behavior:

    AppDbContext.Database.CanConnectAsync()

Expected healthy result:

    SQLite database connection is available.

Expected unhealthy result:

    SQLite dependency check failed: ...

This confirms whether the application can connect to the local SQLite database.

---

## Redis Dependency Check

Redis is checked through the registered Redis connection multiplexer.

Check behavior:

    IConnectionMultiplexer.GetDatabase().PingAsync()

Expected healthy result:

    Redis ping completed successfully.

Expected unhealthy result:

    Redis dependency check failed: ...

This confirms whether the API can reach Redis.

Redis is used by the quote cache flow.

---

## RabbitMQ Dependency Check

RabbitMQ is checked by creating a basic RabbitMQ connection and channel.

Check behavior:

    ConnectionFactory
        ↓
    CreateConnection()
        ↓
    CreateModel()

Expected healthy result:

    RabbitMQ connection is available.

Expected unhealthy result:

    RabbitMQ dependency check failed: ...

This confirms whether the API can reach RabbitMQ.

RabbitMQ is used by the async price refresh flow.

---

## External Finance API Dependency Check

The external finance API check verifies that the external API configuration is usable.

The check first validates configuration.

If BaseUrl is missing:

    External finance API base URL is not configured.

If ApiKey is missing:

    External finance API key is not configured.

In these cases, the dependency status becomes:

    NotConfigured

If configuration exists, the endpoint sends a short request to the external finance API.

The check uses a short timeout:

    3 seconds

This prevents the dependency endpoint from waiting too long during local testing.

Expected healthy result:

    External finance API is reachable.

Expected unhealthy result:

    External finance API returned HTTP ...
    External finance API dependency check failed: ...

---

## NotConfigured Behavior

NotConfigured is used when the application is running but a dependency cannot be fully checked because required configuration is missing.

Current example:

    ExternalFinanceApi

If the API key is missing, the endpoint should not call the external finance API.

Instead, it returns a readable NotConfigured response.

Example:

    {
        "name": "ExternalFinanceApi",
        "status": "NotConfigured",
        "message": "External finance API key is not configured.",
        "responseTimeMilliseconds": 0
    }

This is clearer than returning a generic failure.

---

## Observability Summary Endpoint

The observability summary endpoint is:

    GET /api/observability/summary

Purpose:

    Returns a beginner-friendly summary of the project's observability capabilities.

Expected response includes:

- Application name.
- Environment.
- Generated UTC time.
- Observability summary text.
- Available observability endpoints.
- Implemented observability features.

Expected response shape:

    {
        "applicationName": "MarketInsight Operations Tracker API",
        "environment": "Development",
        "generatedAtUtc": "...",
        "summary": "...",
        "endpoints": [
            "GET /api/health",
            "GET /api/system/info",
            "GET /api/system/dependencies",
            "GET /api/observability/summary"
        ],
        "observabilityFeatures": [
            "Basic health endpoint",
            "System information endpoint",
            "Dependency status endpoint",
            "SQLite dependency check",
            "Redis dependency check",
            "RabbitMQ dependency check",
            "External finance API dependency check",
            "Structured logging placeholders",
            "Redis cache hit and miss logs",
            "RabbitMQ publish and consume logs",
            "CorrelationId usage in async RabbitMQ flow",
            "Swagger XML Summary documentation"
        ]
    }

Implementation file:

    src/MarketInsight.Api/Controllers/ObservabilityController.cs

---

## Swagger Documentation

The observability endpoints include XML Summary documentation and Swagger response metadata.

Example:

    /// <summary>
    /// Returns basic dependency status information for SQLite, Redis, RabbitMQ and the external finance API.
    /// </summary>
    [ProducesResponseType(typeof(DependencyStatusResponse), StatusCodes.Status200OK)]

And:

    /// <summary>
    /// Returns a summary of the application's basic observability features.
    /// </summary>
    [ProducesResponseType(typeof(ObservabilitySummaryResponse), StatusCodes.Status200OK)]

This makes the endpoints easier to understand in Swagger.

---

## Runtime Verification

The observability endpoints can be verified through Swagger.

### Health Test

Request:

    GET /api/health

Expected result:

    200 OK

Expected status:

    Healthy

---

### Dependency Status Test

Request:

    GET /api/system/dependencies

Expected result:

    200 OK

Expected dependency names:

- SQLite.
- Redis.
- RabbitMQ.
- ExternalFinanceApi.

Expected readable fields:

- name.
- status.
- message.
- responseTimeMilliseconds.

---

### Observability Summary Test

Request:

    GET /api/observability/summary

Expected result:

    200 OK

Expected content:

- Application name.
- Environment.
- Endpoint list.
- Observability feature list.

---

## Current Implementation Files

The current basic observability implementation uses these files:

    src/MarketInsight.Api/Controllers/HealthController.cs
    src/MarketInsight.Api/Controllers/SystemController.cs
    src/MarketInsight.Api/Controllers/ObservabilityController.cs
    src/MarketInsight.Api/DTOs/Observability/DependencyStatusItemResponse.cs
    src/MarketInsight.Api/DTOs/Observability/DependencyStatusResponse.cs
    src/MarketInsight.Api/DTOs/Observability/ObservabilitySummaryResponse.cs
    src/MarketInsight.Api/Options/FinanceApiOptions.cs
    src/MarketInsight.Api/Options/RabbitMqOptions.cs
    src/MarketInsight.Api/Program.cs

---

## What This Implementation Does Not Cover

This implementation does not add:

- Production monitoring.
- OpenTelemetry.
- Metrics dashboard.
- Alerting integration.
- Advanced health check UI.
- Authentication.
- Centralized monitoring tools.

The goal is basic observability, not production-grade monitoring.

---

## Implementation Outcome

After this implementation:

- The API exposes a basic health endpoint.
- The API exposes basic system information.
- The API exposes dependency status information.
- SQLite status is checked.
- Redis status is checked.
- RabbitMQ status is checked.
- External finance API status is checked.
- Missing external API configuration returns NotConfigured.
- External API checks use a short timeout.
- Dependency status responses are readable in Swagger.
- XML Summary documentation appears in Swagger.
- Basic observability behavior is documented.

---

## Review Questions

1. What is the purpose of GET /api/health?
2. Why does GET /api/health not check dependencies?
3. Which endpoint checks SQLite, Redis, RabbitMQ and the external finance API?
4. What does Healthy mean?
5. What does Unhealthy mean?
6. What does NotConfigured mean?
7. Why should missing API key return NotConfigured instead of a generic error?
8. Why does the external finance API check use a short timeout?
9. What does GET /api/observability/summary explain?
10. Why is OpenTelemetry out of scope for this MVP?

---

## Summary

Basic observability makes the API easier to test, explain and demonstrate through Swagger.

The API can show:

- Whether the application is running.
- Basic system information.
- SQLite status.
- Redis status.
- RabbitMQ status.
- External finance API status.
- A summary of available observability features.

The final observability flow is:

    Swagger
        ↓
    /api/health
    /api/system/info
    /api/system/dependencies
    /api/observability/summary
        ↓
    Readable API and dependency status responses

This keeps the implementation simple, beginner-friendly and aligned with the MVP scope.