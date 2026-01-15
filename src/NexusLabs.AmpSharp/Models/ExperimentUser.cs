using System.Text.Json.Serialization;

namespace NexusLabs.AmpSharp.Models;

/// <summary>
/// Represents a user context for experiment evaluation.
/// All fields are optional.
/// </summary>
public sealed class ExperimentUser
{
    /// <summary>
    /// User identifier for Amplitude association.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Device identifier for Amplitude association.
    /// </summary>
    [JsonPropertyName("device_id")]
    public string? DeviceId { get; set; }

    /// <summary>
    /// User's country.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// User's city.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// User's region.
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// Designated Market Area.
    /// </summary>
    [JsonPropertyName("dma")]
    public string? DMA { get; set; }

    /// <summary>
    /// User's language.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Platform (e.g., "iOS", "Android", "Web").
    /// </summary>
    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    /// <summary>
    /// Application version.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Operating system.
    /// </summary>
    [JsonPropertyName("os")]
    public string? OS { get; set; }

    /// <summary>
    /// Device manufacturer.
    /// </summary>
    [JsonPropertyName("device_manufacturer")]
    public string? DeviceManufacturer { get; set; }

    /// <summary>
    /// Device brand.
    /// </summary>
    [JsonPropertyName("device_brand")]
    public string? DeviceBrand { get; set; }

    /// <summary>
    /// Device model.
    /// </summary>
    [JsonPropertyName("device_model")]
    public string? DeviceModel { get; set; }

    /// <summary>
    /// Network carrier.
    /// </summary>
    [JsonPropertyName("carrier")]
    public string? Carrier { get; set; }

    /// <summary>
    /// IP address.
    /// </summary>
    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// SDK library identifier (auto-populated by the client).
    /// </summary>
    [JsonPropertyName("library")]
    public string? Library { get; set; }

    /// <summary>
    /// Custom user properties (key-value pairs).
    /// Values can be strings, numbers, booleans, or arrays.
    /// </summary>
    [JsonPropertyName("user_properties")]
    public Dictionary<string, object>? UserProperties { get; set; }

    /// <summary>
    /// Group memberships (group type -> group names).
    /// </summary>
    [JsonPropertyName("groups")]
    public Dictionary<string, string[]>? Groups { get; set; }

    /// <summary>
    /// Properties for specific group instances.
    /// </summary>
    [JsonPropertyName("group_properties")]
    public Dictionary<string, Dictionary<string, object>>? GroupProperties { get; set; }

    /// <summary>
    /// Cohort IDs the user belongs to.
    /// </summary>
    [JsonPropertyName("cohort_ids")]
    public List<string>? CohortIds { get; set; }

    /// <summary>
    /// Cohort memberships within groups.
    /// </summary>
    [JsonPropertyName("group_cohort_ids")]
    public Dictionary<string, List<string>>? GroupCohortIds { get; set; }
}
