using Microsoft.Extensions.Options;
using Polly;
using WiredBrain.Billing.Models;

namespace WiredBrain.Billing.Policies;

public static class PolicyExtensions
{
    public static IHttpClientBuilder AddPolicies(this IHttpClientBuilder builder, params Func<ILogger, ResilienceConfig, IAsyncPolicy<HttpResponseMessage>>[] policyFunctions)
    {
        if (policyFunctions.Length == 0)
        {
            return builder;
        }
        return builder.AddPolicyHandler((serviceProvider, _) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                var resilienceConfig = serviceProvider.GetRequiredService<IOptions<ResilienceConfig>>().Value;
                var policies = policyFunctions.Select(func => func(logger, resilienceConfig)).ToArray();
                if (policies.Length == 1)
                {
                    return policies[0];
                }
                var wrapper = Policy.WrapAsync(policies);
                return wrapper;
            });
    }
    
    public static IHttpClientBuilder AddPolicyHandlerFromRegistry(this IHttpClientBuilder builder, string policyName)
    {
        return builder.AddPolicyHandler((serviceProvider, _) =>
        {
            var registry = serviceProvider.GetRequiredService<ResiliencePolicyRegistry>().Registry;
            return registry.Get<IAsyncPolicy<HttpResponseMessage>>(policyName);
        });
    }
}
