using NexusLabs.AmpSharp.Client;
using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp;

/// <summary>
/// Static factory for creating Amplitude Experiment clients.
/// </summary>
public static class Experiment
{
    /// <summary>
    /// Initializes a remote evaluation client for Amplitude Experiment.
    /// </summary>
    /// <param name="deploymentKey">The Amplitude deployment key.</param>
    /// <param name="config">Optional configuration. If not provided, uses defaults.</param>
    /// <returns>A configured remote evaluation client.</returns>
    public static IRemoteEvaluationClient InitializeRemote(
        string deploymentKey,
        RemoteEvaluationConfig? config = null)
    {
        if (string.IsNullOrWhiteSpace(deploymentKey))
        {
            throw new ArgumentException("Deployment key cannot be null or empty", nameof(deploymentKey));
        }

        config ??= new RemoteEvaluationConfig();

        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(config.FetchTimeoutMillis)
        };

        return new RemoteEvaluationClient(httpClient, deploymentKey, config);
    }
}
