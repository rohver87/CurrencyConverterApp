# CurrencyConverterApp
Using https://www.frankfurter.app/

Project Setup:

There are two projects in the solution - 1. CurrencyConverterApp and 2. CurrencyConverterApp.Tests

Running the application:

Select the CurrencyConverterApp as startup and run the project. The application will start listening to http://localhost:5019

Swagger endpoint: 

http://localhost:5019/swagger/index.html

Api Endpoints: 

1. http://localhost:5019/currencyconverter/exchange-rates?baseCurrency=usd

2. http://localhost:5019/currencyconverter/amount-conversions?amount=10&baseCurrency=usd&toCurrencies=inr

3. http://localhost:5019/currencyconverter/historic-rates?baseCurrency=usd&dateRange=2020-01-01..2020-06-28


Unit tests:

Implemented using xUnit and Moq 

Resiliency in HttpClient:

For handling transient errors, Retry policy is implemented using Polly. Retry policy along with Circuit Breaker policy is used as shown below 

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

Rate limiting:

Token bucket rate limiting policy is used to avoid sending too many requests in case of high load

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


Enhancements/Improvements:

1. We can cache the results for a date and reuse it instead of calling api.frankfurter.app
2. Fine tune the configurations used for Polly retry and Circuit breaker
3. Fine tune the configuration for rate limiting
4. Rate limiting used will work at api instance level. We can use the centralized rate limiting in future when multiple instances are running. 
This can be implemented using Redis INCR and EXPIRE commands on a counter
5. Add more unit test cases and also create integration tests which will call the api.frankfurter.app instead of mocking
 



