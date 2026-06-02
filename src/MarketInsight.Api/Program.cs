using MarketInsight.Api.Clients.Finance;
using MarketInsight.Api.Data;
using MarketInsight.Api.Options;
using MarketInsight.Api.Providers.Quotes;
using MarketInsight.Api.Repositories;
using MarketInsight.Api.Services;
using MarketInsight.Api.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWatchlistItemRepository, WatchlistItemRepository>();
builder.Services.AddScoped<IWatchlistItemService, WatchlistItemService>();

builder.Services.Configure<FinanceApiOptions>(
    builder.Configuration.GetSection("FinanceApi"));

builder.Services.AddScoped<IQuoteProvider, FinnhubQuoteProvider>();

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

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException("Redis connection string is not configured.");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<IQuoteCacheService, RedisQuoteCacheService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

    options.IncludeXmlComments(xmlFilePath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
