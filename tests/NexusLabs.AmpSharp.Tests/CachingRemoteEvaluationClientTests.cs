using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NexusLabs.AmpSharp.Client;
using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp.Tests;

public sealed class CachingRemoteEvaluationClientTests : IDisposable
{
    private readonly Mock<IRemoteEvaluationClient> _innerClientMock;
    private readonly Mock<ILogger<CachingRemoteEvaluationClient>> _loggerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly HybridCache _cache;

    public CachingRemoteEvaluationClientTests()
    {
        _innerClientMock = new Mock<IRemoteEvaluationClient>();
        _loggerMock = new Mock<ILogger<CachingRemoteEvaluationClient>>();

        var services = new ServiceCollection();
        services.AddHybridCache();
        _serviceProvider = services.BuildServiceProvider();
        _cache = _serviceProvider.GetRequiredService<HybridCache>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    [Fact]
    public void Constructor_ThrowsOnNullInnerClient()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CachingRemoteEvaluationClient(null!, _cache));
    }

    [Fact]
    public void Constructor_ThrowsOnNullCache()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CachingRemoteEvaluationClient(_innerClientMock.Object, null!));
    }

    [Fact]
    public void Constructor_AcceptsNullOptions()
    {
        // Act
        var client = new CachingRemoteEvaluationClient(
            _innerClientMock.Object,
            _cache,
            options: null);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_AcceptsNullLogger()
    {
        // Act
        var client = new CachingRemoteEvaluationClient(
            _innerClientMock.Object,
            _cache,
            options: null,
            logger: null);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public async Task FetchV2Async_ThrowsOnNullUser()
    {
        // Arrange
        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.FetchV2Async(null!));
    }

    [Fact]
    public async Task FetchV2Async_ReturnsVariantsFromInnerClient()
    {
        // Arrange
        var client = CreateClient();
        var user = new ExperimentUser { UserId = "user-123" };
        var expectedVariants = new Dictionary<string, Variant>
        {
            ["experiment-1"] = new Variant { Key = "treatment", Value = "variant-a" },
            ["experiment-2"] = new Variant { Key = "control", Value = "default" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVariants);

        // Act
        var result = await client.FetchV2Async(user);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("treatment", result["experiment-1"].Key);
        Assert.Equal("control", result["experiment-2"].Key);
    }

    [Fact]
    public async Task FetchV2Async_CachesResultOnSubsequentCalls()
    {
        // Arrange
        var client = CreateClient();
        var user = new ExperimentUser { UserId = $"cache-test-{Guid.NewGuid()}" };
        var expectedVariants = new Dictionary<string, Variant>
        {
            ["flag-1"] = new Variant { Key = "on", Value = "true" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVariants);

        // Act - call twice
        var result1 = await client.FetchV2Async(user);
        var result2 = await client.FetchV2Async(user);

        // Assert - inner client should only be called once due to caching
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(result1["flag-1"].Key, result2["flag-1"].Key);
    }

    [Fact]
    public async Task FetchV2Async_DifferentUsersCallInnerClientSeparately()
    {
        // Arrange
        var client = CreateClient();
        var user1 = new ExperimentUser { UserId = $"user-1-{Guid.NewGuid()}" };
        var user2 = new ExperimentUser { UserId = $"user-2-{Guid.NewGuid()}" };
        var variants1 = new Dictionary<string, Variant>
        {
            ["flag-1"] = new Variant { Key = "on" }
        };
        var variants2 = new Dictionary<string, Variant>
        {
            ["flag-1"] = new Variant { Key = "off" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user1, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(variants1);
        _innerClientMock
            .Setup(c => c.FetchV2Async(user2, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(variants2);

        // Act
        var result1 = await client.FetchV2Async(user1);
        var result2 = await client.FetchV2Async(user2);

        // Assert - both users should have their own cache entries
        _innerClientMock.Verify(
            c => c.FetchV2Async(user1, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _innerClientMock.Verify(
            c => c.FetchV2Async(user2, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal("on", result1["flag-1"].Key);
        Assert.Equal("off", result2["flag-1"].Key);
    }

    [Fact]
    public async Task FetchV2Async_SameUserDifferentFlagKeysAreCachedSeparately()
    {
        // Arrange
        var userId = $"user-{Guid.NewGuid()}";
        var client = CreateClient();
        var user = new ExperimentUser { UserId = userId };
        var options1 = new FetchOptions { FlagKeys = ["flag-a"] };
        var options2 = new FetchOptions { FlagKeys = ["flag-b"] };

        var variants1 = new Dictionary<string, Variant>
        {
            ["flag-a"] = new Variant { Key = "variant-a" }
        };
        var variants2 = new Dictionary<string, Variant>
        {
            ["flag-b"] = new Variant { Key = "variant-b" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, options1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(variants1);
        _innerClientMock
            .Setup(c => c.FetchV2Async(user, options2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(variants2);

        // Act
        var result1 = await client.FetchV2Async(user, options1);
        var result2 = await client.FetchV2Async(user, options2);

        // Assert - different flag keys should result in separate cache entries
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, options1, It.IsAny<CancellationToken>()),
            Times.Once);
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, options2, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchV2Async_PassesFetchOptionsToInnerClient()
    {
        // Arrange
        var client = CreateClient();
        var user = new ExperimentUser { UserId = $"user-{Guid.NewGuid()}" };
        var options = new FetchOptions
        {
            FlagKeys = ["flag-1", "flag-2"],
            TracksExposure = false
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Variant>());

        // Act
        await client.FetchV2Async(user, options);

        // Assert
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, options, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchV2Async_PassesCancellationTokenToInnerClient()
    {
        // Arrange
        var client = CreateClient();
        var user = new ExperimentUser { UserId = $"user-{Guid.NewGuid()}" };
        using var cts = new CancellationTokenSource();

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Variant>());

        // Act
        await client.FetchV2Async(user, cancellationToken: cts.Token);

        // Assert - verify the factory was called (which receives the token)
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchV2Async_UserWithDeviceIdOnlyCachesCorrectly()
    {
        // Arrange
        var client = CreateClient();
        var user = new ExperimentUser { DeviceId = $"device-{Guid.NewGuid()}" };
        var expectedVariants = new Dictionary<string, Variant>
        {
            ["flag-1"] = new Variant { Key = "on" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVariants);

        // Act - call twice
        await client.FetchV2Async(user);
        await client.FetchV2Async(user);

        // Assert - should cache based on device ID
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchV2Async_UserWithBothIdsGeneratesUniqueCacheKey()
    {
        // Arrange
        var client = CreateClient();
        var userBoth = new ExperimentUser
        {
            UserId = $"user-{Guid.NewGuid()}",
            DeviceId = $"device-{Guid.NewGuid()}"
        };
        var userOnlyUserId = new ExperimentUser
        {
            UserId = userBoth.UserId
        };

        var variantsBoth = new Dictionary<string, Variant>
        {
            ["flag"] = new Variant { Key = "both" }
        };
        var variantsUserOnly = new Dictionary<string, Variant>
        {
            ["flag"] = new Variant { Key = "user-only" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(userBoth, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(variantsBoth);
        _innerClientMock
            .Setup(c => c.FetchV2Async(userOnlyUserId, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(variantsUserOnly);

        // Act
        var resultBoth = await client.FetchV2Async(userBoth);
        var resultUserOnly = await client.FetchV2Async(userOnlyUserId);

        // Assert - different cache keys should be used
        _innerClientMock.Verify(
            c => c.FetchV2Async(userBoth, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _innerClientMock.Verify(
            c => c.FetchV2Async(userOnlyUserId, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal("both", resultBoth["flag"].Key);
        Assert.Equal("user-only", resultUserOnly["flag"].Key);
    }

    [Fact]
    public async Task FetchV2Async_FlagKeysAreSortedForConsistentCacheKey()
    {
        // Arrange
        var client = CreateClient();
        var userId = $"user-{Guid.NewGuid()}";
        var user = new ExperimentUser { UserId = userId };
        var options1 = new FetchOptions { FlagKeys = ["z-flag", "a-flag", "m-flag"] };
        var options2 = new FetchOptions { FlagKeys = ["a-flag", "m-flag", "z-flag"] };

        var expectedVariants = new Dictionary<string, Variant>
        {
            ["flag"] = new Variant { Key = "value" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVariants);

        // Act - same flags in different order should hit the same cache
        await client.FetchV2Async(user, options1);
        await client.FetchV2Async(user, options2);

        // Assert - inner client should only be called once because flag keys are sorted
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchV2Async_EmptyFlagKeysBehavesLikeNoFlagKeys()
    {
        // Arrange
        var client = CreateClient();
        var userId = $"user-{Guid.NewGuid()}";
        var user = new ExperimentUser { UserId = userId };
        var optionsEmpty = new FetchOptions { FlagKeys = [] };

        var expectedVariants = new Dictionary<string, Variant>
        {
            ["flag"] = new Variant { Key = "value" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVariants);

        // Act
        await client.FetchV2Async(user, optionsEmpty);
        await client.FetchV2Async(user, options: null);

        // Assert - both should use the same cache key
        _innerClientMock.Verify(
            c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchV2Async_InnerClientExceptionBubbles()
    {
        // Arrange
        var client = CreateClient();
        var user = new ExperimentUser { UserId = $"user-{Guid.NewGuid()}" };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.FetchV2Async(user));
    }

    [Fact]
    public async Task FetchV2Async_CustomCacheKeyPrefixIsUsed()
    {
        // Arrange
        var options = new CachingOptions { CacheKeyPrefix = "custom-prefix" };
        var client = CreateClient(options);
        var user1 = new ExperimentUser { UserId = $"user-{Guid.NewGuid()}" };

        var variants = new Dictionary<string, Variant>
        {
            ["flag"] = new Variant { Key = "value" }
        };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user1, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(variants);

        // Act
        var result = await client.FetchV2Async(user1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value", result["flag"].Key);
    }

    [Fact]
    public async Task FetchV2Async_ReturnsEmptyDictionaryWhenInnerClientReturnsEmpty()
    {
        // Arrange
        var client = CreateClient();
        var user = new ExperimentUser { UserId = $"user-{Guid.NewGuid()}" };

        _innerClientMock
            .Setup(c => c.FetchV2Async(user, It.IsAny<FetchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Variant>());

        // Act
        var result = await client.FetchV2Async(user);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private CachingRemoteEvaluationClient CreateClient(CachingOptions? options = null)
    {
        return new CachingRemoteEvaluationClient(
            _innerClientMock.Object,
            _cache,
            options,
            _loggerMock.Object);
    }
}
