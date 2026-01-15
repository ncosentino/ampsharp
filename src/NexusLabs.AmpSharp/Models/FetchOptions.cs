namespace NexusLabs.AmpSharp.Models;

/// <summary>
/// Options for fetching variants from Amplitude Experiment.
/// </summary>
public sealed class FetchOptions
{
    /// <summary>
    /// Specific flag keys to fetch. If null or empty, fetches all flags.
    /// </summary>
    public List<string>? FlagKeys { get; set; }

    /// <summary>
    /// Whether to track exposure events. Default is true.
    /// </summary>
    public bool TracksExposure { get; set; } = true;

    /// <summary>
    /// Whether to track assignment events. Default is true.
    /// </summary>
    public bool TracksAssignment { get; set; } = true;
}
