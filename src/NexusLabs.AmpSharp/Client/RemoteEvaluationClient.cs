using NexusLabs.AmpSharp.Http;
using NexusLabs.AmpSharp.Logging;
using NexusLabs.AmpSharp.Models;
using Microsoft.Extensions.Logging;

namespace NexusLabs.AmpSharp.Client;

/// <summary>
/// Implementation of remote evaluation client for Amplitude Experiment.
/// </summary>
public sealed class RemoteEvaluationClient : IRemoteEvaluationClient
{
    private readonly ExperimentApiClient _apiClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteEvaluationClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="config">Configuration options.</param>
    public RemoteEvaluationClient(
        HttpClient httpClient,
        string deploymentKey,
        RemoteEvaluationConfig? config = null)
    {
        config ??= new RemoteEvaluationConfig();
        _logger = config.Logger ?? NullLogger.Instance;

        _apiClient = new ExperimentApiClient(httpClient, deploymentKey, config, _logger);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteEvaluationClient"/> class with explicit logger.
    /// </summary>
    internal RemoteEvaluationClient(
        HttpClient httpClient,
        string deploymentKey,
        RemoteEvaluationConfig config,
        ILogger logger)
    {
        _logger = logger;
        _apiClient = new ExperimentApiClient(httpClient, deploymentKey, config, _logger);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, Variant>> FetchV2Async(
        ExperimentUser user,
        FetchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        // Inject library context
        user.Library ??= "NexusLabs.AmpSharp/0.1.0";

        _logger.LogDebug("Fetching variants for user {UserId}/{DeviceId}", user.UserId, user.DeviceId);

        try
        {
            var variants = await _apiClient.FetchVariantsAsync(user, options, cancellationToken);
            _logger.LogDebug("Successfully fetched {Count} variants", variants.Count);
            return variants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch variants");
            throw;
        }
    }
}
