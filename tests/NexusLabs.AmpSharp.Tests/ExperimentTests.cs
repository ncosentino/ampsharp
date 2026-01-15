using NexusLabs.AmpSharp;
using NexusLabs.AmpSharp.Models;

namespace NexusLabs.AmpSharp.Tests;

public class ExperimentTests
{
    [Fact]
    public void InitializeRemote_CreatesClient()
    {
        // Act
        var client = Experiment.InitializeRemote("test-key");

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void InitializeRemote_ThrowsOnNullKey()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Experiment.InitializeRemote(null!));
    }

    [Fact]
    public void InitializeRemote_ThrowsOnEmptyKey()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Experiment.InitializeRemote(""));
    }

    [Fact]
    public void InitializeRemote_AcceptsConfig()
    {
        // Arrange
        var config = new RemoteEvaluationConfig
        {
            ServerZone = ServerZone.EU,
            FetchTimeoutMillis = 5000
        };

        // Act
        var client = Experiment.InitializeRemote("test-key", config);

        // Assert
        Assert.NotNull(client);
    }
}
