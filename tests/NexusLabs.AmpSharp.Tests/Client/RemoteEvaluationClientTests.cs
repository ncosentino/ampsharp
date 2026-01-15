using System.Net;
using NexusLabs.AmpSharp.Client;
using NexusLabs.AmpSharp.Models;
using Moq;
using Moq.Protected;

namespace NexusLabs.AmpSharp.Tests.Client;

public class RemoteEvaluationClientTests
{
    [Fact]
    public async Task FetchV2Async_InjectsLibraryContext()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{}")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new RemoteEvaluationClient(httpClient, "test-key");

        var user = new ExperimentUser { UserId = "test-user" };

        // Act
        await client.FetchV2Async(user);

        // Assert
        Assert.Equal("NexusLabs.AmpSharp/0.1.0", user.Library);
    }

    [Fact]
    public async Task FetchV2Async_ThrowsOnNullUser()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new RemoteEvaluationClient(httpClient, "test-key");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.FetchV2Async(null!));
    }

    [Fact]
    public async Task FetchV2Async_ReturnsVariants()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("""
            {
                "flag-1": {"key": "on"},
                "flag-2": {"key": "off"}
            }
            """)
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new RemoteEvaluationClient(httpClient, "test-key");

        var user = new ExperimentUser { UserId = "test-user" };

        // Act
        var variants = await client.FetchV2Async(user);

        // Assert
        Assert.NotNull(variants);
        Assert.Equal(2, variants.Count);
        Assert.Equal("on", variants["flag-1"].Key);
        Assert.Equal("off", variants["flag-2"].Key);
    }

    [Fact]
    public async Task FetchV2Async_PassesFetchOptions()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{}")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new RemoteEvaluationClient(httpClient, "test-key");

        var user = new ExperimentUser { UserId = "test-user" };
        var options = new FetchOptions { FlagKeys = new List<string> { "flag-1" } };

        // Act
        await client.FetchV2Async(user, options);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.PathAndQuery.Contains("flag_keys=flag-1")
            ),
            ItExpr.IsAny<CancellationToken>());
    }
}
