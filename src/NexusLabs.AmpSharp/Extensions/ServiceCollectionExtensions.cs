using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NexusLabs.AmpSharp.Client;
using NexusLabs.AmpSharp.Models;

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
        ValidateDeploymentKey(deploymentKey);
        config ??= new RemoteEvaluationConfig();

        RegisterHttpClient(services, deploymentKey, config);

        services.AddSingleton<IRemoteEvaluationClient>(sp =>
            CreateRemoteEvaluationClient(sp, deploymentKey, config));

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

    /// <summary>
    /// Adds Amplitude Experiment remote evaluation client with caching to the service collection.
    /// Registers the default HybridCache with in-memory caching.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="config">Optional remote evaluation configuration.</param>
    /// <param name="cachingOptions">Optional caching options. If not provided, uses defaults.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method registers the default in-memory HybridCache. If you have already registered
    /// a HybridCache (e.g., via FusionCache's <c>.AsHybridCache()</c>), use 
    /// <see cref="AddAmplitudeExperimentWithExistingCache(IServiceCollection, string, RemoteEvaluationConfig?, CachingOptions?)"/> instead.
    /// </remarks>
    public static IServiceCollection AddAmplitudeExperimentWithCaching(
        this IServiceCollection services,
        string deploymentKey,
        RemoteEvaluationConfig? config = null,
        CachingOptions? cachingOptions = null)
    {
        services.AddHybridCache();
        return AddAmplitudeExperimentWithCachingCore(services, deploymentKey, config, cachingOptions);
    }

    /// <summary>
    /// Adds Amplitude Experiment remote evaluation client with caching using configuration actions.
    /// Registers the default HybridCache with in-memory caching.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="configureOptions">Action to configure the remote evaluation options.</param>
    /// <param name="configureCaching">Action to configure the caching options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAmplitudeExperimentWithCaching(
        this IServiceCollection services,
        string deploymentKey,
        Action<RemoteEvaluationConfig>? configureOptions,
        Action<CachingOptions>? configureCaching = null)
    {
        var config = new RemoteEvaluationConfig();
        configureOptions?.Invoke(config);

        var cachingOptions = new CachingOptions();
        configureCaching?.Invoke(cachingOptions);

        services.AddHybridCache();
        return AddAmplitudeExperimentWithCachingCore(services, deploymentKey, config, cachingOptions);
    }

    /// <summary>
    /// Adds Amplitude Experiment remote evaluation client with caching, using an existing HybridCache.
    /// Does NOT register HybridCache - assumes it is already registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="config">Optional remote evaluation configuration.</param>
    /// <param name="cachingOptions">Optional caching options. If not provided, uses defaults.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Use this method when you have already registered HybridCache, such as with FusionCache:
    /// <code>
    /// services.AddFusionCache()
    ///     .TryWithAutoSetup()
    ///     .AsHybridCache();
    /// 
    /// services.AddAmplitudeExperimentWithExistingCache("your-deployment-key");
    /// </code>
    /// </remarks>
    public static IServiceCollection AddAmplitudeExperimentWithExistingCache(
        this IServiceCollection services,
        string deploymentKey,
        RemoteEvaluationConfig? config = null,
        CachingOptions? cachingOptions = null)
    {
        return AddAmplitudeExperimentWithCachingCore(services, deploymentKey, config, cachingOptions);
    }

    /// <summary>
    /// Adds Amplitude Experiment remote evaluation client with caching using configuration actions,
    /// using an existing HybridCache. Does NOT register HybridCache - assumes it is already registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="configureOptions">Action to configure the remote evaluation options.</param>
    /// <param name="configureCaching">Action to configure the caching options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAmplitudeExperimentWithExistingCache(
        this IServiceCollection services,
        string deploymentKey,
        Action<RemoteEvaluationConfig>? configureOptions,
        Action<CachingOptions>? configureCaching = null)
    {
        var config = new RemoteEvaluationConfig();
        configureOptions?.Invoke(config);

        var cachingOptions = new CachingOptions();
        configureCaching?.Invoke(cachingOptions);

        return AddAmplitudeExperimentWithCachingCore(services, deploymentKey, config, cachingOptions);
    }

    private static IServiceCollection AddAmplitudeExperimentWithCachingCore(
        IServiceCollection services,
        string deploymentKey,
        RemoteEvaluationConfig? config,
        CachingOptions? cachingOptions)
    {
        ValidateDeploymentKey(deploymentKey);
        config ??= new RemoteEvaluationConfig();
        cachingOptions ??= new CachingOptions();

        RegisterHttpClient(services, deploymentKey, config);

        services.AddSingleton<IRemoteEvaluationClient>(sp =>
        {
            var innerClient = CreateRemoteEvaluationClient(sp, deploymentKey, config);
            var cache = sp.GetRequiredService<HybridCache>();
            var cachingLogger = sp.GetService<ILogger<CachingRemoteEvaluationClient>>();
            return new CachingRemoteEvaluationClient(innerClient, cache, cachingOptions, cachingLogger);
        });

        return services;
    }

    private static void ValidateDeploymentKey(string deploymentKey)
    {
        if (string.IsNullOrWhiteSpace(deploymentKey))
        {
            throw new ArgumentException("Deployment key cannot be null or empty", nameof(deploymentKey));
        }
    }

    private static void RegisterHttpClient(
        IServiceCollection services,
        string deploymentKey,
        RemoteEvaluationConfig config)
    {
        services.AddHttpClient<IRemoteEvaluationClient, RemoteEvaluationClient>(client =>
        {
            client.BaseAddress = new Uri(config.GetServerUrl());
            client.Timeout = TimeSpan.FromMilliseconds(config.FetchTimeoutMillis);
            client.DefaultRequestHeaders.Add("Authorization", $"Api-Key {deploymentKey}");
        });
    }

    private static RemoteEvaluationClient CreateRemoteEvaluationClient(
        IServiceProvider serviceProvider,
        string deploymentKey,
        RemoteEvaluationConfig config)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(RemoteEvaluationClient));
        var logger = serviceProvider.GetService<ILogger<RemoteEvaluationClient>>();

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
    }
}
