using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NexusLabs.AmpSharp.Client;
using NexusLabs.AmpSharp.Extensions;
using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    private const string ValidDeploymentKey = "test-deployment-key";

    #region AddAmplitudeExperiment Tests

    [Fact]
    public void AddAmplitudeExperiment_RegistersIRemoteEvaluationClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<RemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_RegistersAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAmplitudeExperiment(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Act
        var client1 = provider.GetRequiredService<IRemoteEvaluationClient>();
        var client2 = provider.GetRequiredService<IRemoteEvaluationClient>();

        // Assert
        Assert.Same(client1, client2);
    }

    [Fact]
    public void AddAmplitudeExperiment_ThrowsOnNullDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperiment(null!));
    }

    [Fact]
    public void AddAmplitudeExperiment_ThrowsOnEmptyDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperiment(""));
    }

    [Fact]
    public void AddAmplitudeExperiment_ThrowsOnWhitespaceDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperiment("   "));
    }

    [Fact]
    public void AddAmplitudeExperiment_AcceptsNullConfig()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey, config: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_AcceptsCustomConfig()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new RemoteEvaluationConfig
        {
            ServerZone = ServerZone.EU,
            FetchTimeoutMillis = 5000
        };

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey, config);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_ReturnsSameServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAmplitudeExperiment(ValidDeploymentKey);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddAmplitudeExperiment_WithAction_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configureActionCalled = false;

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey, config =>
        {
            configureActionCalled = true;
            config.ServerZone = ServerZone.EU;
        });
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.True(configureActionCalled);
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_WithAction_AcceptsNullAction()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey, configureOptions: null!);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_RegistersHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = provider.GetService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);
    }

    [Fact]
    public void AddAmplitudeExperiment_UsesLoggerFromDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAmplitudeExperiment(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Act - should not throw, logger is resolved from DI
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();

        // Assert
        Assert.NotNull(client);
    }

    #endregion

    #region AddAmplitudeExperimentWithCaching Tests

    [Fact]
    public void AddAmplitudeExperimentWithCaching_RegistersCachingClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_RegistersAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Act
        var client1 = provider.GetRequiredService<IRemoteEvaluationClient>();
        var client2 = provider.GetRequiredService<IRemoteEvaluationClient>();

        // Assert
        Assert.Same(client1, client2);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_ThrowsOnNullDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperimentWithCaching(null!));
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_ThrowsOnEmptyDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperimentWithCaching(""));
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_ThrowsOnWhitespaceDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperimentWithCaching("   "));
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_AcceptsNullConfig()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey, config: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_AcceptsNullCachingOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey, config: null, cachingOptions: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_AcceptsCustomConfig()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new RemoteEvaluationConfig
        {
            ServerZone = ServerZone.EU,
            FetchTimeoutMillis = 5000
        };

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey, config);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_AcceptsCustomCachingOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var cachingOptions = new CachingOptions
        {
            CacheKeyPrefix = "custom-prefix",
            AbsoluteExpiration = TimeSpan.FromMinutes(10)
        };

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey, cachingOptions: cachingOptions);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_ReturnsSameServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_RegistersHybridCache()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert
        var cache = provider.GetService<HybridCache>();
        Assert.NotNull(cache);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_WithActions_ConfiguresBothOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configActionCalled = false;
        var cachingActionCalled = false;

        // Act
        services.AddAmplitudeExperimentWithCaching(
            ValidDeploymentKey,
            configureOptions: config =>
            {
                configActionCalled = true;
                config.ServerZone = ServerZone.EU;
            },
            configureCaching: caching =>
            {
                cachingActionCalled = true;
                caching.CacheKeyPrefix = "test";
            });
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.True(configActionCalled);
        Assert.True(cachingActionCalled);
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_WithActions_AcceptsNullConfigAction()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperimentWithCaching(
            ValidDeploymentKey,
            configureOptions: null,
            configureCaching: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_WithActions_AcceptsNullCachingAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configActionCalled = false;

        // Act
        services.AddAmplitudeExperimentWithCaching(
            ValidDeploymentKey,
            configureOptions: config =>
            {
                configActionCalled = true;
            },
            configureCaching: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.True(configActionCalled);
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_UsesLoggerFromDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Act - should not throw, logger is resolved from DI
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_RegistersHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = provider.GetService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);
    }

    #endregion

    #region AddAmplitudeExperimentWithExistingCache Tests

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_UsesPreviouslyRegisteredHybridCache()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Simulate user registering their own HybridCache (like FusionCache.AsHybridCache())
        services.AddHybridCache();
        
        // Act
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_ThrowsIfHybridCacheNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Don't register HybridCache - simulate user forgetting to register it
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Act & Assert - should throw when trying to resolve because HybridCache isn't registered
        Assert.Throws<InvalidOperationException>(() =>
            provider.GetRequiredService<IRemoteEvaluationClient>());
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_ThrowsOnNullDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperimentWithExistingCache(null!));
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_ThrowsOnEmptyDeploymentKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddAmplitudeExperimentWithExistingCache(""));
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_AcceptsNullConfig()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();

        // Act
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey, config: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_AcceptsNullCachingOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();

        // Act
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey, config: null, cachingOptions: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_AcceptsCustomConfig()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();
        var config = new RemoteEvaluationConfig
        {
            ServerZone = ServerZone.EU,
            FetchTimeoutMillis = 5000
        };

        // Act
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey, config);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_AcceptsCustomCachingOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();
        var cachingOptions = new CachingOptions
        {
            CacheKeyPrefix = "custom-prefix",
            AbsoluteExpiration = TimeSpan.FromMinutes(10)
        };

        // Act
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey, cachingOptions: cachingOptions);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_ReturnsSameServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();

        // Act
        var result = services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_WithActions_ConfiguresBothOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();
        var configActionCalled = false;
        var cachingActionCalled = false;

        // Act
        services.AddAmplitudeExperimentWithExistingCache(
            ValidDeploymentKey,
            configureOptions: config =>
            {
                configActionCalled = true;
                config.ServerZone = ServerZone.EU;
            },
            configureCaching: caching =>
            {
                cachingActionCalled = true;
                caching.CacheKeyPrefix = "test";
            });
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.True(configActionCalled);
        Assert.True(cachingActionCalled);
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_WithActions_AcceptsNullActions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();

        // Act
        services.AddAmplitudeExperimentWithExistingCache(
            ValidDeploymentKey,
            configureOptions: null,
            configureCaching: null);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_RegistersAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Act
        var client1 = provider.GetRequiredService<IRemoteEvaluationClient>();
        var client2 = provider.GetRequiredService<IRemoteEvaluationClient>();

        // Assert
        Assert.Same(client1, client2);
    }

    [Fact]
    public void AddAmplitudeExperimentWithExistingCache_DoesNotRegisterAnotherHybridCache()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache();
        var initialHybridCacheDescriptors = services.Count(s => s.ServiceType == typeof(HybridCache));

        // Act
        services.AddAmplitudeExperimentWithExistingCache(ValidDeploymentKey);
        var finalHybridCacheDescriptors = services.Count(s => s.ServiceType == typeof(HybridCache));

        // Assert - should not have added any new HybridCache registrations
        Assert.Equal(initialHybridCacheDescriptors, finalHybridCacheDescriptors);
    }

    #endregion

    #region Configuration Integration Tests

    [Fact]
    public void AddAmplitudeExperiment_WithEUServerZone_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new RemoteEvaluationConfig { ServerZone = ServerZone.EU };

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey, config);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_WithCustomServerUrl_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new RemoteEvaluationConfig
        {
            ServerUrl = "https://custom.amplitude.com"
        };

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey, config);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_WithCustomTimeouts_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new RemoteEvaluationConfig
        {
            FetchTimeoutMillis = 30000,
            FetchRetries = 3,
            FetchRetryBackoffMinMillis = 1000,
            FetchRetryBackoffMaxMillis = 5000
        };

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey, config);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_WithCustomExpiration_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var cachingOptions = new CachingOptions
        {
            AbsoluteExpiration = TimeSpan.FromHours(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(5)
        };

        // Act
        services.AddAmplitudeExperimentWithCaching(
            ValidDeploymentKey,
            cachingOptions: cachingOptions);
        using var provider = services.BuildServiceProvider();

        // Assert
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    #endregion

    #region Multiple Registration Tests

    [Fact]
    public void AddAmplitudeExperiment_CalledTwice_LastRegistrationWins()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - register twice with different configs
        services.AddAmplitudeExperiment("first-key");
        services.AddAmplitudeExperiment("second-key");
        using var provider = services.BuildServiceProvider();

        // Assert - should resolve without throwing
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAmplitudeExperiment_ThenAddWithCaching_CachingWins()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperiment(ValidDeploymentKey);
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert - last registration should win
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();
        Assert.IsType<CachingRemoteEvaluationClient>(client);
    }

    [Fact]
    public void AddAmplitudeExperimentWithCaching_ThenAddWithoutCaching_NonCachingWins()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAmplitudeExperimentWithCaching(ValidDeploymentKey);
        services.AddAmplitudeExperiment(ValidDeploymentKey);
        using var provider = services.BuildServiceProvider();

        // Assert - last registration should win
        var client = provider.GetRequiredService<IRemoteEvaluationClient>();
        Assert.IsType<RemoteEvaluationClient>(client);
    }

    #endregion
}
