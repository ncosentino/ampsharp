using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexusLabs.AmpSharp.Models;

/// <summary>
/// Represents a variant result from an experiment evaluation.
/// </summary>
public sealed class Variant
{
    /// <summary>
    /// The variant key (e.g., "on", "off", "control", "treatment").
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// The variant value as a string.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// Optional payload attached to this variant.
    /// </summary>
    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    /// <summary>
    /// The experiment key associated with this variant.
    /// </summary>
    [JsonPropertyName("expKey")]
    public string? ExpKey { get; set; }

    /// <summary>
    /// Metadata produced as a result of evaluation for the user.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}
