namespace NexusLabs.AmpSharp.Client;

/// <summary>
/// Configuration options for caching in <see cref="CachingRemoteEvaluationClient"/>.
/// </summary>
public sealed class CachingOptions
{
    /// <summary>
    /// Gets or sets the cache key prefix. Default is "ampsharp".
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "ampsharp";

    /// <summary>
    /// Gets or sets the absolute expiration for both local and distributed cache. Default is 5 minutes.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the local (in-process) cache expiration. 
    /// If null, uses the same value as <see cref="AbsoluteExpiration"/>.
    /// </summary>
    public TimeSpan? LocalCacheExpiration { get; set; }
}
