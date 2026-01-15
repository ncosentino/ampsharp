using System.Text.Json;
using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp.Tests.Models;

public class VariantTests
{
    [Fact]
    public void DeserializesVariantCorrectly()
    {
        // Arrange
        var json = """
        {
            "key": "on",
            "value": "treatment",
            "payload": {"color": "blue"},
            "expKey": "exp-123",
            "metadata": {"default": false}
        }
        """;

        // Act
        var variant = JsonSerializer.Deserialize<Variant>(json);

        // Assert
        Assert.NotNull(variant);
        Assert.Equal("on", variant.Key);
        Assert.Equal("treatment", variant.Value);
        Assert.NotNull(variant.Payload);
        Assert.Equal("exp-123", variant.ExpKey);
        Assert.NotNull(variant.Metadata);
    }

    [Fact]
    public void DeserializesVariantWithNullFields()
    {
        // Arrange
        var json = """
        {
            "key": "off"
        }
        """;

        // Act
        var variant = JsonSerializer.Deserialize<Variant>(json);

        // Assert
        Assert.NotNull(variant);
        Assert.Equal("off", variant.Key);
        Assert.Null(variant.Value);
        Assert.Null(variant.Payload);
        Assert.Null(variant.ExpKey);
        Assert.Null(variant.Metadata);
    }

    [Fact]
    public void DeserializesVariantDictionary()
    {
        // Arrange
        var json = """
        {
            "flag-1": {"key": "on", "value": "treatment"},
            "flag-2": {"key": "off"}
        }
        """;

        // Act
        var variants = JsonSerializer.Deserialize<Dictionary<string, Variant>>(json);

        // Assert
        Assert.NotNull(variants);
        Assert.Equal(2, variants.Count);
        Assert.Equal("on", variants["flag-1"].Key);
        Assert.Equal("treatment", variants["flag-1"].Value);
        Assert.Equal("off", variants["flag-2"].Key);
    }
}
