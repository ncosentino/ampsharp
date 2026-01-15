using System.Text.Json;
using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp.Tests.Models;

public class ExperimentUserTests
{
    [Fact]
    public void SerializesToSnakeCase()
    {
        // Arrange
        var user = new ExperimentUser
        {
            UserId = "test-user",
            DeviceId = "test-device",
            UserProperties = new Dictionary<string, object>
            {
                ["premium"] = true,
                ["age"] = 25
            }
        };

        // Act
        var json = JsonSerializer.Serialize(user);
        var deserialized = JsonDocument.Parse(json);
        var root = deserialized.RootElement;

        // Assert
        Assert.Equal("test-user", root.GetProperty("user_id").GetString());
        Assert.Equal("test-device", root.GetProperty("device_id").GetString());
        Assert.True(root.GetProperty("user_properties").GetProperty("premium").GetBoolean());
    }

    [Fact]
    public void DeserializesFromSnakeCase()
    {
        // Arrange
        var json = """
        {
            "user_id": "test-user",
            "device_id": "test-device",
            "user_properties": {
                "premium": true
            }
        }
        """;

        // Act
        var user = JsonSerializer.Deserialize<ExperimentUser>(json);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("test-user", user.UserId);
        Assert.Equal("test-device", user.DeviceId);
        Assert.NotNull(user.UserProperties);
        Assert.True(user.UserProperties.ContainsKey("premium"));
    }

    [Fact]
    public void OmitsNullProperties()
    {
        // Arrange
        var user = new ExperimentUser
        {
            UserId = "test-user"
            // All other properties are null
        };

        // Act
        var json = JsonSerializer.Serialize(user, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        // Assert
        Assert.Contains("user_id", json);
        Assert.DoesNotContain("device_id", json);
        Assert.DoesNotContain("country", json);
    }
}
