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
                    // Track retry metrics
                    ResilienceMetrics.TrackRetryAttempt();
                    ResilienceMetrics.TrackRetryDelay(timespan);
                    
                    // Track the failure that triggered the retry
                    if (outcome.Exception is HttpRequestException httpEx)
                    {
                        ResilienceMetrics.TrackRequestFailure(httpEx, httpEx.StatusCode);
                    }
                    else if (outcome.Exception is TimeoutRejectedException)
                    {
                        ResilienceMetrics.TrackTimeout();
                        ResilienceMetrics.TrackRequestFailure(outcome.Exception);
                    }
                    else if (outcome.Exception != null)
                    {
                        ResilienceMetrics.TrackRequestFailure(outcome.Exception);
                    }
                    else if (outcome.Result != null)
                    {
                        ResilienceMetrics.TrackRequestFailure(
                            statusCode: outcome.Result.StatusCode);
                    }
                    
                    logger.LogWarning(
                        "Retry {RetryAttempt}/{MaxRetryAttempts} after {TimespanSeconds}s delay due to {Message}",
                        retryAttempt,
                        config.MaxRetryAttempts,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                }
            );
    }
}
