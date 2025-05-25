using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class RetryPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> Factory(ILogger logger, ResilienceConfig config)
    {
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
    }
}
