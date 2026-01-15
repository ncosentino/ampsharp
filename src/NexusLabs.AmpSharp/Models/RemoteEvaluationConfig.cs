using Microsoft.Extensions.Logging;

namespace NexusLabs.AmpSharp.Models;

/// <summary>
/// Configuration for remote evaluation client.
/// </summary>
public sealed class RemoteEvaluationConfig
{
    /// <summary>
    /// The server zone for data residency. Default is US.
    /// </summary>
    public ServerZone ServerZone { get; set; } = ServerZone.US;

    /// <summary>
    /// Custom server URL. If set, overrides ServerZone.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// Request timeout in milliseconds. Default is 10000.
    /// </summary>
    public int FetchTimeoutMillis { get; set; } = 10000;

    /// <summary>
    /// Number of retry attempts on failure. Default is 8.
    /// </summary>
    public int FetchRetries { get; set; } = 8;

    /// <summary>
    /// Minimum backoff delay in milliseconds. Default is 500.
    /// </summary>
    public int FetchRetryBackoffMinMillis { get; set; } = 500;

    /// <summary>
    /// Maximum backoff delay in milliseconds. Default is 10000.
    /// </summary>
    public int FetchRetryBackoffMaxMillis { get; set; } = 10000;

    /// <summary>
    /// Exponential backoff scalar. Default is 1.5.
    /// </summary>
    public double FetchRetryBackoffScalar { get; set; } = 1.5;

    /// <summary>
    /// Timeout for retry requests in milliseconds. Default is 10000.
    /// </summary>
    public int FetchRetryTimeoutMillis { get; set; } = 10000;

    /// <summary>
    /// Log level for the client. Default is Error.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Error;

    /// <summary>
    /// Custom logger. If not provided, uses NullLogger.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Gets the effective server URL based on ServerUrl or ServerZone.
    /// </summary>
    public string GetServerUrl()
    {
        if (!string.IsNullOrEmpty(ServerUrl))
        {
            return ServerUrl;
        }

        return ServerZone switch
        {
            ServerZone.EU => "https://api.lab.eu.amplitude.com",
            _ => "https://api.lab.amplitude.com"
        };
    }
}
