using Microsoft.Extensions.Options;
using Polly;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class TimeoutPolicyExtension
{
    public static IHttpClientBuilder AddTimeoutPolicy(this IHttpClientBuilder builder)
    {
        return builder.AddPolicyHandler((serviceProvider, _) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<ResilienceConfig>>().Value;
            return Policy.TimeoutAsync<HttpResponseMessage>(config.Timeout);
        });
    }
}
