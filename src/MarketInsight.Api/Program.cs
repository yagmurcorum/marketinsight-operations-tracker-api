using MarketInsight.Api.Clients.Finance;
using MarketInsight.Api.Services;
using System.Reflection;
using MarketInsight.Api.Data;
using MarketInsight.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWatchlistItemRepository, WatchlistItemRepository>();
builder.Services.AddScoped<IWatchlistItemService, WatchlistItemService>();

var financeApiBaseUrl = builder.Configuration["FinanceApi:BaseUrl"];

builder.Services.AddHttpClient<IFinanceQuoteClient, FinanceQuoteClient>(client =>
{
    if (string.IsNullOrWhiteSpace(financeApiBaseUrl))
    {
        throw new InvalidOperationException("Finance API base URL is not configured.");
    }

    client.BaseAddress = new Uri(financeApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

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
