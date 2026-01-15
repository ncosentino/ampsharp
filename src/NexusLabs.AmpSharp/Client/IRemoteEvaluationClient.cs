using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp.Client;

/// <summary>
/// Interface for remote evaluation of Amplitude Experiment flags.
/// </summary>
public interface IRemoteEvaluationClient
{
    /// <summary>
    /// Fetches variants for a user from Amplitude Experiment.
    /// </summary>
    /// <param name="user">The user context for evaluation.</param>
    /// <param name="options">Optional fetch options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary of flag keys to variants.</returns>
    Task<Dictionary<string, Variant>> FetchV2Async(
        ExperimentUser user,
        FetchOptions? options = null,
        CancellationToken cancellationToken = default);
}
