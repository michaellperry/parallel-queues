using Polly;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class TimeoutPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> Factory(ILogger logger, ResilienceConfig config)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(config.Timeout);
    }
}
