# User Secrets and Strategy Pattern

This document describes how the MarketInsight Operations Tracker API manages external finance API configuration and provider abstraction.

The purpose of this document is to explain how the finance API key is kept outside source code and how quote retrieval is routed through a simple provider abstraction.

---

## Purpose

The purpose of this configuration is to prevent sensitive API keys from being hardcoded in the project.

The project needs an external finance API key to retrieve quote data, but this key should not be stored in:

- `appsettings.json`
- `appsettings.Development.json`
- C# source files
- GitHub repository history
- Documentation files

Instead, local development uses .NET User Secrets.

This issue also introduces a simple provider abstraction so future quote refresh logic depends on `IQuoteProvider` instead of directly depending on the external finance API client.

---

## Current Scope

This implementation includes:

- `FinanceApiOptions`
- `IQuoteProvider`
- `FinnhubQuoteProvider`
- User Secrets configuration for the finance API key
- Provider registration through dependency injection
- HttpClient registration using finance API options

This implementation does not include:

- Redis cache
- PriceSnapshot persistence
- Refresh endpoint
- Snapshot listing endpoint
- RabbitMQ
- Background Worker
- Multiple provider implementations

---

## Configuration Model

Finance API configuration is represented by:

```text
src/MarketInsight.Api/Options/FinanceApiOptions.cs
```

The options model contains:

| Property | Source | Purpose |
|---|---|---|
| `BaseUrl` | `appsettings.json` | External finance API base URL |
| `Provider` | `appsettings.json` | Selected provider name |
| `ApiKey` | User Secrets | Secret API key used for local development |

The base URL and provider name can be stored in `appsettings.json` because they are not sensitive.

The API key must be stored in User Secrets.

---

## appsettings.json Configuration

The finance API section in `appsettings.json` should contain only non-secret values.

Expected configuration:

```json
{
  "FinanceApi": {
    "BaseUrl": "https://finnhub.io/api/v1/",
    "Provider": "Finnhub"
  }
}
```

The API key should not be added to this file.

Incorrect:

```json
{
  "FinanceApi": {
    "BaseUrl": "https://finnhub.io/api/v1/",
    "Provider": "Finnhub",
    "ApiKey": "secret-value"
  }
}
```

Reason:

```text
appsettings.json is committed to source control.
Secret values must not be committed.
```

---

## User Secrets Setup

User Secrets is used for local secret storage during development.

Initialize User Secrets for the API project:

```powershell
dotnet user-secrets init --project src/MarketInsight.Api
```

Set the finance API key:

```powershell
dotnet user-secrets set "FinanceApi:ApiKey" "YOUR_FINNHUB_API_KEY" --project src/MarketInsight.Api
```

Verify stored secrets:

```powershell
dotnet user-secrets list --project src/MarketInsight.Api
```

Expected result:

```text
FinanceApi:ApiKey = ...
```

The actual key value should not be shared in documentation, screenshots, commits, or chat messages.

---

## UserSecretsId

After running User Secrets initialization, the project file may contain a `UserSecretsId`.

Example:

```xml
<UserSecretsId>...</UserSecretsId>
```

This value can be committed.

It does not contain the actual API key.

The real secret value is stored outside the repository on the local development machine.

---

## Provider Abstraction

Quote retrieval should go through a provider abstraction.

Provider contract:

```text
src/MarketInsight.Api/Providers/Quotes/IQuoteProvider.cs
```

Provider implementation:

```text
src/MarketInsight.Api/Providers/Quotes/FinnhubQuoteProvider.cs
```

The selected provider for the current MVP is:

```text
FinnhubQuoteProvider
```

Only one provider is implemented at this stage.

This keeps the Strategy Pattern simple and beginner-friendly.

---

## Why Provider Abstraction Is Used

Without a provider abstraction, future quote refresh logic would depend directly on the external finance API client.

Avoid:

```text
QuoteRefreshService
      ↓
FinanceQuoteClient
      ↓
Finnhub API details
```

Preferred direction:

```text
QuoteRefreshService
      ↓
IQuoteProvider
      ↓
FinnhubQuoteProvider
      ↓
IFinanceQuoteClient
      ↓
FinanceQuoteClient
```

Reason:

```text
QuoteRefreshService should depend on the quote provider contract, not on provider-specific API details.
```

This keeps the refresh use case cleaner and easier to extend later.

---

## FinnhubQuoteProvider Responsibility

`FinnhubQuoteProvider` is responsible for using the finance quote client with configured finance API options.

It should:

- Read finance API options through `IOptions<FinanceApiOptions>`
- Validate that the API key is configured
- Use `IFinanceQuoteClient` to retrieve quote data
- Return `QuoteResponse`
- Keep Finnhub-specific provider behavior outside future refresh service logic

It should not:

- Store API keys directly
- Read secrets from hardcoded strings
- Implement Redis cache logic
- Save PriceSnapshot records
- Define API routes
- Return HTTP-specific response objects

---

## Dependency Injection Registration

The finance API options are registered in `Program.cs`.

Expected registration:

```csharp
builder.Services.Configure<FinanceApiOptions>(
    builder.Configuration.GetSection("FinanceApi"));
```

The provider abstraction is registered as:

```csharp
builder.Services.AddScoped<IQuoteProvider, FinnhubQuoteProvider>();
```

The finance API client is registered with HttpClient:

```csharp
builder.Services.AddHttpClient<IFinanceQuoteClient, FinanceQuoteClient>((serviceProvider, client) =>
{
    var financeApiOptions = serviceProvider
        .GetRequiredService<IOptions<FinanceApiOptions>>()
        .Value;

    if (string.IsNullOrWhiteSpace(financeApiOptions.BaseUrl))
    {
        throw new InvalidOperationException("Finance API base URL is not configured.");
    }

    client.BaseAddress = new Uri(financeApiOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});
```

This registration keeps base URL configuration centralized and prepares the provider for later quote refresh usage.

---

## Current Request Flow

At this stage, no Controller calls the provider directly.

The prepared flow is:

```text
Future QuoteRefreshService
      ↓
IQuoteProvider
      ↓
FinnhubQuoteProvider
      ↓
IFinanceQuoteClient
      ↓
FinanceQuoteClient
      ↓
External Finance API
```

The actual refresh endpoint will be implemented in a later issue.

---

## Secret Safety Rules

The project should follow these rules:

| Rule | Standard |
|---|---|
| API key in `appsettings.json` | Not allowed |
| API key in C# code | Not allowed |
| API key in documentation | Not allowed |
| API key in GitHub issue text | Not allowed |
| API key in screenshots | Not allowed |
| API key in User Secrets | Allowed for local development |

Before committing, check:

```powershell
git diff
```

The diff should not contain the real API key.

---

## Out of Scope

This document does not cover:

- Redis cache setup
- Cache-aside implementation
- PriceSnapshot persistence
- QuoteRefreshService implementation
- Refresh endpoint implementation
- Snapshot listing endpoint
- RabbitMQ
- Background Worker
- Production secret management
- Multiple finance provider implementations

These topics will be handled in later issues.

---

## Verification

To verify this step:

```powershell
dotnet build
```

Expected result:

```text
Build succeeded.
```

User Secrets can be checked with:

```powershell
dotnet user-secrets list --project src/MarketInsight.Api
```

Expected secret key name:

```text
FinanceApi:ApiKey
```

The actual secret value should not be copied into documentation or committed to the repository.

---

## Review Checklist

Before closing this issue, check:

- `FinanceApiOptions` exists.
- `FinanceApi:BaseUrl` is configurable.
- `FinanceApi:Provider` is configurable.
- `FinanceApi:ApiKey` is stored in User Secrets.
- API key is not hardcoded.
- Secret values are not committed.
- `IQuoteProvider` exists.
- `FinnhubQuoteProvider` exists.
- `FinnhubQuoteProvider` uses `IFinanceQuoteClient`.
- Strategy Pattern remains simple with one provider.
- Project builds successfully.

---

## Summary

This issue configures finance API settings and introduces a simple quote provider abstraction.

Implemented decisions:

- Finance API settings are represented by `FinanceApiOptions`.
- Base URL and provider name are stored in `appsettings.json`.
- API key is stored in User Secrets.
- Secret values are not committed.
- `IQuoteProvider` defines the provider contract.
- `FinnhubQuoteProvider` is the only provider implementation for the current MVP.
- The provider uses `IFinanceQuoteClient`.
- Future quote refresh logic will depend on `IQuoteProvider`, not directly on the external finance API client.

The next step is to add Redis cache support for quote data.