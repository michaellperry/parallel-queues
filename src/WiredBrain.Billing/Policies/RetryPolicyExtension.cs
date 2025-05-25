using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using WiredBrain.Billing.Models;
using WiredBrain.Billing.Services;

namespace WiredBrain.Billing.Policies;

public static class RetryPolicyExtension
{
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder)
    {
        return builder.AddPolicyHandler((serviceProvider, _) =>
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
        });
    }
}
