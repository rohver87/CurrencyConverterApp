using CurrencyConverterApp;
using CurrencyConverterApp.Service;
using Microsoft.AspNetCore.RateLimiting;
using Polly;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

string? CurrencyConverterApiClient = builder.Configuration["CurrencyConverterApiClient"];
ArgumentException.ThrowIfNullOrEmpty(CurrencyConverterApiClient);

string? CurrencyConverterApiBaseUrl = builder.Configuration["CurrencyConverterApiBaseUrl"];
ArgumentException.ThrowIfNullOrEmpty(CurrencyConverterApiBaseUrl);

builder.Services.AddHttpClient(
    CurrencyConverterApiClient,
    client =>
    {
        client.BaseAddress = new Uri(CurrencyConverterApiBaseUrl);
    })
    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
      {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    }))
    .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30)
    ));

builder.Services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var tokenPolicy = "token";

builder.Services.AddRateLimiter(_ => _
    .AddTokenBucketLimiter(policyName: tokenPolicy, options =>
    {
        options.TokenLimit = 100;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 1000;
        options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        options.TokensPerPeriod = 20;
        options.AutoReplenishment = true;
    }));

var app = builder.Build();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error");

app.UseAuthorization();

app.MapControllers().RequireRateLimiting(tokenPolicy);

app.Run();
