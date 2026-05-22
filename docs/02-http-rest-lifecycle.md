# HTTP/HTTPS Lifecycle and Health Endpoint

## Purpose

This document describes the initial HTTP/HTTPS request-response flow used in the MarketInsight Operations Tracker API.

It also documents the first health check endpoint of the project:

    GET /api/health

The purpose of this endpoint is to verify that the API is running and reachable.

---

## Request-Response Flow

The initial request-response flow is:

    Client / Swagger
      ↓
    HTTP Request
      ↓
    ASP.NET Core Web API
      ↓
    Controller
      ↓
    Action Method
      ↓
    HTTP Response

In the current implementation, Swagger is used as the first API testing surface.

---

## HTTPS Configuration

HTTPS is enabled in the API project.

This keeps the local development setup aligned with common Web API practices and allows the application to run over a secure local endpoint.

---

## Controller-Based API Structure

The project uses controller-based ASP.NET Core Web API structure.

Controllers are responsible for handling HTTP requests and returning HTTP responses.

As the project grows, business logic will be separated into service classes instead of being kept directly inside controllers.

---

## Implemented Endpoint

| Method | Route | Controller | Purpose |
|---|---|---|---|
| GET | `/api/health` | `HealthController` | Returns basic API health information |

---

## Endpoint Response

Expected status code:

    200 OK

Expected response body:

    {
      "status": "Healthy",
      "application": "MarketInsight Operations Tracker API",
      "timestamp": "2026-05-22T10:00:00Z"
    }

Response fields:

| Field | Description |
|---|---|
| `status` | Indicates the current health status of the API |
| `application` | Application name |
| `timestamp` | UTC timestamp of the response |

---

## Implementation Location

The health endpoint is implemented in:

    src/MarketInsight.Api/Controllers/HealthController.cs

The controller route is:

    api/health

The action responds to:

    HTTP GET

---

## Swagger Verification

The endpoint was tested through Swagger.

Tested endpoint:

    GET /api/health

Result:

    200 OK

This confirms that:

- The API runs locally.
- The route is reachable.
- The controller is registered correctly.
- Swagger can call the endpoint.
- The API returns a valid HTTP response.

---

## Current Scope

This document covers the initial request-response flow and the health endpoint only.

The following topics are outside the scope of this document and will be documented separately as they are implemented:

- Entity and DTO design
- SQLite and EF Core setup
- Repository Pattern
- Service Layer
- Redis cache
- RabbitMQ messaging
- External Finance API integration
- Watchlist CRUD operations

---

## Status

The health endpoint is implemented and verified through Swagger.