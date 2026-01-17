using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

using NexusLabs.AmpSharp.Logging;
using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp.Client;

/// <summary>
/// A caching decorator for <see cref="IRemoteEvaluationClient"/> that uses <see cref="HybridCache"/>.
/// Provides stampede protection via HybridCache's GetOrCreateAsync.
/// </summary>
public sealed class CachingRemoteEvaluationClient : IRemoteEvaluationClient
{
    private readonly IRemoteEvaluationClient _innerClient;
    private readonly HybridCache _cache;
    private readonly CachingOptions _options;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingRemoteEvaluationClient"/> class.
    /// </summary>
    /// <param name="innerClient">The inner client to decorate.</param>
    /// <param name="cache">The hybrid cache implementation.</param>
    /// <param name="options">Optional caching options. If not provided, uses defaults.</param>
    /// <param name="logger">Optional logger.</param>
    public CachingRemoteEvaluationClient(
        IRemoteEvaluationClient innerClient,
        HybridCache cache,
        CachingOptions? options = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(innerClient);
        ArgumentNullException.ThrowIfNull(cache);

        _innerClient = innerClient;
        _cache = cache;
        _options = options ?? new CachingOptions();
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, Variant>> FetchV2Async(
        ExperimentUser user,
        FetchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var cacheKey = GenerateCacheKey(user, options);

        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = _options.AbsoluteExpiration,
            LocalCacheExpiration = _options.LocalCacheExpiration
        };

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async cancel =>
            {
                _logger.LogDebug("Cache miss for user {UserId}/{DeviceId}, fetching from API", user.UserId, user.DeviceId);
                return await _innerClient.FetchV2Async(user, options, cancel);
            },
            entryOptions,
            cancellationToken: cancellationToken);
    }

    private string GenerateCacheKey(ExperimentUser user, FetchOptions? options)
    {
        var keyParts = new List<string> { _options.CacheKeyPrefix };

        if (!string.IsNullOrEmpty(user.UserId))
        {
            keyParts.Add($"u:{user.UserId}");
        }

        if (!string.IsNullOrEmpty(user.DeviceId))
        {
            keyParts.Add($"d:{user.DeviceId}");
        }

        if (options?.FlagKeys is { Count: > 0 })
        {
            keyParts.Add($"f:{string.Join(",", options.FlagKeys.OrderBy(k => k))}");
        }

        return string.Join(":", keyParts);
    }
}
