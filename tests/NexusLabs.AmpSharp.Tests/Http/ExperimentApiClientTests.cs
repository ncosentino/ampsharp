using System.Net;
using System.Text.Json;
using NexusLabs.AmpSharp.Http;
using NexusLabs.AmpSharp.Logging;
using NexusLabs.AmpSharp.Models;
using Moq;
using Moq.Protected;

namespace NexusLabs.AmpSharp.Tests.Http;

public class ExperimentApiClientTests
{
    [Fact]
    public async Task FetchVariantsAsync_SendsCorrectRequest()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("""
            {
                "flag-1": {"key": "on", "value": "treatment"}
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
        var config = new RemoteEvaluationConfig();
        var apiClient = new ExperimentApiClient(httpClient, "test-key", config, NullLogger.Instance);

        var user = new ExperimentUser { UserId = "test-user", DeviceId = "test-device" };

        // Act
        var variants = await apiClient.FetchVariantsAsync(user);

        // Assert
        Assert.NotNull(variants);
        Assert.Single(variants);
        Assert.Equal("on", variants["flag-1"].Key);
        Assert.Equal("treatment", variants["flag-1"].Value);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.PathAndQuery.Contains("/v1/vardata") &&
                req.RequestUri.PathAndQuery.Contains("user_id=test-user") &&
                req.RequestUri.PathAndQuery.Contains("device_id=test-device")
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task FetchVariantsAsync_IncludesFlagKeysInQuery()
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
        var config = new RemoteEvaluationConfig();
        var apiClient = new ExperimentApiClient(httpClient, "test-key", config, NullLogger.Instance);

        var user = new ExperimentUser { UserId = "test-user" };
        var options = new FetchOptions { FlagKeys = new List<string> { "flag-1", "flag-2" } };

        // Act
        await apiClient.FetchVariantsAsync(user, options);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.PathAndQuery.Contains("flag_keys=flag-1%2Cflag-2")
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task FetchVariantsAsync_AddsNoTrackHeader()
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
        var config = new RemoteEvaluationConfig();
        var apiClient = new ExperimentApiClient(httpClient, "test-key", config, NullLogger.Instance);

        var user = new ExperimentUser { UserId = "test-user" };
        var options = new FetchOptions { TracksExposure = false, TracksAssignment = false };

        // Act
        await apiClient.FetchVariantsAsync(user, options);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Contains("X-Amp-Exp-Track") &&
                req.Headers.GetValues("X-Amp-Exp-Track").First() == "no-track"
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task FetchVariantsAsync_RetriesOn500Error()
    {
        // Arrange
        var callCount = 0;
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount < 3)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Content = new StringContent("Server error")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var config = new RemoteEvaluationConfig
        {
            FetchRetries = 5,
            FetchRetryBackoffMinMillis = 10,
            FetchRetryBackoffMaxMillis = 100
        };
        var apiClient = new ExperimentApiClient(httpClient, "test-key", config, NullLogger.Instance);

        var user = new ExperimentUser { UserId = "test-user" };

        // Act
        var variants = await apiClient.FetchVariantsAsync(user);

        // Assert
        Assert.NotNull(variants);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task FetchVariantsAsync_DoesNotRetryOn400Error()
    {
        // Arrange
        var callCount = 0;
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad request")
                };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var config = new RemoteEvaluationConfig { FetchRetries = 5 };
        var apiClient = new ExperimentApiClient(httpClient, "test-key", config, NullLogger.Instance);

        var user = new ExperimentUser { UserId = "test-user" };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            apiClient.FetchVariantsAsync(user));

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task FetchVariantsAsync_ReturnsEmptyDictionaryOnEmptyResponse()
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
        var config = new RemoteEvaluationConfig();
        var apiClient = new ExperimentApiClient(httpClient, "test-key", config, NullLogger.Instance);

        var user = new ExperimentUser { UserId = "test-user" };

        // Act
        var variants = await apiClient.FetchVariantsAsync(user);

        // Assert
        Assert.NotNull(variants);
        Assert.Empty(variants);
    }
}
