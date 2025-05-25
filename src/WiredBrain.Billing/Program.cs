using MassTransit;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Prometheus;
using WiredBrain.Billing;
using WiredBrain.Billing.Models;
using WiredBrain.Billing.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<OrderPlacedConsumer>();
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/");
            
        cfg.ReceiveEndpoint("billing-service", e =>
        {
            e.Consumer<OrderPlacedConsumer>(context);
            
            // Use the default MassTransit concurrency settings based on CPU count
            int concurrentMessages = e.ConcurrentMessageLimit ?? e.PrefetchCount;
            BillingMetrics.TrackNumProcessors(concurrentMessages); // Track the number of concurrent messages that can be processed
            
            Console.WriteLine($"Billing service configured with {concurrentMessages} concurrent processors");
        });
    });
});

// Configure resilience options
builder.Services.Configure<ResilienceConfig>(
    builder.Configuration.GetSection("PaymentService:Resilience"));

// Add HTTP client for payment service with Polly policies
builder.Services.AddHttpClient("PaymentService", (serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<ResilienceConfig>>().Value;
    client.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"] ?? "http://simulated-payments:80/");
    client.Timeout = config.Timeout;
})
.AddPolicyHandler((serviceProvider, _) =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<PaymentServiceClient>>();
    var config = serviceProvider.GetRequiredService<IOptions<ResilienceConfig>>().Value;
    
    // Use the new DecorrelatedJitterBackoffV2 formula for smoother distribution of retry intervals
    var delay = Polly.Contrib.WaitAndRetry.Backoff.DecorrelatedJitterBackoffV2(
        medianFirstRetryDelay: TimeSpan.FromSeconds(config.InitialBackoffSeconds),
        retryCount: config.MaxRetryAttempts);
    
    return HttpPolicyExtensions
        .HandleTransientHttpError() // HttpRequestException, 5XX and 408 status codes
        .Or<TimeoutRejectedException>() // Handle timeout rejections
        .WaitAndRetryAsync(
            delay,
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                logger.LogWarning(
                    "Retry {RetryAttempt} after {TimespanSeconds}s delay due to {Message}",
                    retryAttempt,
                    timespan.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
            }
        );
})
.AddPolicyHandler((serviceProvider, _) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<ResilienceConfig>>().Value;
    return Policy.TimeoutAsync<HttpResponseMessage>(config.Timeout);
});

// Register services
builder.Services.AddSingleton<BillingRepository>();
builder.Services.AddScoped<PaymentServiceClient>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WiredBrain Billing API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WiredBrain Billing API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMetricServer(); // Exposes /metrics endpoint
app.UseHttpMetrics();  // Collects HTTP request metrics

app.Run();
