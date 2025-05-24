# Circuit Breaker Within a Consumer

Wire up a Polly circuit-breaker inside your consumer

```csharp
// a simple async circuit-breaker policy
AsyncCircuitBreakerPolicy breaker = Policy
  .Handle<PaymentTimeoutException>()
  .CircuitBreakerAsync(
    exceptionsAllowedBeforeBreaking: 3,
    durationOfBreak: TimeSpan.FromSeconds(30),
    onBreak:    (ex, ts) => OnCircuitOpen(),     // see next
    onReset:    ()         => OnCircuitClosed(),
    onHalfOpen: ()         => Log.LogInformation("CB half-open")
  );
```

Wrap your outbound call in the policy:

```csharp
public class OrderConsumer : IConsumer<Order>
{
    public async Task Consume(ConsumeContext<Order> ctx)
    {
        await breaker.ExecuteAsync(async () =>
        {
            // outbound call to payment service…
            await _paymentsClient.Charge(ctx.Message);
        });

        // Ack happens automatically if no exception
    }
}
```

What happens now:

1. **Normal operation**: messages are pulled, processed, then Acked.
2. **Downstream flaps**: after N failures, Polly trips → `OnCircuitOpen` fires.
3. **Subsequent requests fail**: Polly throws `BrokenCircuitException` on the next call.
4. **MassTransit Nacks**: the message is Nacked (or retried) and sent to the error queue.
5. **Circuit reset**: after the `durationOfBreak`, Polly calls `OnCircuitClosed`.
6. **Half-open**: Polly calls `OnHalfOpen` to test the downstream.
7. **Circuit reset**: once the downstream recovers (or half-open succeeds), Polly calls `OnCircuitClosed`.
8. **Normal operation**: messages are pulled, processed, then Acked.
9. **Shovel error queue into the main queue**: once the circuit is closed, you can shovel the error queue back into the main queue to re-process the requests.

## Next Steps

The circuit breaker prevents traffic from overwhelming the downstream service. However, on its own, it doesn't stop the incoming queue from being drained. To prevent the consumer from pulling more messages, you can use a **Kill Switch**. This is a MassTransit-specific feature that stops the consumer from pulling messages when the downstream service is unhealthy.
