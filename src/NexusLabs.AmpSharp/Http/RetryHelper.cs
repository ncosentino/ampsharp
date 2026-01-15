using System.Net;
using Microsoft.Extensions.Logging;

namespace NexusLabs.AmpSharp.Http;

/// <summary>
/// Helper class for implementing exponential backoff retry logic.
/// </summary>
internal static class RetryHelper
{
    /// <summary>
    /// Executes an async operation with exponential backoff retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="minDelayMillis">Minimum backoff delay in milliseconds.</param>
    /// <param name="maxDelayMillis">Maximum backoff delay in milliseconds.</param>
    /// <param name="scalar">Exponential backoff scalar.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries,
        int minDelayMillis,
        int maxDelayMillis,
        double scalar,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt <= maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (IsRetryable(ex, out var statusCode))
            {
                lastException = ex;

                if (attempt >= maxRetries)
                {
                    logger.LogError(ex, "Request failed after {Attempts} attempts with status {StatusCode}",
                        attempt + 1, statusCode);
                    throw;
                }

                var delay = CalculateBackoffDelay(attempt, minDelayMillis, maxDelayMillis, scalar);
                logger.LogWarning("Request failed with status {StatusCode}. Retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})",
                    statusCode, delay, attempt + 1, maxRetries);

                await Task.Delay(delay, cancellationToken);
                attempt++;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "Request timed out");
                throw;
            }
            catch (Exception ex)
            {
                // Non-retryable exceptions
                logger.LogError(ex, "Request failed with non-retryable error");
                throw;
            }
        }

        throw lastException ?? new InvalidOperationException("Retry logic failed unexpectedly");
    }

    /// <summary>
    /// Calculates the backoff delay for the given attempt.
    /// </summary>
    /// <param name="attempt">The current attempt number (0-based).</param>
    /// <param name="minDelayMillis">Minimum delay in milliseconds.</param>
    /// <param name="maxDelayMillis">Maximum delay in milliseconds.</param>
    /// <param name="scalar">Exponential backoff scalar.</param>
    /// <returns>The delay in milliseconds.</returns>
    public static int CalculateBackoffDelay(int attempt, int minDelayMillis, int maxDelayMillis, double scalar)
    {
        var delay = minDelayMillis * Math.Pow(scalar, attempt);
        return (int)Math.Min(delay, maxDelayMillis);
    }

    /// <summary>
    /// Determines if an HTTP request exception is retryable.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <param name="statusCode">The HTTP status code if available.</param>
    /// <returns>True if the request should be retried, false otherwise.</returns>
    private static bool IsRetryable(HttpRequestException exception, out HttpStatusCode? statusCode)
    {
        statusCode = exception.StatusCode;

        if (statusCode == null)
        {
            // Network errors without status code are retryable
            return true;
        }

        // Retry on 5xx errors
        if ((int)statusCode >= 500 && (int)statusCode < 600)
        {
            return true;
        }

        // Retry on 429 (Too Many Requests)
        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return true;
        }

        // Do not retry on 4xx errors (except 429)
        return false;
    }
}
