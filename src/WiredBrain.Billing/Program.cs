using MassTransit;
using Microsoft.Extensions.Options;
using Polly;
using Prometheus;
using WiredBrain.Billing;
using WiredBrain.Billing.Models;
using WiredBrain.Billing.Policies;
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

// Register resilience policies
builder.Services.AddSingleton<ResiliencePolicyRegistry>();

// Add HTTP client for payment service with Polly policies
builder.Services.AddHttpClient("PaymentService", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"] ?? "http://simulated-payments:80/");
})
.AddPolicyHandler((serviceProvider, _) =>
{
    var registry = serviceProvider.GetRequiredService<ResiliencePolicyRegistry>().Registry;
    return registry.Get<IAsyncPolicy<HttpResponseMessage>>("PaymentService.PolicyWrap");
});

// Register services
builder.Services.AddSingleton<BillingRepository>();
builder.Services.AddSingleton<PaymentServiceClient>();

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
