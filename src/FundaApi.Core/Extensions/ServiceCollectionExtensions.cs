using ComposableAsync;
using FundaApi.Core.Contracts;
using FundaApi.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using RateLimiter;
using System;
using System.Net;

namespace FundaApi.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFundaApi(this IServiceCollection services, Action<FundaApiOptions> options)
    {
        services.AddOptions<FundaApiOptions>()
          .Configure(options)
          .Validate(o => !string.IsNullOrEmpty(o.ApiKey), "Api key must be provided.");

        services.AddHttpClient<IBrokerApi, BrokerApi>()
                .ConfigurePrimaryHttpMessageHandler(configureHandler =>
                {
                    return TimeLimiter
                            .GetFromMaxCountByInterval(100, TimeSpan.FromSeconds(60))
                            .AsDelegatingHandler();
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler((services, request) => HttpPolicyExtensions
                       .HandleTransientHttpError()
                       .OrResult(msg => msg.StatusCode == HttpStatusCode.Unauthorized)
                       .WaitAndRetryAsync(
                            retryCount: 3,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (retryAttempt, delay) =>
                            {
                                services.GetService<ILogger<BrokerApi>>()?.LogTrace("Delaying for {delay} seconds, then making retry.", delay.TotalSeconds);
                            })
                       );
    }
}
