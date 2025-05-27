# Kill Switch Implementation Plan

MassTransit already provides this “stop consuming when downstream is unhealthy” behavior out-of-the-box in the form of its **Kill Switch** middleware. Under the covers, a Kill Switch is essentially the messaging-centric circuit breaker: when failures exceed your threshold, it pauses the receive endpoint for you (back-pressure!), then automatically restarts it after a timeout. No manual `StopAsync`/`StartAsync` needed.

---

## Use the Kill Switch

### 1. Configure on a single endpoint

```csharp
cfg.ReceiveEndpoint("billing-orders", e =>
{
    // only track exceptions after at least 10 messages,
    // and trip if >15% of those failed
    e.UseKillSwitch(k => k
        .SetActivationThreshold(10)           // track at least 10 calls
        .SetTripThreshold(0.15)              // 15% failure rate
        .SetRestartTimeout(TimeSpan.FromSeconds(30))
        .SetTrackingPeriod(TimeSpan.FromMinutes(1))
        // optionally filter which exceptions count:
        //.SetExceptionFilter(exc => exc is PaymentTimeoutException)
    );

    e.Consumer<OrderConsumer>();
});
```

### 2. Or apply globally to every endpoint

```csharp
services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // this will apply to *all* receive endpoints
        cfg.UseKillSwitch(k => k
            .SetActivationThreshold(10)
            .SetTripThreshold(0.15)
            .SetRestartTimeout(TimeSpan.FromSeconds(30))
            .SetTrackingPeriod(TimeSpan.FromMinutes(1)));
        cfg.ConfigureEndpoints(context);
    });
});
```

Once the Kill Switch trips, MassTransit **automatically** stops pulling messages from RabbitMQ (or your transport) and then **restarts** the endpoint after your `RestartTimeout`. This means:

1. Bad downstream → you stop consuming (back-pressure at the broker).
2. Broker queues up requests instead of your service.
3. After your interval, MassTransit resumes the pump and checks if the downstream has recovered.

No manual endpoint control is needed—MassTransit handles pause/resume for you.

---

### Why Kill Switch over Circuit Breaker here?

* **`UseCircuitBreaker`** (Polly-based) only trips calls *inside* your consumer and then throws `BrokenCircuitException`. MassTransit will then *fault* the message (Nack → retry or error-queue), which doesn’t give true back-pressure.
* **`UseKillSwitch`** halts the receive pump itself, so your client never even pulls the work off the queue until the downstream is healthy again ([MassTransit][1]).

---

**Recommendation:** Tweak `ActivationThreshold`, `TripThreshold`, and `RestartTimeout` to fit your load, and you’ll get reliable back-pressure.

[1]: https://masstransit.io/documentation/configuration/middleware/filters?utm_source=chatgpt.com "Middleware Filters - MassTransit"
