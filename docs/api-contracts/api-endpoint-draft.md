# Controller Endpoint Draft and System Info Endpoint

## Purpose

This document describes the initial controller endpoint structure used in the MarketInsight Operations Tracker API.

It also documents the second basic API endpoint of the project:

    GET /api/system/info

The purpose of this endpoint is to return basic system information about the API.

---

## Request-Response Flow

The system info request-response flow is:

    Client / Swagger
      ↓
    HTTP GET Request
      ↓
    ASP.NET Core Web API
      ↓
    SystemController
      ↓
    GetSystemInfo()
      ↓
    200 OK Response

In the current implementation, Swagger is used to test the endpoint and verify the returned system information.

---

## Controller-Based API Structure

The project uses controller-based ASP.NET Core Web API structure.

Controllers are responsible for handling HTTP requests and returning HTTP responses.

As the project grows, business logic will be separated into service classes instead of being kept directly inside controllers.

---

## Implemented Endpoint

| Method | Route | Controller | Purpose |
|---|---|---|---|
| GET | `/api/system/info` | `SystemController` | Returns basic application and environment information |

---

## Endpoint Response

Expected status code:

    200 OK

Expected response body:

    {
      "applicationName": "MarketInsight Operations Tracker API",
      "version": "1.0.0",
      "environment": "Development"
    }

Response fields:

| Field | Description |
|---|---|
| `applicationName` | Application name |
| `version` | Current API version |
| `environment` | Current runtime environment |

---

## Implementation Location

The system info endpoint is implemented in:

    src/MarketInsight.Api/Controllers/SystemController.cs

The controller route is:

    api/system

The action route is:

    info

The final endpoint route is:

    GET /api/system/info

---

## Route Structure

The final route is created by combining the controller route and the action route.

Controller route:

    [Route("api/system")]

Action route:

    [HttpGet("info")]

Combined route:

    GET /api/system/info

This keeps the endpoint path readable and consistent with the current API route structure.

---

## Action Method

The action method is:

    GetSystemInfo()

The method responds to:

    HTTP GET

Return type:

    IActionResult

The method returns an HTTP 200 OK response with a JSON response body.

---

## Response Model

The current response is returned as a simple anonymous object.

Current response fields:

    applicationName
    version
    environment

This is acceptable for the current early-stage endpoint.

As the project grows, response models will be moved into DTO classes when needed.

---

## Environment Information

The endpoint uses `IWebHostEnvironment` to read the current application environment.

The environment value is returned from:

    _environment.EnvironmentName

Example value during local development:

    Development

---

## XML Summary Documentation

The endpoint uses XML Summary documentation so that Swagger can display a readable endpoint description.

Controller summary:

    Provides basic system information about the MarketInsight Operations Tracker API.

Action summary:

    Returns basic application name, version, and environment information.

The endpoint also includes response metadata:

    [ProducesResponseType(StatusCodes.Status200OK)]

This helps Swagger show the expected response status code.

---

## Swagger Verification

The endpoint was tested through Swagger.

Tested endpoint:

    GET /api/system/info

Result:

    200 OK

Verified response body:

    {
      "applicationName": "MarketInsight Operations Tracker API",
      "version": "1.0.0",
      "environment": "Development"
    }

This confirms that:

- The API runs locally.
- `SystemController` is registered correctly.
- The route is reachable.
- The action method responds to HTTP GET requests.
- Swagger displays the endpoint.
- XML Summary documentation is visible in Swagger.
- The endpoint returns a valid HTTP response.

---

## Controller Responsibility

Controllers should handle HTTP request and response flow.

A controller may:

- Receive a request
- Call the required application logic
- Return an appropriate HTTP response

Controllers should not contain large business logic.

As the project grows, business logic will be moved into service classes.

For this endpoint, returning a simple system information response directly from the controller is acceptable because the endpoint has no business workflow yet.

---

## Current Scope

This document covers the initial controller endpoint draft and the `GET /api/system/info` endpoint only.

The following topics are outside the scope of this document and will be documented separately as they are implemented:

- DTO design
- Service Layer
- Repository Pattern
- SQLite and EF Core setup
- Watchlist CRUD operations
- Redis cache
- RabbitMQ messaging
- External Finance API integration
- Error handling
- Dependency status endpoint

---

## Related Files

| File | Purpose |
|---|---|
| `src/MarketInsight.Api/Controllers/SystemController.cs` | Implements the system info endpoint |
| `src/MarketInsight.Api/Controllers/HealthController.cs` | Existing health endpoint controller |
| `src/MarketInsight.Api/Program.cs` | Registers controllers and Swagger |
| `docs/04-api-endpoint-draft.md` | Documents the initial controller endpoint draft |

---

## Status

The `GET /api/system/info` endpoint is implemented and verified through Swagger.

The endpoint returns basic application name, version, and environment information.