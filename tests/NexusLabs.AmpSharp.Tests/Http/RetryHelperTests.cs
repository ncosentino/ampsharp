using NexusLabs.AmpSharp.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace NexusLabs.AmpSharp.Tests.Http;

public class RetryHelperTests
{
    [Fact]
    public void CalculateBackoffDelay_WithZeroAttempt_ReturnsMinDelay()
    {
        // Act
        var delay = RetryHelper.CalculateBackoffDelay(0, 500, 10000, 1.5);

        // Assert
        Assert.Equal(500, delay);
    }

    [Fact]
    public void CalculateBackoffDelay_WithMultipleAttempts_CalculatesExponential()
    {
        // Act
        var delay1 = RetryHelper.CalculateBackoffDelay(1, 500, 10000, 1.5);
        var delay2 = RetryHelper.CalculateBackoffDelay(2, 500, 10000, 1.5);
        var delay3 = RetryHelper.CalculateBackoffDelay(3, 500, 10000, 1.5);

        // Assert
        Assert.Equal(750, delay1);    // 500 * 1.5^1
        Assert.Equal(1125, delay2);   // 500 * 1.5^2
        Assert.Equal(1687, delay3);   // 500 * 1.5^3
    }

    [Fact]
    public void CalculateBackoffDelay_CapsAtMaxDelay()
    {
        // Act
        var delay = RetryHelper.CalculateBackoffDelay(20, 500, 10000, 1.5);

        // Assert
        Assert.Equal(10000, delay);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_SucceedsOnFirstAttempt()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var callCount = 0;
        Func<Task<string>> operation = () =>
        {
            callCount++;
            return Task.FromResult("success");
        };

        // Act
        var result = await RetryHelper.ExecuteWithRetryAsync(
            operation,
            maxRetries: 3,
            minDelayMillis: 100,
            maxDelayMillis: 1000,
            scalar: 1.5,
            logger.Object);

        // Assert
        Assert.Equal("success", result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_RetriesOn500Error()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var callCount = 0;
        Func<Task<string>> operation = () =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new HttpRequestException("Server error", null, System.Net.HttpStatusCode.InternalServerError);
            }
            return Task.FromResult("success");
        };

        // Act
        var result = await RetryHelper.ExecuteWithRetryAsync(
            operation,
            maxRetries: 3,
            minDelayMillis: 10,
            maxDelayMillis: 100,
            scalar: 1.5,
            logger.Object);

        // Assert
        Assert.Equal("success", result);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_RetriesOn429Error()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var callCount = 0;
        Func<Task<string>> operation = () =>
        {
            callCount++;
            if (callCount < 2)
            {
                throw new HttpRequestException("Rate limited", null, System.Net.HttpStatusCode.TooManyRequests);
            }
            return Task.FromResult("success");
        };

        // Act
        var result = await RetryHelper.ExecuteWithRetryAsync(
            operation,
            maxRetries: 3,
            minDelayMillis: 10,
            maxDelayMillis: 100,
            scalar: 1.5,
            logger.Object);

        // Assert
        Assert.Equal("success", result);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_DoesNotRetryOn400Error()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var callCount = 0;
        Func<Task<string>> operation = () =>
        {
            callCount++;
            throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            RetryHelper.ExecuteWithRetryAsync(
                operation,
                maxRetries: 3,
                minDelayMillis: 10,
                maxDelayMillis: 100,
                scalar: 1.5,
                logger.Object));

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_ThrowsAfterMaxRetries()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var callCount = 0;
        Func<Task<string>> operation = () =>
        {
            callCount++;
            throw new HttpRequestException("Server error", null, System.Net.HttpStatusCode.InternalServerError);
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            RetryHelper.ExecuteWithRetryAsync(
                operation,
                maxRetries: 2,
                minDelayMillis: 10,
                maxDelayMillis: 100,
                scalar: 1.5,
                logger.Object));

        Assert.Equal(3, callCount); // Initial attempt + 2 retries
    }
}
