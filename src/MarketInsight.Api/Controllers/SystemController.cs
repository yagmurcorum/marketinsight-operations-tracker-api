using System.Diagnostics;
using MarketInsight.Api.Data;
using MarketInsight.Api.DTOs.Observability;
using MarketInsight.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace MarketInsight.Api.Controllers;

/// <summary>
/// Provides basic system information and dependency status for the MarketInsight Operations Tracker API.
/// </summary>
[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private const string HealthyStatus = "Healthy";
    private const string UnhealthyStatus = "Unhealthy";
    private const string NotConfiguredStatus = "NotConfigured";

    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _dbContext;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly FinanceApiOptions _financeApiOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemController"/> class.
    /// </summary>
    /// <param name="environment">The web host environment.</param>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    /// <param name="rabbitMqOptions">RabbitMQ configuration options.</param>
    /// <param name="financeApiOptions">Finance API configuration options.</param>
    /// <param name="httpClientFactory">HTTP client factory used for external API checks.</param>
    public SystemController(
        IWebHostEnvironment environment,
        AppDbContext dbContext,
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IOptions<FinanceApiOptions> financeApiOptions,
        IHttpClientFactory httpClientFactory)
    {
        _environment = environment;
        _dbContext = dbContext;
        _connectionMultiplexer = connectionMultiplexer;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _financeApiOptions = financeApiOptions.Value;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Returns basic application name, version, and environment information.
    /// </summary>
    /// <returns>Basic system information about the API.</returns>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSystemInfo()
    {
        var response = new
        {
            applicationName = "MarketInsight Operations Tracker API",
            version = "1.0.0",
            environment = _environment.EnvironmentName
        };

        return Ok(response);
    }

    /// <summary>
    /// Returns basic dependency status information for SQLite, Redis, RabbitMQ and the external finance API.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Readable dependency status information for the application.</returns>
    [HttpGet("dependencies")]
    [ProducesResponseType(typeof(DependencyStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DependencyStatusResponse>> GetDependencies(CancellationToken cancellationToken)
    {
        var dependencies = new List<DependencyStatusItemResponse>
        {
            await CheckSQLiteAsync(cancellationToken),
            await CheckRedisAsync(),
            CheckRabbitMq(),
            await CheckExternalFinanceApiAsync(cancellationToken)
        };

        var response = new DependencyStatusResponse
        {
            OverallStatus = CalculateOverallStatus(dependencies),
            CheckedAtUtc = DateTime.UtcNow,
            Dependencies = dependencies
        };

        return Ok(response);
    }

    private async Task<DependencyStatusItemResponse> CheckSQLiteAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            stopwatch.Stop();

            if (!canConnect)
            {
                return new DependencyStatusItemResponse
                {
                    Name = "SQLite",
                    Status = UnhealthyStatus,
                    Message = "SQLite database connection is not available.",
                    ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
                };
            }

            return new DependencyStatusItemResponse
            {
                Name = "SQLite",
                Status = HealthyStatus,
                Message = "SQLite database connection is available.",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "SQLite",
                Status = UnhealthyStatus,
                Message = $"SQLite dependency check failed: {ex.Message}",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private async Task<DependencyStatusItemResponse> CheckRedisAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var database = _connectionMultiplexer.GetDatabase();

            await database.PingAsync();

            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "Redis",
                Status = HealthyStatus,
                Message = "Redis ping completed successfully.",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "Redis",
                Status = UnhealthyStatus,
                Message = $"Redis dependency check failed: {ex.Message}",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private DependencyStatusItemResponse CheckRabbitMq()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqOptions.HostName,
                Port = _rabbitMqOptions.Port,
                UserName = _rabbitMqOptions.UserName,
                Password = _rabbitMqOptions.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "RabbitMQ",
                Status = HealthyStatus,
                Message = "RabbitMQ connection is available.",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "RabbitMQ",
                Status = UnhealthyStatus,
                Message = $"RabbitMQ dependency check failed: {ex.Message}",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private async Task<DependencyStatusItemResponse> CheckExternalFinanceApiAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(_financeApiOptions.BaseUrl))
        {
            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "ExternalFinanceApi",
                Status = NotConfiguredStatus,
                Message = "External finance API base URL is not configured.",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }

        if (string.IsNullOrWhiteSpace(_financeApiOptions.ApiKey))
        {
            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "ExternalFinanceApi",
                Status = NotConfiguredStatus,
                Message = "External finance API key is not configured.",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            httpClient.Timeout = TimeSpan.FromSeconds(3);

            var baseUrl = NormalizeBaseUrl(_financeApiOptions.BaseUrl);
            var requestUrl = $"{baseUrl}quote?symbol=AAPL&token={Uri.EscapeDataString(_financeApiOptions.ApiKey)}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken);

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                return new DependencyStatusItemResponse
                {
                    Name = "ExternalFinanceApi",
                    Status = UnhealthyStatus,
                    Message = $"External finance API returned HTTP {(int)response.StatusCode}.",
                    ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
                };
            }

            return new DependencyStatusItemResponse
            {
                Name = "ExternalFinanceApi",
                Status = HealthyStatus,
                Message = "External finance API is reachable.",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new DependencyStatusItemResponse
            {
                Name = "ExternalFinanceApi",
                Status = UnhealthyStatus,
                Message = $"External finance API dependency check failed: {ex.Message}",
                ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private static string CalculateOverallStatus(List<DependencyStatusItemResponse> dependencies)
    {
        if (dependencies.Any(dependency => dependency.Status == UnhealthyStatus))
        {
            return UnhealthyStatus;
        }

        if (dependencies.Any(dependency => dependency.Status == NotConfiguredStatus))
        {
            return NotConfiguredStatus;
        }

        return HealthyStatus;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        return baseUrl.EndsWith('/')
            ? baseUrl
            : $"{baseUrl}/";
    }
}
