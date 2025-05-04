﻿using MassTransit;
using Microsoft.OpenApi.Models;
using Prometheus;
using WiredBrain.Messages;
using WiredBrain.Ordering;
using WiredBrain.Ordering.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WiredBrain.Ordering API", Version = "v1" });
});

// Register the configuration service as a singleton
builder.Services.AddSingleton<QueueConfigurationService>();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

// Configure metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();

// Map controllers
app.MapControllers();

// Start the order publishing background service
Task.Run(async () => await PublishOrdersAsync());

app.Run();

async Task PublishOrdersAsync()
{
    // Get the bus from the service provider
    var busControl = app.Services.GetRequiredService<IBus>();
    var configService = app.Services.GetRequiredService<QueueConfigurationService>();

    try
    {
        Console.WriteLine("Ordering Service Started. Publishing orders...");
        
        // Publish orders at the configured rate
        while (true)
        {
            // Start a stopwatch to measure the time taken for order placement
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Get the current configuration and randomized delays
            var config = configService.GetConfiguration();
            var randomizedProcessingDelay = configService.GetRandomizedBillingProcessingDelay();
            var order = GenerateRandomOrder(randomizedProcessingDelay);
            
            await busControl.Publish<OrderPlaced>(order);
            
            // Track the order placement in metrics
            OrderingMetrics.TrackOrderPlaced();
            
            // Calculate expected wait time using Kingman's formula
            var expectedWaitTime = configService.CalculateExpectedWaitTime();
            var expectedWaitTimeStr = double.IsPositiveInfinity(expectedWaitTime) ? "∞" : $"{expectedWaitTime:F2}ms";
            
            Console.WriteLine($"Published order: {order.OrderId} for {order.CustomerName} - ${order.Amount} " +
                             $"(Base Delay: {config.OrderArrivalDelayMs}ms, Processing: {randomizedProcessingDelay}ms, " +
                             $"Expected Wait: {expectedWaitTimeStr})");

            // Stop the stopwatch and calculate the elapsed time
            stopwatch.Stop();
            var elapsedTime = stopwatch.ElapsedMilliseconds;

            // Calculate the remaining time to wait before publishing the next order
            // Use the randomized arrival delay based on the coefficient of arrival variation
            var randomizedArrivalDelay = configService.GetRandomizedOrderArrivalDelay();
            var waitTime = randomizedArrivalDelay - elapsedTime;
            if (waitTime > 0)
            {
                await Task.Delay((int)waitTime);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error publishing orders: {ex.Message}");
    }
}

OrderPlaced GenerateRandomOrder(int billingProcessingDelayMs)
{
    var random = new Random();
    var customers = new[] { "John", "Jane", "Bob", "Alice", "Charlie" };
    var configService = app.Services.GetRequiredService<QueueConfigurationService>();
    var config = configService.GetConfiguration();
    
    return new OrderPlaced
    {
        OrderId = Guid.NewGuid(),
        CustomerName = customers[random.Next(customers.Length)],
        OrderDate = DateTime.UtcNow,
        Amount = (decimal)Math.Round(random.NextSingle() * 100, 2),
        BillingProcessingDelayMs = billingProcessingDelayMs,
        CoefficientOfServiceVariation = config.CoefficientOfServiceVariation
    };
}
