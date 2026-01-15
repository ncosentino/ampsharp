using NexusLabs.AmpSharp.Client;
using NexusLabs.AmpSharp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NexusLabs.AmpSharp.Extensions;

/// <summary>
/// Extension methods for configuring Amplitude Experiment in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Amplitude Experiment remote evaluation client to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="config">Optional configuration. If not provided, uses defaults.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAmplitudeExperiment(
        this IServiceCollection services,
        string deploymentKey,
        RemoteEvaluationConfig? config = null)
    {
        if (string.IsNullOrWhiteSpace(deploymentKey))
        {
            throw new ArgumentException("Deployment key cannot be null or empty", nameof(deploymentKey));
        }

        config ??= new RemoteEvaluationConfig();

        // Register HttpClient with IHttpClientFactory
        services.AddHttpClient<IRemoteEvaluationClient, RemoteEvaluationClient>(client =>
        {
            client.BaseAddress = new Uri(config.GetServerUrl());
            client.Timeout = TimeSpan.FromMilliseconds(config.FetchTimeoutMillis);
            client.DefaultRequestHeaders.Add("Authorization", $"Api-Key {deploymentKey}");
        });

        // Register RemoteEvaluationClient as singleton
        services.AddSingleton<IRemoteEvaluationClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(RemoteEvaluationClient));

            var logger = serviceProvider.GetService<ILogger<RemoteEvaluationClient>>();

            // Create a new config with the logger from DI
            var effectiveConfig = new RemoteEvaluationConfig
            {
                ServerZone = config.ServerZone,
                ServerUrl = config.ServerUrl,
                FetchTimeoutMillis = config.FetchTimeoutMillis,
                FetchRetries = config.FetchRetries,
                FetchRetryBackoffMinMillis = config.FetchRetryBackoffMinMillis,
                FetchRetryBackoffMaxMillis = config.FetchRetryBackoffMaxMillis,
                FetchRetryBackoffScalar = config.FetchRetryBackoffScalar,
                FetchRetryTimeoutMillis = config.FetchRetryTimeoutMillis,
                LogLevel = config.LogLevel,
                Logger = logger
            };

            return new RemoteEvaluationClient(httpClient, deploymentKey, effectiveConfig);
        });

        return services;
    }

    /// <summary>
    /// Adds Amplitude Experiment remote evaluation client to the service collection with a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="configureOptions">Action to configure the client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAmplitudeExperiment(
        this IServiceCollection services,
        string deploymentKey,
        Action<RemoteEvaluationConfig> configureOptions)
    {
        var config = new RemoteEvaluationConfig();
        configureOptions?.Invoke(config);

        return AddAmplitudeExperiment(services, deploymentKey, config);
    }
}
